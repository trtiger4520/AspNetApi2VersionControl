using System.Web.Http;
using Microsoft.Web.Http;

namespace VersionControl.Controllers
{
    [RoutePrefix("api/order")]
    public class OrderController : ApiController
    {
        [HttpGet]
        [Route("version")]
        [ApiVersion("1.0")]
        public string GetVersion() => "v1";
        [HttpGet]
        [Route("version")]
        [ApiVersion("1.5")]
        public string GetVersion1dot2() => "v1.5";
    }
}
