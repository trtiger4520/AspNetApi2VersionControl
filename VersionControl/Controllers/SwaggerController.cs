using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Web.Http;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using NSwag.Generation.WebApi;

namespace VersionControl.Controllers
{
    [RoutePrefix("api/swagger/doc")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SwaggerController : ApiController
    {
        private readonly Assembly _appAssembly;

        // TODO: 快取功能，在服務重建之前使用相同的Document沒有問題
        // private static readonly IDictionary<string, OpenApiDocument> _docCache;

        public SwaggerController()
        {
            _appAssembly = Assembly.GetAssembly(typeof(WebApiApplication));
        }

        [HttpGet]
        [Route("v{version}/swagger.json", Name = nameof(DocumentAsync))]
        public async Task<HttpResponseMessage> DocumentAsync(string version)
        {
            // 嘗試轉換成ApiVersion
            var tryVersion = ApiVersion.TryParse(version, out ApiVersion spectifyVersion);
            if (!tryVersion)
                return ReturnNotFound();

            // 確認是否在已知的版本範圍 (使用ApiExplorer達成)
            var apiExplorer = Configuration.Services.GetApiExplorer();
            if (!apiExplorer.ApiDescriptions
                .Any(api => api.GetApiVersion() == spectifyVersion))
                return ReturnNotFound();

            // 產生OpenApiDocument
            var doc = await GetVersionDocAsync(spectifyVersion);

            return ReturnJson(doc.ToJson());
        }

        private static HttpResponseMessage ReturnNotFound()
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        private static HttpResponseMessage ReturnJson(string json)
        {
            var content = new StringContent(json);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = new HttpResponseMessage
            {
                Content = content,
                StatusCode = HttpStatusCode.OK,
            };

            return response;
        }

        private Task<OpenApiDocument> GetVersionDocAsync(ApiVersion version)
        {
            var stringVersion = version.ToString();
            var settings = new WebApiOpenApiDocumentGeneratorSettings
            {
                Version = stringVersion,
            };

            // 使用Nswag提供的ApiVersionProcessor來篩選指定版本
            //
            // 原始碼內容可看到，如果是AspNetCore會尋找套件設定，
            // 如果不是它會尋找ApiVersionAttribute與MapToApiVersionAttribute來確認版本
            // (IncludedVersions.Contains(version))
            //
            // (奇怪，官方文件看不到，還是我眼瞎)
            // https://github.com/RicoSuter/NSwag/blob/master/src/NSwag.Generation/Processors/ApiVersionProcessor.cs
            var versionProcess = new ApiVersionProcessor()
            {
                IncludedVersions = new string[] { stringVersion },
            };

            // 篩選出指定版本的API項目
            // 注意如果路徑相同，未指定版本的項目會與之衝突
            // API可以正常使用，但文件無法產生，Swagger亦無法瀏覽
            // 例如：
            // [HttpGet, Route("version")]
            // public string GetVersion() => "v1";
            // --
            // [HttpGet, Route("version"), ApiVersion("2.0")]
            // public string GetVersion() => "v2";
            settings.AddOperationFilter(versionProcess.Process);
            settings.OperationProcessors.Add(new AddVersionHeaderParameter(stringVersion));

            var generator = new WebApiOpenApiDocumentGenerator(settings);

            var controllers = WebApiOpenApiDocumentGenerator
                .GetControllerClasses(_appAssembly);

            return generator.GenerateForControllersAsync(controllers);
        }

    }

    // 自訂Swagger處理Header增加版本參數
    // 使用static保持使用相同的OpenApiParameter修改預設值
    // (或是只要在Operation.Parameters取出物件調整版本預設值即可)
    internal class AddVersionHeaderParameter : IOperationProcessor
    {
        private string _defaultVersion = "1.0";
        private static OpenApiParameter _apiVersionParameter;

        public string DefaultVersion { get => _defaultVersion; set => _defaultVersion = value; }
        public static OpenApiParameter ApiVersionParameter
        {
            get
            {
                if (_apiVersionParameter is null)
                {
                    _apiVersionParameter = new OpenApiParameter
                    {
                        Name = "x-ms-version",
                        Kind = OpenApiParameterKind.Header,
                        Type = NJsonSchema.JsonObjectType.String,
                        IsRequired = false,
                        Description = "header version",
                        Default = "1.0"
                    };
                }
                return _apiVersionParameter;
            }
        }

        public AddVersionHeaderParameter()
        {

        }

        public AddVersionHeaderParameter(string defaultVersion)
        {
            _defaultVersion = defaultVersion;
        }

        public bool Process(OperationProcessorContext context)
        {
            ApiVersionParameter.Default = DefaultVersion;

            if (!context.OperationDescription.Operation.Parameters.Contains(ApiVersionParameter))
                context.OperationDescription.Operation.Parameters.Add(ApiVersionParameter);

            return true;
        }
    }
}
