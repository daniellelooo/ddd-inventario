using InventarioDDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventarioDDD.Infrastructure.Configuration.EntityConfigurations
{
    public class OrdenDeCompraConfiguration : IEntityTypeConfiguration<OrdenDeCompra>
    {
        public void Configure(EntityTypeBuilder<OrdenDeCompra> builder)
        {
            builder.ToTable("OrdenesCompra");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Numero)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.Observaciones)
                .HasMaxLength(1000);

            // Configurar Cantidad como owned type (value object)
            builder.OwnsOne(o => o.Cantidad, c =>
            {
                c.Property(cd => cd.Valor)
                    .HasColumnName("Cantidad")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
            });

            // Configurar PrecioUnitario como owned type (value object)
            builder.OwnsOne(o => o.PrecioUnitario, p =>
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

            // Configurar enum
            builder.Property(o => o.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Ignorar propiedades de navegación
            builder.Ignore(o => o.Ingrediente);
            builder.Ignore(o => o.Proveedor);
        }
    }
}
