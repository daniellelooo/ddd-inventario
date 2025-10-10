using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.Domain.Entities
{
    public class Proveedor
    {
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public string NIT { get; private set; }
        public string Telefono { get; private set; }
        public string Email { get; private set; }
        public DireccionProveedor Direccion { get; private set; }
        public string PersonaContacto { get; private set; }
        public DateTime FechaRegistro { get; private set; }
        public bool Activo { get; private set; }

        // Lista de ingredientes que puede suministrar
        private readonly List<Guid> _ingredientesSuministrados;
        public IReadOnlyList<Guid> IngredientesSuministrados => _ingredientesSuministrados.AsReadOnly();

        private Proveedor()
        {
            Nombre = string.Empty;
            NIT = string.Empty;
            Telefono = string.Empty;
            Email = string.Empty;
            Direccion = null!;
            PersonaContacto = string.Empty;
            _ingredientesSuministrados = new List<Guid>();
        }

        public Proveedor(string nombre, string nit, string telefono, string email,
                        DireccionProveedor direccion, string personaContacto)
        {
            Id = Guid.NewGuid();
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            NIT = nit ?? throw new ArgumentNullException(nameof(nit));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Direccion = direccion ?? throw new ArgumentNullException(nameof(direccion));
            PersonaContacto = personaContacto ?? throw new ArgumentNullException(nameof(personaContacto));
            FechaRegistro = DateTime.UtcNow;
            Activo = true;
            _ingredientesSuministrados = new List<Guid>();
        }

        public void ActualizarInformacion(string nombre, string telefono, string email,
                                         DireccionProveedor direccion, string personaContacto)
        {
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Direccion = direccion ?? throw new ArgumentNullException(nameof(direccion));
            PersonaContacto = personaContacto ?? throw new ArgumentNullException(nameof(personaContacto));
        }

        public void AgregarIngredienteSuministrado(Guid ingredienteId)
        {
            if (!_ingredientesSuministrados.Contains(ingredienteId))
            {
                _ingredientesSuministrados.Add(ingredienteId);
            }
        }

        public void RemoverIngredienteSuministrado(Guid ingredienteId)
        {
            _ingredientesSuministrados.Remove(ingredienteId);
        }

        public bool PuedeSuministrar(Guid ingredienteId)
        {
            return Activo && _ingredientesSuministrados.Contains(ingredienteId);
        }

        public void Activar()
        {
            Activo = true;
        }

        public void Desactivar()
        {
            Activo = false;
        }

        public bool EsNacional()
        {
            return Direccion.EsNacional();
        }

        public bool EsInternacional()
        {
            return Direccion.EsInternacional();
        }
    }
}