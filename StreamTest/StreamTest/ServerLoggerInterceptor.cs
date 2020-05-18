using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StreamTest
{
    public class ServerLoggerInterceptor : Interceptor
    {
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var headers = context.RequestHeaders.ToList();
            foreach (var head in headers)
            {
                Console.WriteLine("请求头key: " + head.Key + "    请求头value: " + head.Value);
            }

            DateTime now = DateTime.Now;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = continuation(request, context);

            stopwatch.Stop();
            
            return result;
        }
    }
}
