using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.Web.Http;
using Microsoft.Web.Http.Routing;
using Microsoft.Web.Http.Versioning;

namespace VersionControl
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var constraintResolver = new DefaultInlineConstraintResolver();

            // 使用網址指定版本時，註冊自訂路由規則
            // 可使RouteAttrbute設定時認出apiVersion規則
            // 例如：[Route(api/v{version:apiVersion}/{controller})]
            constraintResolver.ConstraintMap.Add(
                "apiVersion", typeof(ApiVersionRouteConstraint));

            config.MapHttpAttributeRoutes(constraintResolver);

            // TODO: 確認使用類型，看是用..
            //  - 網址 /api/v1/controller
            //  - RequestHeader x-ms-version: 1.0
            //  - QueryString /api/controller?api-version=1.0
            // 都可自訂參數名稱
            config.AddApiVersioning(option =>
            {
                // 預設版本(未指定時) [ApiVersion()]
                option.DefaultApiVersion = new ApiVersion(1, 0);

                // 未套用ApiVersion的API自動套用預設版本(上面設定的版本：DefaultApiVersion)
                option.AssumeDefaultVersionWhenUnspecified = true;

                // 於Response的Header內提供可支援的版本資訊
                option.ReportApiVersions = true;

                // Query String
                //option.ApiVersionReader = new QueryStringApiVersionReader
                //{
                //    ParameterNames = { "api-version", "x-ms-version" }
                //};

                // Url版本控制(好像不需要也支援的樣子?!)
                //option.ApiVersionReader = new UrlSegmentApiVersionReader();

                // 接收Header版本控制
                option.ApiVersionReader = new HeaderApiVersionReader("x-ms-version");
            });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }
    }
}
