using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcService;
namespace Test_gRPC.NetFrameworkClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //Channel channel = new Channel("127.0.0.1:5001", ChannelCredentials.Insecure);
            Channel channel = new Channel("localhost", 5001, ChannelCredentials.Insecure);

            var client = new Greeter.GreeterClient(channel);
            String user = "mags";

            var reply = client.SayHello(new HelloRequest { Name = user });
            Console.WriteLine("Greeting: " + reply.Message);

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
