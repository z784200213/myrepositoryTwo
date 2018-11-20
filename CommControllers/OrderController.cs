using System.Web.Http;

namespace CommControllers
{
    [RoutePrefix("Order")]
    public class OrderController:ApiController
    {
        [Route("GetSring"),HttpGet]
        public string GetString()
        {
            return "orderController返回";
        }
    }
}