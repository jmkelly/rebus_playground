using System;

namespace RebusTests.Messages
{
    public class RunStarted
    {
        public Guid Id {get;set;}
        public int NumberOfMessages { get; set; }
        public DateTime StartedAt { get; set; }
    }
}
