using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Enums;

namespace InventarioDDD.Domain.Aggregates
{
    public class MovimientoInventarioAggregate
    {
        private readonly List<MovimientoInventario> _movimientos;

        public IReadOnlyList<MovimientoInventario> Movimientos => _movimientos.AsReadOnly();

        public MovimientoInventarioAggregate(List<MovimientoInventario>? movimientos = null)
        {
            _movimientos = movimientos ?? new List<MovimientoInventario>();
        }

        // Reglas de negocio del agregado MovimientoInventario

        /// <summary>
        /// Registra un movimiento de entrada de inventario
        /// </summary>
        public MovimientoInventario RegistrarEntrada(Guid ingredienteId, decimal cantidad,
                                                   UnidadDeMedida unidadDeMedida, string motivo,
                                                   Guid? loteId = null, string? documentoReferencia = null,
                                                   Guid? usuarioId = null, string? observaciones = null)
        {
            ValidarMovimiento(ingredienteId, cantidad, unidadDeMedida, motivo);

            var movimiento = new MovimientoInventario(
                ingredienteId,
                TipoMovimiento.Entrada,
                cantidad,
                unidadDeMedida,
                motivo,
                loteId,
                documentoReferencia,
                usuarioId,
                observaciones);

            _movimientos.Add(movimiento);

            // Disparar evento: MovimientoInventarioRegistrado
            // Disparar evento: StockActualizado

            return movimiento;
        }

        /// <summary>
        /// Registra un movimiento de salida de inventario
        /// </summary>
        public MovimientoInventario RegistrarSalida(Guid ingredienteId, decimal cantidad,
                                                  UnidadDeMedida unidadDeMedida, string motivo,
                                                  Guid? loteId = null, string? documentoReferencia = null,
                                                  Guid? usuarioId = null, string? observaciones = null)
        {
            ValidarMovimiento(ingredienteId, cantidad, unidadDeMedida, motivo);

            var movimiento = new MovimientoInventario(
                ingredienteId,
                TipoMovimiento.Salida,
                cantidad,
                unidadDeMedida,
                motivo,
                loteId,
                documentoReferencia,
                usuarioId,
                observaciones);

            _movimientos.Add(movimiento);

            // Disparar evento: MovimientoInventarioRegistrado
            // Disparar evento: StockActualizado
            // Disparar evento: IngredientesConsumidos (si es por consumo)

            return movimiento;
        }

        /// <summary>
        /// Registra un ajuste de inventario (puede ser positivo o negativo)
        /// </summary>
        public MovimientoInventario RegistrarAjuste(Guid ingredienteId, decimal cantidadAjuste,
                                                   UnidadDeMedida unidadDeMedida, string motivo,
                                                   Guid? usuarioId = null, string? observaciones = null)
        {
            ValidarMovimiento(ingredienteId, Math.Abs(cantidadAjuste), unidadDeMedida, motivo);

            var movimiento = new MovimientoInventario(
                ingredienteId,
                TipoMovimiento.Ajuste,
                cantidadAjuste, // Puede ser negativo para ajustes hacia abajo
                unidadDeMedida,
                motivo,
                null,
                null,
                usuarioId,
                observaciones);

            _movimientos.Add(movimiento);

            // Disparar evento: AjusteInventarioRegistrado
            // Disparar evento: StockActualizado

            return movimiento;
        }

        /// <summary>
        /// Registra una transferencia entre ubicaciones
        /// </summary>
        public MovimientoInventario RegistrarTransferencia(Guid ingredienteId, decimal cantidad,
                                                          UnidadDeMedida unidadDeMedida, string motivo,
                                                          Guid? loteId = null, Guid? usuarioId = null,
                                                          string? observaciones = null)
        {
            ValidarMovimiento(ingredienteId, cantidad, unidadDeMedida, motivo);

            var movimiento = new MovimientoInventario(
                ingredienteId,
                TipoMovimiento.Transferencia,
                cantidad,
                unidadDeMedida,
                motivo,
                loteId,
                null,
                usuarioId,
                observaciones);

            _movimientos.Add(movimiento);

            // Disparar evento: TransferenciaRegistrada

            return movimiento;
        }

