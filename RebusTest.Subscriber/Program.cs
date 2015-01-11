using System;
using Rebus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Transports.Msmq;
using RebusTests.Messages;
using System.Threading;


namespace RebusTests.Subscriber
{
    class Program
    {
        static void Main()
        {
        

            using (var adapter = new BuiltinContainerAdapter())
            {

                Configure.With(adapter)
                         .Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
                         .Transport(t => t.UseMsmqAndGetInputQueueNameFromAppConfig())
                         .MessageOwnership(o => o.FromRebusConfigurationSection())
                         .CreateBus()
                         .Start();

                adapter.Register(() => new TestMessageHandler(adapter.Bus));

                adapter.Bus.Subscribe<TestMessage>();

                Console.WriteLine("Press ENTER to quit");
                Console.ReadLine();
            }
        }
    }

    class TestMessageHandler : IHandleMessages<TestMessage>
    {

        readonly IBus _bus;

        public TestMessageHandler(IBus bus)
        {
            _bus = bus;
        }
        
        public void Handle(TestMessage message)
        {
            Console.WriteLine("received message {0} for run {1}", message.Id, message.RunId);
            _bus.Send<Counter>(new Counter { RunId = message.RunId });
        }
    }
}
