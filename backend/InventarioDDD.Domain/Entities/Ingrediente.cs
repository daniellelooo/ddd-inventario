using InventarioDDD.Domain.ValueObjects;

namespace InventarioDDD.Domain.Entities
{
    public class Ingrediente
    {
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public UnidadDeMedida UnidadDeMedida { get; private set; }
        public CantidadDisponible CantidadEnStock { get; private set; }
        public RangoDeStock RangoDeStock { get; private set; }
        public Guid CategoriaId { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime? FechaActualizacion { get; private set; }
        public bool Activo { get; private set; }

        // Colección de lotes asociados
        private readonly List<Lote> _lotes;
        public IReadOnlyList<Lote> Lotes => _lotes.AsReadOnly();

        private Ingrediente()
        {
            Nombre = string.Empty;
            Descripcion = string.Empty;
            UnidadDeMedida = null!;
            CantidadEnStock = null!;
            RangoDeStock = null!;
            _lotes = new List<Lote>();
        }

        public Ingrediente(string nombre, string descripcion, UnidadDeMedida unidadDeMedida,
                          RangoDeStock rangoDeStock, Guid categoriaId)
        {
            Id = Guid.NewGuid();
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Descripcion = descripcion ?? throw new ArgumentNullException(nameof(descripcion));
            UnidadDeMedida = unidadDeMedida ?? throw new ArgumentNullException(nameof(unidadDeMedida));
            CantidadEnStock = new CantidadDisponible(0);
            RangoDeStock = rangoDeStock ?? throw new ArgumentNullException(nameof(rangoDeStock));
            CategoriaId = categoriaId;
            FechaCreacion = DateTime.UtcNow;
            Activo = true;
            _lotes = new List<Lote>();
        }

        public void AgregarLote(Lote lote)
        {
            if (lote == null) throw new ArgumentNullException(nameof(lote));

            _lotes.Add(lote);
            RecalcularStock();
        }

        public void ConsumirIngrediente(decimal cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero");

            if (CantidadEnStock.Valor < cantidad)
                throw new InvalidOperationException("Stock insuficiente para el consumo");

            // Consumir por FIFO (primero los lotes más antiguos)
            var lotesOrdenados = _lotes
                .Where(l => l.CantidadDisponible > 0)
                .OrderBy(l => l.FechaVencimiento.Valor)
                .ToList();

            decimal cantidadRestante = cantidad;

            foreach (var lote in lotesOrdenados)
            {
                if (cantidadRestante <= 0) break;

                if (lote.CantidadDisponible >= cantidadRestante)
                {
                    lote.Consumir(cantidadRestante);
                    cantidadRestante = 0;
                }
                else
                {
                    cantidadRestante -= lote.CantidadDisponible;
                    lote.Consumir(lote.CantidadDisponible);
                }
            }

            RecalcularStock();
        }

        public bool TieneStockBajo()
        {
            return CantidadEnStock.Valor <= RangoDeStock.StockMinimo;
        }

        public bool RequiereReabastecimiento()
        {
            return CantidadEnStock.Valor <= RangoDeStock.CalcularPuntoDeReorden();
        }

        private void RecalcularStock()
        {
            var totalStock = _lotes.Sum(l => l.CantidadDisponible);
            CantidadEnStock = new CantidadDisponible(totalStock);
            FechaActualizacion = DateTime.UtcNow;
        }

        public void ActualizarRangoStock(RangoDeStock nuevoRango)
        {
            RangoDeStock = nuevoRango ?? throw new ArgumentNullException(nameof(nuevoRango));
            FechaActualizacion = DateTime.UtcNow;
        }

        public void Desactivar()
        {
            Activo = false;
            FechaActualizacion = DateTime.UtcNow;
        }
    }
}