using System;

namespace ClientServerConnector
{
    public class Connector
    {
        public Connector()
        {

        }

        public static void Test()
        {
            GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
            {
                HttpHandler = new WinHttpHandler()
            });

            var client = new Greeter.GreeterClient(channel);
            String user = ".NET";

            var reply = client.SayHello(new HelloRequest { Name = user });
            Console.WriteLine("Greeting: " + reply.Message);

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
