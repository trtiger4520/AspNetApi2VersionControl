using System.Web.Http;
using Microsoft.Web.Http;

namespace VersionControl.Controllers
{
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    [ApiVersion("1.2")]
    [ApiVersion("2.0")]
    [RoutePrefix("api/booking")]
    public class BookingController : ApiController
    {
        [HttpGet]
        [Route("version")]
        [MapToApiVersion("1.0")]
        public string GetVersion() => "v1";
        [HttpGet]
        [Route("version")]
        [MapToApiVersion("1.1")]
        public string GetVersion1dot1() => "v1.1";
        [HttpGet]
        [Route("version")]
        [MapToApiVersion("1.2")]
        public string GetVersion1dot2() => "v1.2";
        [HttpGet]
        [Route("version")]
        [MapToApiVersion("2.0")]
        public string GetVersion2() => "v2";
    }
}
