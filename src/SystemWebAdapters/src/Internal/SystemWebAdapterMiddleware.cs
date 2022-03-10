// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace System.Web.Internal
{
    internal class SystemWebAdapterMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContextCore context, RequestDelegate next)
        {
            var options = context.GetSystemWebMetadata();

            if (options is not null && options.Enabled)
            {
                return SetupSystemWebAdapterAsync(options, context, next);
            }

            return next(context);
        }

        private static async Task SetupSystemWebAdapterAsync(SystemWebAdapterAttribute options, HttpContextCore context, RequestDelegate next)
        {
            await BufferRequestStreamAsync(options, context);

            await next(context);
        }

        /// <summary>
        /// Set up input stream to be fully buffered so calls such as `.Length` work as in ASP.NET Framework
        /// </summary>
        private static async ValueTask BufferRequestStreamAsync(SystemWebAdapterAttribute options, HttpContextCore context)
        {
            if (!options.BufferRequestStream)
            {
                return;
            }

            context.Request.EnableBuffering();
            await context.Request.Body.DrainAsync(context.RequestAborted);
            context.Request.Body.Position = 0;
        }
    }
}
