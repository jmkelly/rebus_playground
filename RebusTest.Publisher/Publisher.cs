using System;
using Rebus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Transports.Msmq;
using System.IO;
using RebusTests.Messages;
using System.Diagnostics;
using Rebus.PostgreSql;


namespace RebusTests.Publisher
{
    class Publisher
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=localhost;Database=rebus;User=test;Password=test;";
            using (var adapter = new BuiltinContainerAdapter())
            {
                Configure.With(adapter)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
                         .Transport(t => t.UseMsmqAndGetInputQueueNameFromAppConfig())
                         //.Subscriptions(c => c.StoreInXmlFile("C:\\temp\\publisher_subscriptions.xml"))
                         .Subscriptions(c => c.StoreInPostgreSql(connectionString,"publisher_subscription_store").EnsureTableIsCreated())
                         .CreateBus()
                         .Start(10);

                adapter.Register<MessageFromWebHandler>(() => new MessageFromWebHandler(adapter.Bus));

                while (true)
                {
                    Guid RunId = Guid.NewGuid();
                    Console.WriteLine(@"Publisher / Web Handler.  Can publish from itself, as well as handle messages sent from Web endpoint.  Publish how many messages?");

                    int numberOfMessages = Convert.ToInt32(Console.ReadLine());
                    
                    adapter.Bus.Publish(new RunStarted { Id = RunId, StartedAt = DateTime.Now, NumberOfMessages=numberOfMessages });

                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    for (int i = 0; i < numberOfMessages; i++)
                    {
                        if (i % 1000 == 0)
                        {
                            Console.WriteLine(string.Format("{0} Messages Published", i));
                        }
                        adapter.Bus.Publish(new TestMessage { Id = Guid.NewGuid(), Description = string.Format("Test message {0}", i),RunId=RunId });
                    }
                    sw.Stop();
                    Console.WriteLine(string.Format("{0} messages published in {1:1}s ", numberOfMessages,sw.Elapsed.TotalSeconds));
                    Console.WriteLine(string.Format("{0:0} rate msg/s", Convert.ToDouble(numberOfMessages) / sw.Elapsed.TotalSeconds));
                }
            }
        }

        public class MessageFromWebHandler : IHandleMessages<MessageFromWeb>
        {

            public readonly IBus _bus;
            public MessageFromWebHandler(IBus bus)
            {
                _bus = bus;
            }

            public void Handle(MessageFromWeb message)
            {
                _bus.Publish(new TestMessage { Id = Guid.NewGuid(), Description = "Web Message", RunId = message.RunId });
            }
        }
    }
}
