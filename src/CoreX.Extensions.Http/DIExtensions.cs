﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DIExtensions
    {
        /// <summary>
        /// A utility extension that enables logging for Bad Request (400) responses as warnings, which by default are not logged. 
        /// This is not specific to the HttpLogger and can work with other logging providers.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection EnableLoggingForBadRequests(this IServiceCollection services)
        {
            services.PostConfigure<ApiBehaviorOptions>(options =>
            {
                var builtInFactory = options.InvalidModelStateResponseFactory;

                options.InvalidModelStateResponseFactory = context =>
                {
                    var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger(context.ActionDescriptor.DisplayName);

                    var builder = new StringBuilder();
                    builder.AppendLine("BadRequest: One or more validation errors occurred.");
                    foreach (var modelState in context.ModelState)
                    {
                        foreach (var error in modelState.Value.Errors)
                        {
                            builder.AppendLine($"{modelState.Key}: {error.ErrorMessage}");
                        }
                    }

                    builder.AppendLine(context.HttpContext.Request.ToHtml(showHeaders: false, showBody: true));

                    logger.LogWarning(builder.ToString());
                    return builtInFactory(context);
                };
            });

            return services;
        }
    }
}
