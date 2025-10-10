namespace InventarioDDD.Domain.Entities
{
    public class Categoria
    {
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public bool Activa { get; private set; }

        private Categoria()
        {
            Nombre = string.Empty;
            Descripcion = string.Empty;
        }

        public Categoria(string nombre, string descripcion)
        {
            Id = Guid.NewGuid();
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Descripcion = descripcion ?? throw new ArgumentNullException(nameof(descripcion));
            FechaCreacion = DateTime.UtcNow;
            Activa = true;
        }

        public void ActualizarInformacion(string nombre, string descripcion)
        {
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Descripcion = descripcion ?? throw new ArgumentNullException(nameof(descripcion));
        }

        public void Desactivar()
        {
            Activa = false;
        }

        public void Activar()
        {
            Activa = true;
        }
    }
}