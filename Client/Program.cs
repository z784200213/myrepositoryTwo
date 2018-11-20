using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DataObject;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Process();
            Console.ReadKey();
        }
        public static async void Process()
        {
            HttpClient client = new HttpClient {BaseAddress = new Uri("http://localhost:5000")};
            client.DefaultRequestHeaders.Add("Test","1234");
            HttpResponseMessage message = await client.GetAsync("HomeTest/GetAlls");
            var obj =await message.Content.ReadAsAsync<List<Product>>();
           
        }
    }
}
