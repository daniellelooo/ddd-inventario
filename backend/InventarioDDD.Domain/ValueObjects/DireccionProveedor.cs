namespace InventarioDDD.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa la dirección de un proveedor
    /// </summary>
    public class DireccionProveedor
    {
        public string Calle { get; private set; }
        public string Ciudad { get; private set; }
        public string Pais { get; private set; }
        public string? CodigoPostal { get; private set; }

        // Constructor privado para Entity Framework
        private DireccionProveedor()
        {
            Calle = string.Empty;
            Ciudad = string.Empty;
            Pais = string.Empty;
        }

        public DireccionProveedor(string calle, string ciudad, string pais, string? codigoPostal = null)
        {
            if (string.IsNullOrWhiteSpace(ciudad))
                throw new ArgumentException("La ciudad no puede estar vacía", nameof(ciudad));

            if (string.IsNullOrWhiteSpace(pais))
                throw new ArgumentException("El país no puede estar vacío", nameof(pais));

            Calle = calle?.Trim() ?? string.Empty;
            Ciudad = ciudad.Trim();
            Pais = pais.Trim();
            CodigoPostal = string.IsNullOrWhiteSpace(codigoPostal) ? null : codigoPostal.Trim();
        }

        public bool TieneDireccionCompleta => !string.IsNullOrWhiteSpace(Calle);
        public bool TieneCodigoPostal => !string.IsNullOrWhiteSpace(CodigoPostal);

        public string ObtenerDireccionCompleta()
        {
            var partes = new List<string>();

            if (!string.IsNullOrWhiteSpace(Calle))
                partes.Add(Calle);

            partes.Add(Ciudad);
            partes.Add(Pais);

            if (TieneCodigoPostal)
                partes.Add(CodigoPostal!);

            return string.Join(", ", partes);
        }

        public string ObtenerDireccionResumida()
        {
            return $"{Ciudad}, {Pais}";
        }

        public bool EsDeMismoPais(DireccionProveedor otra)
        {
            return string.Equals(Pais, otra.Pais, StringComparison.OrdinalIgnoreCase);
        }

        public bool EsDeMismaCiudad(DireccionProveedor otra)
        {
            return EsDeMismoPais(otra) &&
                   string.Equals(Ciudad, otra.Ciudad, StringComparison.OrdinalIgnoreCase);
        }

        public bool EsNacional()
        {
            // Asumiendo que Colombia es el país base
            return string.Equals(Pais, "Colombia", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(Pais, "CO", StringComparison.OrdinalIgnoreCase);
        }

        public bool EsInternacional()
        {
            return !EsNacional();
        }

        public DireccionProveedor ActualizarCalle(string nuevaCalle)
        {
            return new DireccionProveedor(nuevaCalle, Ciudad, Pais, CodigoPostal);
        }

        public DireccionProveedor ActualizarCodigoPostal(string nuevoCodigoPostal)
        {
            return new DireccionProveedor(Calle, Ciudad, Pais, nuevoCodigoPostal);
        }

        public override bool Equals(object? obj)
        {
            return obj is DireccionProveedor other &&
                   string.Equals(Calle, other.Calle, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Ciudad, other.Ciudad, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Pais, other.Pais, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CodigoPostal, other.CodigoPostal, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Calle?.ToLowerInvariant(),
                Ciudad.ToLowerInvariant(),
                Pais.ToLowerInvariant(),
                CodigoPostal?.ToLowerInvariant()
            );
        }

        public override string ToString()
        {
            return ObtenerDireccionCompleta();
        }

        public string ToStringFormatted()
        {
            var direccion = new System.Text.StringBuilder();

            if (!string.IsNullOrWhiteSpace(Calle))
            {
                direccion.AppendLine(Calle);
            }

            var ciudadPais = CodigoPostal != null
                ? $"{Ciudad} {CodigoPostal}, {Pais}"
                : $"{Ciudad}, {Pais}";

            direccion.Append(ciudadPais);

            return direccion.ToString();
        }

        // Factory methods para direcciones comunes
        public static DireccionProveedor CrearNacional(string calle, string ciudad, string? codigoPostal = null)
        {
            return new DireccionProveedor(calle, ciudad, "Colombia", codigoPostal);
        }

        public static DireccionProveedor CrearInternacional(string calle, string ciudad, string pais, string? codigoPostal = null)
        {
            return new DireccionProveedor(calle, ciudad, pais, codigoPostal);
        }

        public static DireccionProveedor CrearSoloCiudad(string ciudad, string pais = "Colombia")
        {
            return new DireccionProveedor(string.Empty, ciudad, pais);
        }

        // Direcciones predefinidas para testing
        public static DireccionProveedor Bogota => CrearNacional("", "Bogotá", "110111");
        public static DireccionProveedor Medellin => CrearNacional("", "Medellín", "050001");
        public static DireccionProveedor Cali => CrearNacional("", "Cali", "760001");
        public static DireccionProveedor Barranquilla => CrearNacional("", "Barranquilla", "080001");
    }
}