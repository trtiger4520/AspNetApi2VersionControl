using System.Web.Http;
using Microsoft.Owin;
using NSwag.AspNet.Owin;
using Owin;

[assembly: OwinStartup(typeof(VersionControl.Startup))]

namespace VersionControl
{
    public partial class Startup
    {
        private readonly HttpConfiguration _config;

        public Startup()
        {
            _config = GlobalConfiguration.Configuration;
        }

        public void Configuration(IAppBuilder app)
        {
            // ApiExplorer
            // 註冊Versioning套件所提供的整體版本化資訊
            // 可取得當前所有版本、Controller...等其他內容
            var apiExplorer = _config.AddVersionedApiExplorer(option =>
            {
                option.GroupNameFormat = "'v'VVV";
                option.SubstituteApiVersionInUrl = true;
            });

            // Nswag此處不提供整的WebApiApplication的Assembly資訊來產生OpenApiDocument，
            // 改由自訂API位置提供內容
            app.UseSwaggerUi3(settings =>
            {
                // 從ApiExplorer取得所有版本作為名稱並指定至自訂路由去
                // 可查看SwaggerController.DocumentAsync
                foreach (var api in apiExplorer.ApiDescriptions)
                {
                    var versionString = api.ApiVersion.ToString();
                    settings.SwaggerRoutes.Add(new SwaggerUi3Route(
                        $"v{versionString}",
                        $"api/swagger/doc/v{versionString}/swagger.json"));
                }

                // OpenApiDocument來源擴充
                // 增加指定路徑與命名，顯示於Swagger右上選單
                // Ex:
                //settings.SwaggerRoutes.Add(
                //    new SwaggerUi3Route("v1", "api/swagger/doc/v1/swagger.json"));
                //settings.SwaggerRoutes.Add(
                //    new SwaggerUi3Route("v2", "api/swagger/doc/v2/swagger.json"));

                settings.PostProcess = document =>
                {
                    document.Info.Title = "Version Control API";
                    document.Info.Description = "Version Control API";
                    document.Info.Version = "v1.0";
                };

            });
        }
    }

}
