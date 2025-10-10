namespace InventarioDDD.Domain.Enums
{
    public enum EstadoOrdenCompra
    {
        Pendiente = 1,
        Aprobada = 2,
        Enviada = 3,
        Recibida = 4,
        Cancelada = 5
    }

    public enum EstadoOrden
    {
        Pendiente = 1,
        Aprobada = 2,
        EnvioPendiente = 3,
        Recibida = 4,
        Cancelada = 5
    }

    public enum TipoMovimiento
    {
        Entrada = 1,
        Salida = 2,
        Ajuste = 3,
        Transferencia = 4
    }

    public enum EstadoIngrediente
    {
        Activo = 1,
        Inactivo = 2,
        Descontinuado = 3
    }
}