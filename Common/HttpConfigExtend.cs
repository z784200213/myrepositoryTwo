using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;
using Autofac.Integration.WebApi;
using Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Common
{
    public static class HttpConfigExtend
    {
        public static List<Assembly> Assemblies { get; private set; }
        public static IComponentContext Context { get; private set; }

        public static HttpConfiguration UseJson(this HttpConfiguration config, string dateFormating = "yyyy-MM-dd HH:mm:ss")
        {
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            ((DefaultContractResolver)config.Formatters.JsonFormatter.SerializerSettings.ContractResolver).IgnoreSerializableAttribute = true;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.DateFormatString = dateFormating;
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.MediaTypeMappings.Add((MediaTypeMapping)new QueryStringMapping("format", "json", "application/json"));
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            return config;
        }

        public static HttpConfiguration UseHttpRouteAttribute(this HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            return config;
        }
        public static HttpConfiguration UseFilters(this HttpConfiguration config, params IFilter[] filters)
        {
            foreach (IFilter filter in filters)
                config.Filters.Add(filter);
            return config;
        }
        public static HttpConfiguration LoadAssemby(this HttpConfiguration config)
        {                   
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var files = Directory.GetFiles(dir, "*.dll");           
            List<Assembly>listasses=new List<Assembly>();
            foreach (var file in files)
            {
                var  ase= Assembly.LoadFile(file);
                
                listasses.Add(ase);
            }
            Assemblies = listasses;  
            return config;
        }

        public static void Run(this HttpConfiguration config)
        {
            var builder = new ContainerBuilder();
            RegisterSubType<IBaseService>(builder)?.AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            RegisterSubType<IBaseResponstory>(builder)?.AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            builder.RegisterApiControllers(Assemblies.ToArray());
            IContainer container=   builder.Build();
             Context = container.Resolve<IComponentContext>();
         
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            
        }

        private static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterSubType<T>( ContainerBuilder builder)
        {
            var types = FindTypes(x => typeof(T).IsAssignableFrom(x) && x != typeof(T) && x.IsClass && !x.IsAbstract);
            if (types.Any())
            {
                return builder.RegisterTypes(types);
            }
            return null;
        }
        public static Type[] FindTypes(Func<Type, bool> filter)
        {
            var list = new List<Type>();
            foreach (var assembly in Assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    list.AddRange(filter != null ? types.Where(filter) : types);
                }
                catch (Exception e)
                {
                   
                }
            }

            return list.ToArray();
        }

    }
    internal class ApiErrorHandler : ExceptionHandler
    {
        public override Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            return Task.Run((Action)(() =>
            {
                Exception exception = context.Exception;
                Result result;
                if (exception is SerializationException && exception.Message.Contains("KnownTypeAttribute") || 
                exception.InnerException is InvalidOperationException && 
                exception.InnerException.Message.Contains("XmlInclude") 
                && exception.InnerException.Message.Contains("SoapInclude"))
                {
                     result =Result.Fail(ex:exception);
                    
                }
                else
                {
                
                    string message = string.Format("URL:{0}", (object)context.Request.RequestUri);
                   
                }
                context.Result = (IHttpActionResult)new ResponseMessageResult(context.Request.CreateResponse<Result>(
                    HttpStatusCode.OK, Result.Success()));
            }), cancellationToken);
        }
    }
}