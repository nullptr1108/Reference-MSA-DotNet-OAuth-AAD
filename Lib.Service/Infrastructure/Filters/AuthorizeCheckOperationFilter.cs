using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lib_Service.Infrastructure.Filters
{
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            // Check for authorize attribute
            //var hasAuthorize = context.ApiDescription.ControllerAttributes().OfType<AuthorizeAttribute>().Any() ||
            //                   context.ApiDescription.ActionAttributes().OfType<AuthorizeAttribute>().Any();

            //if (hasAuthorize)
            //{
            //    operation.Responses.Add("401", new Response { Description = "Unauthorized" });
            //    operation.Responses.Add("403", new Response { Description = "Forbidden" });

            //    operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
            //    operation.Security.Add(new Dictionary<string, IEnumerable<string>>
            //    {
            //        { "oauth2", new [] { "basketapi" } }
            //    });
            //}


            var found = false;
            if (context.ApiDescription.ActionDescriptor is
                Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor controller)
            {
                found = controller.ControllerTypeInfo.GetCustomAttributes().OfType<AuthorizeAttribute>().Any();
            }

            if (!found)
            {
                if (!context.ApiDescription.TryGetMethodInfo(out var mi)) return;
                if (!mi.GetCustomAttributes().OfType<AuthorizeAttribute>().Any()) return;
            }

            operation.Responses.Add("401", new Response {Description = "Unauthorized"});
            operation.Responses.Add("403", new Response {Description = "Forbidden"});

            operation.Security = new List<IDictionary<string, IEnumerable<string>>>
            {
                new Dictionary<string, IEnumerable<string>>
                {
                    {"oauth2", new[] {"basketapi"}}
                }
            };
        }
    }
}