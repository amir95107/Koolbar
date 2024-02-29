namespace DataLayer.Events
{
    public enum EventType
    {
        Add=1,
        Update=2,
        Delete=3
    }

    public abstract class DomainEvent
    {
        public string ExchangeName { get; set; }

        public string[] Routes { get; set; } = Array.Empty<string>();

        public EventType EventType { get; set; } = EventType.Add;
        public Dictionary<string, object> Payload { get; set; } = new Dictionary<string, object>();

        public DomainEvent()
        {
            ExchangeName = "STATE_CHANGES";
            Routes = new[] { "Koolbar" };
        }

    }
}
