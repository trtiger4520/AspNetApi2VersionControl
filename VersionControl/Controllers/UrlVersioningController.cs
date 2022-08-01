using System.Web.Http;
using Microsoft.Web.Http;

namespace VersionControl.Controllers
{
    [RoutePrefix("api/urlversioning")]
    public class UrlVersioningController : ApiController
    {
        [HttpGet]
        [Route("version")]
        [ApiVersion("1.0")]
        public string GetVersion() => "v1";

        [HttpGet]
        [Route("qwert")]
        public string GetString() => "qwert";
    }


    [RoutePrefix("api/v{version:apiVersion}/urlversioning")]
    public class UrlVersioningV2Controller : ApiController
    {
        [HttpGet]
        [Route("version")]
        [ApiVersion("1.2")]
        public string GetVersion() => "v1.2";
    }
}
