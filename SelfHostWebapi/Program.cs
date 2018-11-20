using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.SelfHost;
using Common;
using Newtonsoft.Json;

namespace SelfHostWebapi
{
    class Program
    {
        static void Main(string[] args)
        {
           
        
            var config = new HttpSelfHostConfiguration("http://localhost:5000"); //配置主机
            //config.Routes.MapHttpRoute(    //配置路由
            //    "API Default", "api/{controller}/{id}",
            //    new { id = RouteParameter.Optional });
            config.UseJson().UseHttpRouteAttribute().LoadAssemby()
                //.UseFilters(new ApiAuthFilter())
                .Run(); 
                      
            using (HttpSelfHostServer server = new HttpSelfHostServer(config)) //监听HTTP
            {
                server.OpenAsync().Wait(); //开启来自客户端的请求
                Console.WriteLine("Press Enter to quit");
                Console.ReadLine();
            }
            //Assembly.Load("WebApi,V")
        }

        public static async void Process()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress =new Uri("http://localhost:5000");
            HttpResponseMessage message = await client.GetAsync("HomeTest/GetAlls");
            var obj = message.Content.ReadAsAsync<object>();
        }


    }
    internal sealed class ApiAuthFilter : AuthorizationFilterAttribute
    {
        private const string INTERNAL_USER_CODE = "internalusercode";
        private const string INTERNAL_USER_NAME = "internalusername";
        private const string INTERNAL_AGENT = "internalcurrentorg";
        private const string INTERNAL_SUPPLIERS = "internalsuppliers";
        private const string INTERNAL_AUTHORITY = "internalauthority";
        private const string INTERNAL_HOSPITALS = "internalhospitals";

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (SkipAuthorization(actionContext))
            {
                return;
            }
            string message = null;
            //if (!this.IsAuthorized(actionContext, ref message))
            //{
            //    this.HandleUnauthorizedRequest(actionContext, message);
            //}
        }

        //private bool IsAuthorized(HttpActionContext actionContext, ref string message)
        //{
        //    PrintAllHeaders(actionContext);
        //    var user = ParseHeader(actionContext);
        //    if (!Valid(user, ref message))
        //    {
        //        return false;
        //    }
        //    WebContext.SetUser(user);
        //    return true;
        //}
        //private bool Valid(UserInfo user, ref string message)
        //{
        //    if (user == null)
        //    {
        //        message = "没有用户信息。";
        //        return false;
        //    }
        //    if (user.Name.IsNullOrEmptyOrWhiteSpace())
        //    {
        //        message = "用户名称不正确。";
        //        return false;
        //    }
        //    if (user.Code.IsNullOrEmptyOrWhiteSpace())
        //    {
        //        message = "用户代码不正确。";
        //        return false;
        //    }
        //    if (user.Authority.IsNullOrEmpty())
        //    {
        //        message = "用户没有任何权限。";
        //        return false;
        //    }

        //    if (!user.IsSuperAdmin)
        //    {
        //        if (user.AgentName.IsNullOrEmptyOrWhiteSpace())
        //        {
        //            message = "用户机构名称不正确。";
        //            return false;
        //        }
        //        if (user.AgentCode.IsNullOrEmptyOrWhiteSpace())
        //        {
        //            message = "用户机构代码不正确。";
        //            return false;
        //        }
        //    }

        //    return true;
        //}
        //private void HandleUnauthorizedRequest(HttpActionContext actionContext, string message)
        //{
        //    var info = "身份验证失败。";
        //    if (!string.IsNullOrWhiteSpace(message))
        //    {
        //        info = "身份验证失败：" + message;
        //    }
        //    var result = Result.Fail(info, -4);
        //    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, result);
        //    actionContext.Response.Headers.Add("contentType", "application/json");
        //}


        private IDictionary<string, string> GetHeaderDictionary(HttpRequestHeaders requestHeader, string headerName)
        {
            var headerValues = GetHeaderValue(requestHeader, headerName);
            if (string.IsNullOrWhiteSpace(headerValues))
            {
                return new Dictionary<string, string>();
            }
            else
            {
                return JsonConvert.DeserializeObject<IDictionary<string, string>>(headerValues);
            }
        }

        private string GetHeaderValue(HttpRequestHeaders headers, string headerName)
        {
            if (headers.Contains(headerName))
            {
                var headerValues = headers.GetValues(headerName).FirstOrDefault();
                return System.Web.HttpUtility.UrlDecode(headerValues);
            }
            return null;
        }

        private static bool SkipAuthorization(HttpActionContext actionContext)
        {
            if (!actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any())
            {
                return actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
            }
            return true;
        }

        [Conditional("DEBUG")]
        private static void PrintAllHeaders(HttpActionContext actionContext)
        {
            var requestHeader = actionContext.Request.Headers;
            var str = string.Empty;
            foreach (var header in requestHeader)
            {
                str += ";" + header.Key + "=" + string.Join(",", header.Value);
            }

            str = str.Substring(1);

        }
    }
    public class AuthFilter : AuthorizationFilterAttribute
    {
        
    }

    public class TypeFinder : BaseFinder
    {
        protected override Assembly[] FideAssembly()
        {
            Assembly[] array = BuildManager.GetReferencedAssemblies().OfType<Assembly>().ToArray<Assembly>();
            List<Assembly> assemblyList = new List<Assembly>();
            foreach (Assembly assembly in array)
            {
                if (!this.AssemblyExcludeRegex.IsMatch(assembly.FullName))
                    assemblyList.Add(assembly);
            }
            return assemblyList.ToArray();
        }
    }

    public abstract class BaseFinder
    {
        protected readonly Regex AssemblyExcludeRegex;
        private readonly Lazy<Assembly[]> allAssemblies;
        private const string ASSEMBLY_EXCLUDE_PATTERN = "^System|^mscorlib|^Microsoft|^CppCodeProvider|^VJSharpCodeProvider|^WebDev\r\n                                                        |^Nuget|^Castle|^Iesi|^log4net|^Autofac|^AutoMapper|^EntityFramework|^EPPlus|^nunit\r\n                                                        |^TestDriven|^MbUnit|^Rhino|^QuickGraph|^TestFu|^Telerik|^Antlr3|^Recaptcha|^FluentValidation\r\n                                                        |^ImageResizer|^itextsharp|^MiniProfiler|^Newtonsoft|^Pandora|^WebGrease|^Noesis|^DotNetOpenAuth\r\n                                                        |^Facebook|^LinqToTwitter|^PerceptiveMCAPI|^CookComputing|^GCheckout|^Mono\\.Math|^Org\\.Mentalis\r\n                                                        |^App_Web|^BundleTransformer|^ClearScript|^JavaScriptEngineSwitcher|^MsieJavaScriptEngine|^Glimpse\r\n                                                        |^Ionic|^App_GlobalResources|^AjaxMin|^MaxMind|^NReco|^OffAmazonPayments|^UAParser";

        public Assembly[] AllAssemblies
        {
            get
            {
                return this.allAssemblies.Value;
            }
        }

        protected BaseFinder()
        {
            this.AssemblyExcludeRegex = new Regex("^System|^mscorlib|^Microsoft|^CppCodeProvider|^VJSharpCodeProvider|^WebDev\r\n                                                        |^Nuget|^Castle|^Iesi|^log4net|^Autofac|^AutoMapper|^EntityFramework|^EPPlus|^nunit\r\n                                                        |^TestDriven|^MbUnit|^Rhino|^QuickGraph|^TestFu|^Telerik|^Antlr3|^Recaptcha|^FluentValidation\r\n                                                        |^ImageResizer|^itextsharp|^MiniProfiler|^Newtonsoft|^Pandora|^WebGrease|^Noesis|^DotNetOpenAuth\r\n                                                        |^Facebook|^LinqToTwitter|^PerceptiveMCAPI|^CookComputing|^GCheckout|^Mono\\.Math|^Org\\.Mentalis\r\n                                                        |^App_Web|^BundleTransformer|^ClearScript|^JavaScriptEngineSwitcher|^MsieJavaScriptEngine|^Glimpse\r\n                                                        |^Ionic|^App_GlobalResources|^AjaxMin|^MaxMind|^NReco|^OffAmazonPayments|^UAParser", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
            this.allAssemblies = new Lazy<Assembly[]>(new Func<Assembly[]>(this.FideAssembly));
        }

        protected abstract Assembly[] FideAssembly();

        public Type[] FindTypes(Func<Type, bool> filter)
        {
            List<Type> typeList = new List<Type>();
            foreach (Assembly allAssembly in this.AllAssemblies)
            {
                try
                {
                    Type[] types = allAssembly.GetTypes();
                    if (filter != null)
                        typeList.AddRange(((IEnumerable<Type>)types).Where<Type>(filter));
                    else
                        typeList.AddRange((IEnumerable<Type>)types);
                }
                catch (Exception ex)
                {
                }
            }
            return typeList.ToArray();
        }
    }
    public class TestHandle : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write("项目启用成功");
        }

        public bool IsReusable { get; }
    }
    public class TestHandle1 : HttpMessageHandler
    {

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
