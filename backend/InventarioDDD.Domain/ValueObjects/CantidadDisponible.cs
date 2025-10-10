namespace InventarioDDD.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa una cantidad disponible en inventario
    /// </summary>
    public class CantidadDisponible
    {
        public decimal Valor { get; private set; }

        // Constructor privado para Entity Framework
        private CantidadDisponible()
        {
        }

        public CantidadDisponible(decimal valor)
        {
            if (valor < 0)
                throw new ArgumentException("La cantidad disponible no puede ser negativa", nameof(valor));

            Valor = valor;
        }

        public bool EsCero => Valor == 0;
        public bool EsMayorQue(decimal cantidad) => Valor > cantidad;
        public bool EsMenorQue(decimal cantidad) => Valor < cantidad;
        public bool EsMayorOIgualQue(decimal cantidad) => Valor >= cantidad;
        public bool EsMenorOIgualQue(decimal cantidad) => Valor <= cantidad;

        public CantidadDisponible Sumar(decimal cantidad)
        {
            if (cantidad < 0)
                throw new ArgumentException("No se puede sumar una cantidad negativa", nameof(cantidad));

            return new CantidadDisponible(Valor + cantidad);
        }

        public CantidadDisponible Restar(decimal cantidad)
        {
            if (cantidad < 0)
                throw new ArgumentException("No se puede restar una cantidad negativa", nameof(cantidad));

            var nuevoValor = Valor - cantidad;
            if (nuevoValor < 0)
                throw new InvalidOperationException("La operación resultaría en una cantidad negativa");

            return new CantidadDisponible(nuevoValor);
        }

        public static CantidadDisponible Zero => new(0);

        public override bool Equals(object? obj)
        {
            return obj is CantidadDisponible other && Valor == other.Valor;
        }

        public override int GetHashCode()
        {
            return Valor.GetHashCode();
        }

        public override string ToString()
        {
            return Valor.ToString("F2");
        }

        public static implicit operator decimal(CantidadDisponible cantidad)
        {
            return cantidad.Valor;
        }

        public static bool operator >(CantidadDisponible left, CantidadDisponible right)
        {
            return left.Valor > right.Valor;
        }

        public static bool operator <(CantidadDisponible left, CantidadDisponible right)
        {
            return left.Valor < right.Valor;
        }

        public static bool operator >=(CantidadDisponible left, CantidadDisponible right)
        {
            return left.Valor >= right.Valor;
        }

        public static bool operator <=(CantidadDisponible left, CantidadDisponible right)
        {
            return left.Valor <= right.Valor;
        }

        public static CantidadDisponible operator +(CantidadDisponible left, CantidadDisponible right)
        {
            return new CantidadDisponible(left.Valor + right.Valor);
        }

        public static CantidadDisponible operator -(CantidadDisponible left, CantidadDisponible right)
        {
            return left.Restar(right.Valor);
        }
    }
}