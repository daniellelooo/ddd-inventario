using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.Domain.Aggregates;

/// <summary>
/// Aggregate Root: Ingrediente
/// Controla completamente el ciclo de vida de sus Lotes
/// </summary>
public class Ingrediente
{
    private readonly List<Lote> _lotes = new();
    private long _nextLoteId = 1;

    public long Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public Categoria Categoria { get; private set; } = null!;
    public List<long> ProveedoresIds { get; private set; } = new();
    public UnidadDeMedida UnidadDeMedida { get; private set; } = null!;
    public RangoDeStock RangoDeStock { get; private set; } = null!;
    public PrecioConMoneda? PrecioReferencia { get; private set; }
    public bool Activo { get; private set; } = true;

    // Propiedad para acceder a los lotes (solo lectura)
    public IReadOnlyList<Lote> Lotes => _lotes.AsReadOnly();

    // Constructor para EF Core
    private Ingrediente() { }

    public Ingrediente(long id, string nombre, Categoria categoria, List<long> proveedoresIds,
                       UnidadDeMedida unidadDeMedida, RangoDeStock rangoDeStock,
                       PrecioConMoneda? precioReferencia = null, bool activo = true)
    {
        ValidarNombre(nombre);
        ValidarCategoria(categoria);
        ValidarUnidadDeMedida(unidadDeMedida);
        ValidarRangoDeStock(rangoDeStock);

        Id = id;
        Nombre = nombre;
        Categoria = categoria;
        ProveedoresIds = proveedoresIds ?? new List<long>();
        UnidadDeMedida = unidadDeMedida;
        RangoDeStock = rangoDeStock;
        PrecioReferencia = precioReferencia;
        Activo = activo;
    }

    // ============================================
    // VALIDACIONES (Protegen las Invariantes)
    // ============================================

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del ingrediente es requerido", nameof(nombre));

