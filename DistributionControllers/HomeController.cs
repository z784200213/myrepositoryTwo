using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DataObject;
using OrderServiceContact;

namespace DistributionControllers
{
    [RoutePrefix("HomeTest1")]
    public class HomeTestController : ApiController
    {
        private readonly IOrderService _service;

        public HomeTestController(IOrderService service)
        {

            _service = service;
        }
        static readonly List<Product> ModelList = new List<Product>()
        {
            new Product(){Id=1,Name="电脑",Description="电器"},
            new Product(){Id=2,Name="冰箱",Description="电器"},
        };
        [Route("GetAlls")]
        //获取所有数据
        [HttpGet]
        public List<Product> GetAll()
        {
            var requsetheaders = Request.Headers;
            _service.Test();

            return ModelList;
        }

        //获取一条数据
        [HttpGet]
        public Product GetOne(int id)
        {
            return ModelList.FirstOrDefault(p => p.Id == id);
        }

        //新增
        [HttpPost]
        public bool PostNew(Product model)
        {
            ModelList.Add(model);
            return true;
        }

        //删除
        [HttpDelete]
        public bool Delete(int id)
        {
            return ModelList.Remove(ModelList.Find(p => p.Id == id));
        }

        //更新
        [HttpPut]
        public bool PutOne(Product model)
        {
            Product editModel = ModelList.Find(p => p.Id == model.Id);
            editModel.Name = model.Name;
            editModel.Description = model.Description;
            return true;
        }
      
    }
}