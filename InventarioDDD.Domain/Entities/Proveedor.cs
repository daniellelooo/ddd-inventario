namespace InventarioDDD.Domain.Entities;

/// <summary>
/// Entidad Proveedor
/// </summary>
public class Proveedor
{
    public long Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Contacto { get; private set; } = string.Empty;
    public string Telefono { get; private set; } = string.Empty;
    public string Direccion { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    // Constructor para EF Core
    private Proveedor() { }

    public Proveedor(long id, string nombre, string contacto, string telefono,
                     string direccion, string email, bool activo = true)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es requerido", nameof(nombre));

        Id = id;
        Nombre = nombre;
        Contacto = contacto ?? string.Empty;
        Telefono = telefono ?? string.Empty;
        Direccion = direccion ?? string.Empty;
        Email = email ?? string.Empty;
        Activo = activo;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    public void ActualizarContacto(string nuevoContacto, string nuevoTelefono, string nuevoEmail)
    {
        Contacto = nuevoContacto ?? Contacto;
        Telefono = nuevoTelefono ?? Telefono;
        Email = nuevoEmail ?? Email;
    }

    public void ActualizarDireccion(string nuevaDireccion)
    {
        Direccion = nuevaDireccion ?? Direccion;
    }
}
