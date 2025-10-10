using InventarioDDD.Domain.Entities;
using InventarioDDD.Domain.ValueObjects;
using InventarioDDD.Domain.Aggregates;
using InventarioDDD.Domain.Enums;

namespace InventarioDDD.Domain.Services
{
    /// <summary>
    /// Servicio de dominio para la gestión de consumo de ingredientes
    /// </summary>
    public class ServicioDeConsumo
    {
        /// <summary>
        /// Planifica el consumo de ingredientes aplicando la regla FIFO
        /// </summary>
        public PlanConsumo PlanificarConsumo(IngredienteAggregate ingredienteAggregate, decimal cantidadRequerida)
        {
            if (ingredienteAggregate == null)
                throw new ArgumentNullException(nameof(ingredienteAggregate));

            if (cantidadRequerida <= 0)
                throw new ArgumentException("La cantidad requerida debe ser mayor a cero");

            var planConsumo = new PlanConsumo();
            var lotesDisponibles = ingredienteAggregate.Lotes
                .Where(l => l.CantidadDisponible > 0 && !l.EstaVencido())
                .OrderBy(l => l.FechaVencimiento.Valor) // FIFO
                .ToList();

            if (!lotesDisponibles.Any())
            {
                planConsumo.AgregarError("No hay lotes disponibles para el consumo");
                return planConsumo;
            }

            decimal cantidadRestante = cantidadRequerida;
            var totalDisponible = lotesDisponibles.Sum(l => l.CantidadDisponible);

            if (totalDisponible < cantidadRequerida)
            {
                planConsumo.AgregarError($"Stock insuficiente. Disponible: {totalDisponible}, Requerido: {cantidadRequerida}");
                return planConsumo;
            }

            foreach (var lote in lotesDisponibles)
            {
                if (cantidadRestante <= 0) break;

                var cantidadAConsumir = Math.Min(cantidadRestante, lote.CantidadDisponible);

                planConsumo.AgregarConsumoLote(new ConsumoLotePlanificado
                {
                    LoteId = lote.Id,
                    CodigoLote = lote.Codigo,
                    CantidadAConsumir = cantidadAConsumir,
                    FechaVencimiento = lote.FechaVencimiento.Valor,
                    PrecioUnitario = lote.PrecioUnitario
                });

                cantidadRestante -= cantidadAConsumir;
            }

            planConsumo.MarcarComoExitoso();
            return planConsumo;
        }

        /// <summary>
        /// Valida si es posible realizar un consumo específico
        /// </summary>
        public ResultadoValidacion ValidarConsumo(IngredienteAggregate ingredienteAggregate,
                                                decimal cantidadRequerida, string motivo)
        {
            var resultado = new ResultadoValidacion();

            if (ingredienteAggregate == null)
            {
                resultado.AgregarError("El agregado de ingrediente es requerido");
                return resultado;
            }

            if (cantidadRequerida <= 0)
            {
                resultado.AgregarError("La cantidad a consumir debe ser mayor a cero");
                return resultado;
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                resultado.AgregarError("El motivo del consumo es requerido");
                return resultado;
            }

            // Verificar stock disponible
            var stockActual = ingredienteAggregate.Ingrediente.CantidadEnStock.Valor;
            if (stockActual < cantidadRequerida)
            {
                resultado.AgregarError($"Stock insuficiente. Disponible: {stockActual}, Requerido: {cantidadRequerida}");
                return resultado;
            }

            // Verificar que existan lotes no vencidos
            var lotesDisponibles = ingredienteAggregate.Lotes
                .Where(l => l.CantidadDisponible > 0 && !l.EstaVencido())
                .ToList();

            if (!lotesDisponibles.Any())
            {
                resultado.AgregarError("No hay lotes disponibles (todos están vencidos o agotados)");
                return resultado;
            }

            // Verificar alertas de stock bajo después del consumo
            var stockDespuesConsumo = stockActual - cantidadRequerida;
            if (stockDespuesConsumo <= ingredienteAggregate.Ingrediente.RangoDeStock.StockMinimo)
            {
                resultado.AgregarAdvertencia($"El consumo dejará el stock en nivel crítico: {stockDespuesConsumo}");
            }

            if (stockDespuesConsumo <= ingredienteAggregate.Ingrediente.RangoDeStock.CalcularPuntoDeReorden())
            {
                resultado.AgregarAdvertencia("Se requiere generar orden de reabastecimiento");
            }

            resultado.MarcarComoExitoso();
            return resultado;
        }

