using System.Collections.Generic;
using System.Web.Http;

namespace DistributionControllers
{
    [RoutePrefix("Distribution")]
    public class DistributionController : ApiController
    {
        [Route("getlist")]
        public List<string> GetList()
        {
            return  new List<string>()
            {
                "1233",
                "张三"
            };
        }
    }
}