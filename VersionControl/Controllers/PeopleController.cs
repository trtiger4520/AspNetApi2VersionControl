using System.Web.Http;

namespace VersionControl.Controllers
{
    [RoutePrefix("api/people")]
    public class PeopleController : ApiController
    {
        // GET ~/people
        // GET ~/people?api-version=1.0
        [Route]
        public IHttpActionResult Get() => Ok(new[] { new Person() });
    }

    internal class Person
    {
        public Person()
        {
        }
    }

}