        if (nombre.Length > 100)
            throw new ArgumentException("El nombre no puede exceder 100 caracteres", nameof(nombre));
    }

    private static void ValidarCategoria(Categoria categoria)
    {
        if (categoria == null)
            throw new ArgumentNullException(nameof(categoria), "La categoría es requerida");
    }

    private static void ValidarUnidadDeMedida(UnidadDeMedida unidad)
    {
        if (unidad == null)
            throw new ArgumentNullException(nameof(unidad), "La unidad de medida es requerida");
    }

    private static void ValidarRangoDeStock(RangoDeStock rango)
    {
        if (rango == null)
            throw new ArgumentNullException(nameof(rango), "El rango de stock es requerido");
    }

    // ============================================
    // MÉTODOS DE NEGOCIO - Gestión de Lotes
    // ============================================

    /// <summary>
    /// Agrega un nuevo lote al ingrediente (recepción de mercancía)
    /// </summary>
    public Lote AgregarLote(CantidadDisponible cantidad, FechaVencimiento fechaVencimiento, long proveedorId)
    {
        if (cantidad == null || !cantidad.EsPositivo())
            throw new ArgumentException("La cantidad debe ser positiva", nameof(cantidad));

        if (fechaVencimiento == null)
            throw new ArgumentNullException(nameof(fechaVencimiento));

        if (fechaVencimiento.EstaVencido())
            throw new ArgumentException("No se puede agregar un lote vencido");

        if (ProveedoresIds.Any() && !ProveedoresIds.Contains(proveedorId))
            throw new ArgumentException("El proveedor no está autorizado");

        var nuevoLote = new Lote(_nextLoteId++, cantidad, fechaVencimiento, DateTime.Now, proveedorId);
        _lotes.Add(nuevoLote);

        return nuevoLote;
    }

    /// <summary>
    /// Consume cantidad del ingrediente aplicando FIFO
    /// </summary>
    public void Consumir(CantidadDisponible cantidadRequerida)
    {
        if (cantidadRequerida == null || !cantidadRequerida.EsPositivo())
            throw new ArgumentException("La cantidad a consumir debe ser positiva", nameof(cantidadRequerida));

        if (!TieneStockSuficiente(cantidadRequerida))
            throw new InvalidOperationException("Stock insuficiente");

        var cantidadPendiente = cantidadRequerida.Valor;
        var lotesDisponibles = ObtenerLotesDisponibles();

        foreach (var lote in lotesDisponibles)
        {
            if (cantidadPendiente <= 0) break;

            var cantidadDelLote = lote.Cantidad.Valor;
            if (cantidadDelLote <= cantidadPendiente)
            {
                lote.MarcarAgotado();
                cantidadPendiente -= cantidadDelLote;
            }
            else
            {
                lote.ActualizarCantidad(new CantidadDisponible(cantidadDelLote - cantidadPendiente, UnidadDeMedida));
                cantidadPendiente = 0;
            }
        }
    }

    /// <summary>
    /// Calcula el stock total disponible
    /// </summary>
    public CantidadDisponible CalcularStockDisponible()
    {
        var total = _lotes
            .Where(lote => !lote.Agotado)
            .Where(lote => !lote.EstaVencido())
            .Sum(lote => lote.Cantidad.Valor);

        return new CantidadDisponible(total, UnidadDeMedida);
    }

    /// <summary>
    /// Verifica si hay stock suficiente
    /// </summary>
    public bool TieneStockSuficiente(CantidadDisponible cantidadRequerida)
    {
        return CalcularStockDisponible().Valor >= cantidadRequerida.Valor;
    }

    /// <summary>
    /// Obtiene lotes disponibles ordenados por FIFO
    /// </summary>
    public List<Lote> ObtenerLotesDisponibles()
    {
        return _lotes
            .Where(lote => !lote.Agotado)
            .Where(lote => !lote.EstaVencido())
            .OrderBy(lote => lote.FechaRecepcion)
            .ToList();
    }

    /// <summary>
    /// Verifica si está en stock bajo
    /// </summary>
    public bool EstaEnStockBajo()
    {
        return CalcularStockDisponible().Valor <= RangoDeStock.Minimo;
    }

    /// <summary>
    /// Verifica si necesita reabastecimiento
    /// </summary>
    public bool NecesitaReabastecimiento()
    {
        return CalcularStockDisponible().Valor < RangoDeStock.Optimo;
    }

    /// <summary>
    /// Calcula cantidad recomendada para reabastecimiento
    /// </summary>
    public CantidadDisponible CalcularCantidadReabastecimiento()
    {
        var stockActual = CalcularStockDisponible().Valor;
        var cantidadNecesaria = Math.Max(0, RangoDeStock.Optimo - stockActual);
        return new CantidadDisponible(cantidadNecesaria, UnidadDeMedida);
    }

    /// <summary>
    /// Obtiene lotes próximos a vencer
    /// </summary>
    public List<Lote> ObtenerLotesProximosVencer(int dias)
    {
        var fechaLimite = DateTime.Today.AddDays(dias);
        return _lotes
            .Where(lote => !lote.Agotado)
            .Where(lote => !lote.EstaVencido())
            .Where(lote => lote.FechaVencimiento.Fecha < fechaLimite)
            .OrderBy(lote => lote.FechaVencimiento.Fecha)
            .ToList();
    }

    /// <summary>
    /// Marca lotes vencidos automáticamente
    /// </summary>
    public List<Lote> MarcarLotesVencidos()
    {
        var lotesVencidos = _lotes
            .Where(lote => !lote.Agotado)
            .Where(lote => lote.EstaVencido())
            .ToList();

        foreach (var lote in lotesVencidos)
        {
            lote.MarcarAgotado();
        }

        return lotesVencidos;
    }

    // ============================================
    // MÉTODOS DE NEGOCIO - Gestión del Ingrediente
    // ============================================

    public void CambiarNombre(string nuevoNombre)
    {
        ValidarNombre(nuevoNombre);
        Nombre = nuevoNombre;
    }

    public void Activar() => Activo = true;

    public void Desactivar()
    {
        if (CalcularStockDisponible().Valor > 0)
            throw new InvalidOperationException("No se puede desactivar con stock disponible");

        Activo = false;
    }

    public void ActualizarPrecio(PrecioConMoneda nuevoPrecio)
    {
        if (nuevoPrecio == null || nuevoPrecio.Monto <= 0)
            throw new ArgumentException("El precio debe ser positivo", nameof(nuevoPrecio));

        PrecioReferencia = nuevoPrecio;
    }

    public void ActualizarRangoStock(RangoDeStock nuevoRango)
    {
        ValidarRangoDeStock(nuevoRango);
        RangoDeStock = nuevoRango;
    }

    public void AgregarProveedor(long proveedorId)
    {
        if (proveedorId > 0 && !ProveedoresIds.Contains(proveedorId))
            ProveedoresIds.Add(proveedorId);
    }

    public void RemoverProveedor(long proveedorId)
    {
        ProveedoresIds.Remove(proveedorId);
    }
}