        /// <summary>
        /// Obtiene el historial de movimientos de un ingrediente específico
        /// </summary>
        public List<MovimientoInventario> ObtenerHistorialIngrediente(Guid ingredienteId,
                                                                     DateTime? fechaDesde = null,
                                                                     DateTime? fechaHasta = null)
        {
            var movimientos = _movimientos.Where(m => m.IngredienteId == ingredienteId);

            if (fechaDesde.HasValue)
                movimientos = movimientos.Where(m => m.FechaMovimiento >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                movimientos = movimientos.Where(m => m.FechaMovimiento <= fechaHasta.Value);

            return movimientos.OrderByDescending(m => m.FechaMovimiento).ToList();
        }

        /// <summary>
        /// Obtiene movimientos por tipo en un período específico
        /// </summary>
        public List<MovimientoInventario> ObtenerMovimientosPorTipo(TipoMovimiento tipo,
                                                                   DateTime fechaDesde,
                                                                   DateTime fechaHasta)
        {
            return _movimientos
                .Where(m => m.TipoMovimiento == tipo &&
                           m.FechaMovimiento >= fechaDesde &&
                           m.FechaMovimiento <= fechaHasta)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToList();
        }

        /// <summary>
        /// Obtiene movimientos de un usuario específico
        /// </summary>
        public List<MovimientoInventario> ObtenerMovimientosUsuario(Guid usuarioId,
                                                                   DateTime? fechaDesde = null,
                                                                   DateTime? fechaHasta = null)
        {
            var movimientos = _movimientos.Where(m => m.UsuarioId == usuarioId);

            if (fechaDesde.HasValue)
                movimientos = movimientos.Where(m => m.FechaMovimiento >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                movimientos = movimientos.Where(m => m.FechaMovimiento <= fechaHasta.Value);

            return movimientos.OrderByDescending(m => m.FechaMovimiento).ToList();
        }

        /// <summary>
        /// Calcula el impacto neto en el stock de un ingrediente
        /// </summary>
        public decimal CalcularImpactoNetoStock(Guid ingredienteId, DateTime fechaDesde, DateTime fechaHasta)
        {
            return _movimientos
                .Where(m => m.IngredienteId == ingredienteId &&
                           m.FechaMovimiento >= fechaDesde &&
                           m.FechaMovimiento <= fechaHasta)
                .Sum(m => m.ObtenerImpactoEnStock());
        }

        /// <summary>
        /// Obtiene un resumen de movimientos por período
        /// </summary>
        public ResumenMovimientos ObtenerResumenPeriodo(DateTime fechaDesde, DateTime fechaHasta)
        {
            var movimientosEnPeriodo = _movimientos
                .Where(m => m.FechaMovimiento >= fechaDesde && m.FechaMovimiento <= fechaHasta)
                .ToList();

            var entradas = movimientosEnPeriodo.Where(m => m.EsEntrada()).Sum(m => m.Cantidad);
            var salidas = movimientosEnPeriodo.Where(m => m.EsSalida()).Sum(m => m.Cantidad);
            var ajustes = movimientosEnPeriodo.Where(m => m.EsAjuste()).Sum(m => m.ObtenerImpactoEnStock());
            var transferencias = movimientosEnPeriodo.Where(m => m.EsTransferencia()).Sum(m => m.Cantidad);

            return new ResumenMovimientos(
                movimientosEnPeriodo.Count,
                entradas,
                salidas,
                ajustes,
                transferencias
            );
        }

        /// <summary>
        /// Obtiene movimientos con documentos de referencia
        /// </summary>
        public List<MovimientoInventario> ObtenerMovimientosConDocumento(string documentoReferencia)
        {
            return _movimientos
                .Where(m => !string.IsNullOrEmpty(m.DocumentoReferencia) &&
                           m.DocumentoReferencia.Contains(documentoReferencia))
                .OrderByDescending(m => m.FechaMovimiento)
                .ToList();
        }

        /// <summary>
        /// Valida la consistencia de los movimientos
        /// </summary>
        public List<string> ValidarConsistencia()
        {
            var errores = new List<string>();

            // Verificar que no haya movimientos duplicados
            var movimientosDuplicados = _movimientos
                .GroupBy(m => new { m.IngredienteId, m.FechaMovimiento, m.Cantidad, m.TipoMovimiento })
                .Where(g => g.Count() > 1)
                .ToList();

            if (movimientosDuplicados.Any())
            {
                errores.Add($"Se encontraron {movimientosDuplicados.Count} grupos de movimientos duplicados");
            }

            // Verificar que no haya movimientos con fechas futuras
            var movimientosFuturos = _movimientos
                .Where(m => m.FechaMovimiento > DateTime.UtcNow)
                .ToList();

            if (movimientosFuturos.Any())
            {
                errores.Add($"Se encontraron {movimientosFuturos.Count} movimientos con fechas futuras");
            }

            return errores;
        }

        // Métodos privados para validaciones

        private void ValidarMovimiento(Guid ingredienteId, decimal cantidad, UnidadDeMedida unidadDeMedida, string motivo)
        {
            if (ingredienteId == Guid.Empty)
                throw new ArgumentException("El ID del ingrediente es requerido");

            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero");

            if (unidadDeMedida == null)
                throw new ArgumentException("La unidad de medida es requerida");

            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo del movimiento es requerido");
        }
    }

    // Clase auxiliar para el resumen de movimientos
    public class ResumenMovimientos
    {
        public int TotalMovimientos { get; }
        public decimal TotalEntradas { get; }
        public decimal TotalSalidas { get; }
        public decimal TotalAjustes { get; }
        public decimal TotalTransferencias { get; }
        public decimal MovimientoNeto => TotalEntradas - TotalSalidas + TotalAjustes;

        public ResumenMovimientos(int totalMovimientos, decimal totalEntradas, decimal totalSalidas,
                                decimal totalAjustes, decimal totalTransferencias)
        {
            TotalMovimientos = totalMovimientos;
            TotalEntradas = totalEntradas;
            TotalSalidas = totalSalidas;
            TotalAjustes = totalAjustes;
            TotalTransferencias = totalTransferencias;
        }
    }
}