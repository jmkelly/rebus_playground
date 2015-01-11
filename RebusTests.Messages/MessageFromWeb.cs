using System;

namespace RebusTests.Messages
{
    public class MessageFromWeb
    {
        public Guid Id { get; set; }
        public Guid RunId { get; set; }
        public DateTime StartedAt { get; set; }
    }
}