        /// <summary>
        /// Identifica lotes próximos a vencer que deben ser consumidos prioritariamente
        /// </summary>
        public List<AlertaConsumoUrgente> IdentificarConsumosUrgentes(List<IngredienteAggregate> ingredientes,
                                                                     int diasAnticipacion = 7)
        {
            var alertas = new List<AlertaConsumoUrgente>();

            foreach (var ingredienteAggregate in ingredientes)
            {
                var lotesUrgentes = ingredienteAggregate.ObtenerLotesProximosAVencer(diasAnticipacion);

                foreach (var lote in lotesUrgentes)
                {
                    if (lote.CantidadDisponible > 0)
                    {
                        alertas.Add(new AlertaConsumoUrgente
                        {
                            IngredienteId = ingredienteAggregate.Id,
                            NombreIngrediente = ingredienteAggregate.Ingrediente.Nombre,
                            LoteId = lote.Id,
                            CodigoLote = lote.Codigo,
                            CantidadDisponible = lote.CantidadDisponible,
                            FechaVencimiento = lote.FechaVencimiento.Valor,
                            DiasHastaVencimiento = lote.FechaVencimiento.DiasHastaVencimiento(),
                            ValorEnRiesgo = lote.CalcularValorInventario()
                        });
                    }
                }
            }

            return alertas.OrderBy(a => a.DiasHastaVencimiento).ToList();
        }

        /// <summary>
        /// Calcula el impacto financiero de un consumo
        /// </summary>
        public ImpactoFinancieroConsumo CalcularImpactoFinanciero(PlanConsumo planConsumo)
        {
            if (planConsumo == null || !planConsumo.EsExitoso)
                return new ImpactoFinancieroConsumo(0, 0, 0);

            var costoTotal = planConsumo.ConsumosLote.Sum(c => c.CantidadAConsumir * c.PrecioUnitario.Valor);
            var costoPromedioPonderado = planConsumo.CantidadTotal > 0 ? costoTotal / planConsumo.CantidadTotal : 0;
            var valorInventarioConsumido = costoTotal;

            return new ImpactoFinancieroConsumo(costoTotal, costoPromedioPonderado, valorInventarioConsumido);
        }

        /// <summary>
        /// Optimiza el orden de consumo para minimizar desperdicios
        /// </summary>
        public List<RecomendacionConsumo> OptimizarConsumo(List<IngredienteAggregate> ingredientes)
        {
            var recomendaciones = new List<RecomendacionConsumo>();

            foreach (var ingredienteAggregate in ingredientes)
            {
                var lotesOrdenados = ingredienteAggregate.Lotes
                    .Where(l => l.CantidadDisponible > 0 && !l.EstaVencido())
                    .OrderBy(l => l.FechaVencimiento.Valor)
                    .ToList();

                foreach (var lote in lotesOrdenados.Take(2)) // Los 2 más antiguos
                {
                    var diasHastaVencimiento = lote.FechaVencimiento.DiasHastaVencimiento();
                    var prioridad = CalcularPrioridadConsumo(diasHastaVencimiento, lote.CantidadDisponible);

                    if (prioridad > 0)
                    {
                        recomendaciones.Add(new RecomendacionConsumo
                        {
                            IngredienteId = ingredienteAggregate.Id,
                            NombreIngrediente = ingredienteAggregate.Ingrediente.Nombre,
                            LoteId = lote.Id,
                            CodigoLote = lote.Codigo,
                            CantidadRecomendada = lote.CantidadDisponible,
                            Prioridad = prioridad,
                            Motivo = ObtenerMotivoRecomendacion(diasHastaVencimiento)
                        });
                    }
                }
            }

            return recomendaciones.OrderByDescending(r => r.Prioridad).ToList();
        }

        // Métodos privados auxiliares

        private int CalcularPrioridadConsumo(int diasHastaVencimiento, decimal cantidad)
        {
            if (diasHastaVencimiento <= 3) return 5; // Crítico
            if (diasHastaVencimiento <= 7) return 4; // Alto
            if (diasHastaVencimiento <= 14) return 3; // Medio
            if (diasHastaVencimiento <= 30) return 2; // Bajo
            return 1; // Mínimo
        }

        private string ObtenerMotivoRecomendacion(int diasHastaVencimiento)
        {
            return diasHastaVencimiento switch
            {
                <= 3 => "Consumo urgente - Vence en menos de 3 días",
                <= 7 => "Consumo prioritario - Vence en menos de 1 semana",
                <= 14 => "Consumo recomendado - Vence en menos de 2 semanas",
                <= 30 => "Considerar consumo - Vence en menos de 1 mes",
                _ => "Seguimiento normal"
            };
        }
    }
}