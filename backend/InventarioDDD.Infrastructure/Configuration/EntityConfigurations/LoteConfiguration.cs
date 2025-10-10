using InventarioDDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventarioDDD.Infrastructure.Configuration.EntityConfigurations
{
    public class LoteConfiguration : IEntityTypeConfiguration<Lote>
    {
        public void Configure(EntityTypeBuilder<Lote> builder)
        {
            builder.ToTable("Lotes");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Codigo)
                .IsRequired()
                .HasMaxLength(50);

            // Configurar FechaVencimiento como owned type (value object)
            builder.OwnsOne(l => l.FechaVencimiento, fv =>
            {
                fv.Property(f => f.Valor)
                    .HasColumnName("FechaVencimiento")
                    .IsRequired();
            });

            // Configurar PrecioUnitario como owned type (value object)
            builder.OwnsOne(l => l.PrecioUnitario, p =>
            {
                p.Property(pr => pr.Valor)
                    .HasColumnName("PrecioUnitario")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                p.Property(pr => pr.Moneda)
                    .HasColumnName("Moneda")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // Propiedad calculada - no se mapea a la base de datos
            builder.Ignore(l => l.Vencido);
        }
    }
}
