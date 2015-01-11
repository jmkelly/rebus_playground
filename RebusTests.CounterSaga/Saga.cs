using Rebus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Transports.Msmq;
using System;
using RebusTests.Messages;
using Rebus.PostgreSql;


namespace RebusTests.CounterSaga
{
    class Saga
    {
        static void Main()
        {
            string connectionString = "Server=localhost;Database=rebus;User=test;Password=test;";

            using (var adapter = new BuiltinContainerAdapter())
            {

                Configure.With(adapter)
                         .Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
                         .Transport(t => t.UseMsmqAndGetInputQueueNameFromAppConfig())
                         .Subscriptions(c => c.StoreInPostgreSql(connectionString,"counter_saga_subscription_store").EnsureTableIsCreated())
                         //.Subscriptions(c => c.StoreInXmlFile("C:\\temp\\saga_subscriptions.xml"))
                         .Sagas(c => c.StoreInPostgreSql(connectionString,"saga_store","saga_index").EnsureTablesAreCreated()) 
                         .MessageOwnership(o => o.FromRebusConfigurationSection())
                         .CreateBus()
                         .Start();

                adapter.Register(typeof(CounterSaga));

                adapter.Bus.Subscribe<RunStarted>();
                adapter.Bus.Subscribe<TestMessage>();

                Console.WriteLine("Welcome to the timer / counter saga.  Press ENTER to quit");
                Console.ReadLine();
            }
        }
    }

    public class CounterSagaData : ISagaData
    {

        public Guid Id {get;set;}
        public int Revision { get; set; }
        public int ReceivedCount { get; set; }
        public int NumberOfMessages { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class CounterSaga : Saga<CounterSagaData>, IAmInitiatedBy<RunStarted>, IHandleMessages<TestMessage>
    {
        public override void ConfigureHowToFindSaga()
        {
            Incoming<RunStarted>(m => m.Id).CorrelatesWith(s => s.Id);
            Incoming<TestMessage>(m => m.RunId).CorrelatesWith(s => s.Id);
        }

        public void Handle(RunStarted message)
        {
            if (!IsNew) return;
            Console.WriteLine("Run started for job {0}", message.Id);

            Data.Id = message.Id;
            Data.NumberOfMessages = message.NumberOfMessages;
            Data.ReceivedCount = 0;
            Data.StartTime = DateTime.Now;
        }
        public void Handle(TestMessage message)
        {
            Data.ReceivedCount = Data.ReceivedCount + 1;

            if (Data.ReceivedCount % 1000 == 0)
            {
                Console.WriteLine(string.Format("{0} Messages Handled for job {1}", Data.ReceivedCount,message.RunId));
            }

            if (RunCompleted())
            {
                var elapsedSeconds = DateTime.Now.Subtract(Data.StartTime).TotalSeconds;
                Console.WriteLine(string.Format("{0} messages handled in {1:1}s for job {2} ", Data.NumberOfMessages,elapsedSeconds,Data.Id));
                Console.WriteLine("rate {0:0} msg/s for job {1}", Convert.ToDouble(Data.NumberOfMessages) / elapsedSeconds,Data.Id); 
                MarkAsComplete();
            }
        }

        public bool RunCompleted()
        {
            return Data.ReceivedCount == Data.NumberOfMessages;
        }
    }
}
