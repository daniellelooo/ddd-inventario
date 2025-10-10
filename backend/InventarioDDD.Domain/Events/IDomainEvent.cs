namespace InventarioDDD.Domain.Events
{
    public interface IDomainEvent
    {
        Guid Id { get; }
        DateTime FechaOcurrencia { get; }
    }

    public abstract class DomainEvent : IDomainEvent
    {
        public Guid Id { get; private set; }
        public DateTime FechaOcurrencia { get; private set; }

        protected DomainEvent()
        {
            Id = Guid.NewGuid();
            FechaOcurrencia = DateTime.UtcNow;
        }
    }
}