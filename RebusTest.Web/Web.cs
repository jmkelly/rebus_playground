using System;
using Rebus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Transports.Msmq;
using Rebus.PostgreSql;
using RebusTests.Messages;
using System.Diagnostics;


namespace RebusTest.Web
{
    public class Web
    {
        static void Main()
        {

            string connectionString = "Server=localhost;Database=rebus;User=test;Password=test;";
            using (var adapter = new BuiltinContainerAdapter())
            {
                Configure.With(adapter)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
                         .Transport(t => t.UseMsmqAndGetInputQueueNameFromAppConfig())
                         .MessageOwnership(c => c.FromRebusConfigurationSection())
                         //.Subscriptions(c => c.StoreInXmlFile("C:\\temp\\web_subscriptions.xml"))
                         .Subscriptions(c => c.StoreInPostgreSql(connectionString, "publisher_subscription_store").EnsureTableIsCreated())
                         .CreateBus()
                         .Start(10);


                while (true)
                {
                    Guid RunId = Guid.NewGuid();
                    Console.WriteLine(@"Imitation web server. Send how many messages?");

                    int numberOfMessages = Convert.ToInt32(Console.ReadLine());

                    adapter.Bus.Publish(new RunStarted { Id = RunId, StartedAt = DateTime.Now, NumberOfMessages = numberOfMessages });

                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    for (int i = 0; i < numberOfMessages; i++)
                    {
                        if (i % 1000 == 0)
                        {
                            Console.WriteLine(string.Format("{0} Messages Sent", i));
                        }
                        adapter.Bus.Send(new MessageFromWeb{ Id = Guid.NewGuid(), RunId = RunId, StartedAt = DateTime.Now });
                    }
                    sw.Stop();
                    Console.WriteLine(string.Format("{0} messages sent in {1:1}s ", numberOfMessages, sw.Elapsed.TotalSeconds));
                    Console.WriteLine(string.Format("{0:0} rate msg/s", Convert.ToDouble(numberOfMessages) / sw.Elapsed.TotalSeconds));

                }


            }
        }
    }
}
    
