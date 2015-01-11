using System;

namespace RebusTests.Messages
{
    public class TestMessage
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public Guid RunId { get; set; }
    }
}
