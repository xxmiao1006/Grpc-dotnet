using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace StreamTest.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {


            _logger = logger;
        }



        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {


            //Console.WriteLine("context.Peer：" + context.Peer);
            //Console.WriteLine("context.Host：" + context.Host);
            //Console.WriteLine("context.Method：" + context.Method);
            //Console.WriteLine("context.RequestHeaders：" + context.RequestHeaders);
            //Console.WriteLine("context.ResponseTrailers：" + context.ResponseTrailers);
            //Console.WriteLine("context.WriteOptions：" + context.WriteOptions);
            //Console.WriteLine("context.Status：" + context.Status);
            //Console.WriteLine("context.UserState：" + context.UserState);

            Console.WriteLine(request.Name + "--请求睡眠开始：" + request.Sleep);

            Thread.Sleep(request.Sleep);

            Console.WriteLine(request.Name + "--请求睡眠完成：" + request.Sleep);

            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + context.Peer
            });
        }

        public override async Task<SaveAllReply> SaveAll(IAsyncStreamReader<SaveAllRequest> requestStream, ServerCallContext context)
        {
            var userList = new List<user>();
            //while (await requestStream.MoveNext())
            //{
            //    userList.Add(requestStream.Current.User);
            //}

            await foreach (var saveAllRequest in requestStream.ReadAllAsync())
            {
                userList.Add(saveAllRequest.User);
            }

            foreach (var user in userList)
            {
                Console.Out.WriteLine("user = {0}", user);
            }

            return await Task.FromResult(new SaveAllReply { ReturnCode = 200, ReturnMessage = "success" });
        }

        public override async Task GetAll(GetAllRequest request, IServerStreamWriter<GetAllReply> responseStream, ServerCallContext context)
        {
            var data = new List<user>();
            for (int i = 0; i < 4; i++)
            {

                data.Add(new user
                {
                    UserName = "responseStream",
                    Age = i
                });
            }


            if (request.Age > 0)
            {
                data = data.Where(x => x.Age == request.Age).ToList();
            }

            foreach (var user in data)
            {
                await responseStream.WriteAsync(new GetAllReply { User = user });
            }
        }

        public override async Task ReadySetGo(IAsyncStreamReader<RaceMessage> requestStream, IServerStreamWriter<RaceMessage> responseStream, ServerCallContext context)
        {
            var raceDuration = TimeSpan.Parse(context.RequestHeaders.Single(x => x.Key == "race-duration").Value);

            RaceMessage? lastMessageReceived = null;

            var readTask = Task.Run(async () =>
            {
                await foreach (var message in requestStream.ReadAllAsync())
                {
                    lastMessageReceived = message;
                }
            });

            var sw = Stopwatch.StartNew();
            var sent = 0;
            while (sw.Elapsed < raceDuration)
            {
                await responseStream.WriteAsync(new RaceMessage { Count = ++sent });
            }

            await readTask;
        }
    }
}
