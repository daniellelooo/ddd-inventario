namespace InventarioDDD.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa una unidad de medida
    /// </summary>
    public class UnidadDeMedida
    {
        public string Nombre { get; private set; }
        public string Simbolo { get; private set; }

        // Constructor privado para Entity Framework
        private UnidadDeMedida()
        {
            Nombre = string.Empty;
            Simbolo = string.Empty;
        }

        public UnidadDeMedida(string nombre, string simbolo)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la unidad de medida no puede estar vacío", nameof(nombre));

            if (string.IsNullOrWhiteSpace(simbolo))
                throw new ArgumentException("El símbolo de la unidad de medida no puede estar vacío", nameof(simbolo));

            Nombre = nombre.Trim();
            Simbolo = simbolo.Trim();
        }

        public override bool Equals(object? obj)
        {
            return obj is UnidadDeMedida other &&
                   Nombre == other.Nombre &&
                   Simbolo == other.Simbolo;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Nombre, Simbolo);
        }

        public override string ToString()
        {
            return $"{Nombre} ({Simbolo})";
        }

        // Unidades de medida predefinidas comunes
        public static UnidadDeMedida Kilogramos => new("Kilogramos", "kg");
        public static UnidadDeMedida Gramos => new("Gramos", "g");
        public static UnidadDeMedida Litros => new("Litros", "L");
        public static UnidadDeMedida Mililitros => new("Mililitros", "mL");
        public static UnidadDeMedida Unidades => new("Unidades", "u");
        public static UnidadDeMedida Libras => new("Libras", "lb");
        public static UnidadDeMedida Onzas => new("Onzas", "oz");
    }
}