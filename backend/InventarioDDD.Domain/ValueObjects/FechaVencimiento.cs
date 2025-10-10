namespace InventarioDDD.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa una fecha de vencimiento
    /// </summary>
    public class FechaVencimiento
    {
        public DateTime Valor { get; private set; }

        // Constructor privado para Entity Framework
        private FechaVencimiento()
        {
        }

        public FechaVencimiento(DateTime fecha)
        {
            // Normalizar a solo fecha (sin hora)
            Valor = fecha.Date;
        }

        public bool EstaVencido => DateTime.Today > Valor;
        public bool VenceHoy => DateTime.Today == Valor;
        public bool VenceMañana => DateTime.Today.AddDays(1) == Valor;
        public bool EstaVigente => DateTime.Today <= Valor;

        public int DiasHastaVencimiento()
        {
            var diferencia = Valor - DateTime.Today;
            return diferencia.Days;
        }

        public int DiasDesdeVencimiento()
        {
            if (!EstaVencido) return 0;
            var diferencia = DateTime.Today - Valor;
            return diferencia.Days;
        }

        public bool VenceEnLosPróximosDias(int dias)
        {
            if (dias <= 0)
                throw new ArgumentException("El número de días debe ser positivo", nameof(dias));

            var fechaLimite = DateTime.Today.AddDays(dias);
            return Valor <= fechaLimite && EstaVigente;
        }

        public bool VenceEnLaPróximaSemana()
        {
            return VenceEnLosPróximosDias(7);
        }

        public bool VenceEnElPróximoMes()
        {
            return VenceEnLosPróximosDias(30);
        }

        public string ObtenerEstadoVencimiento()
        {
            if (EstaVencido)
                return $"Vencido hace {DiasDesdeVencimiento()} días";

            if (VenceHoy)
                return "Vence hoy";

            if (VenceMañana)
                return "Vence mañana";

            var diasRestantes = DiasHastaVencimiento();

            return diasRestantes switch
            {
                <= 3 => $"Vence en {diasRestantes} días (CRÍTICO)",
                <= 7 => $"Vence en {diasRestantes} días (URGENTE)",
                <= 15 => $"Vence en {diasRestantes} días (ATENCIÓN)",
                <= 30 => $"Vence en {diasRestantes} días",
                _ => $"Vence en {diasRestantes} días"
            };
        }

        public int ObtenerPrioridadVencimiento()
        {
            if (EstaVencido) return 5; // Crítico
            if (VenceHoy || VenceMañana) return 4; // Muy alto

            var dias = DiasHastaVencimiento();
            return dias switch
            {
                <= 3 => 4,  // Muy alto
                <= 7 => 3,  // Alto
                <= 15 => 2, // Medio
                <= 30 => 1, // Bajo
                _ => 0      // Muy bajo
            };
        }

        public bool RequiereAlerta(int diasAnticipacion = 7)
        {
            return VenceEnLosPróximosDias(diasAnticipacion) || EstaVencido;
        }

        public FechaVencimiento ExtenderVencimiento(int dias)
        {
            if (dias <= 0)
                throw new ArgumentException("Los días de extensión deben ser positivos", nameof(dias));

            return new FechaVencimiento(Valor.AddDays(dias));
        }

        public override bool Equals(object? obj)
        {
            return obj is FechaVencimiento other && Valor == other.Valor;
        }

        public override int GetHashCode()
        {
            return Valor.GetHashCode();
        }

        public override string ToString()
        {
            return Valor.ToString("yyyy-MM-dd");
        }

        public string ToStringConEstado()
        {
            return $"{ToString()} - {ObtenerEstadoVencimiento()}";
        }

        // Factory methods
        public static FechaVencimiento Hoy => new(DateTime.Today);
        public static FechaVencimiento Mañana => new(DateTime.Today.AddDays(1));
        public static FechaVencimiento EnDias(int dias) => new(DateTime.Today.AddDays(dias));
        public static FechaVencimiento EnSemanas(int semanas) => new(DateTime.Today.AddDays(semanas * 7));
        public static FechaVencimiento EnMeses(int meses) => new(DateTime.Today.AddMonths(meses));
        public static FechaVencimiento EnAños(int años) => new(DateTime.Today.AddYears(años));

        // Operadores
        public static implicit operator DateTime(FechaVencimiento fecha)
        {
            return fecha.Valor;
        }

        public static bool operator >(FechaVencimiento left, FechaVencimiento right)
        {
            return left.Valor > right.Valor;
        }

        public static bool operator <(FechaVencimiento left, FechaVencimiento right)
        {
            return left.Valor < right.Valor;
        }

        public static bool operator >=(FechaVencimiento left, FechaVencimiento right)
        {
            return left.Valor >= right.Valor;
        }

        public static bool operator <=(FechaVencimiento left, FechaVencimiento right)
        {
            return left.Valor <= right.Valor;
        }

        public static bool operator ==(FechaVencimiento left, FechaVencimiento right)
        {
            return left?.Valor == right?.Valor;
        }

        public static bool operator !=(FechaVencimiento left, FechaVencimiento right)
        {
            return !(left == right);
        }
    }
}