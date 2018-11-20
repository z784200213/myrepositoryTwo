using Core;
using OrderServiceContact;

namespace OrderServices
{
    public class OrderService: IOrderService
    {
        private IBaseResponstory _responstory;
        public OrderService(IBaseResponstory responstory)
        {
            _responstory = responstory;
        }

        public string Test()
        {
          return _responstory.responstory();
        }
    }
}