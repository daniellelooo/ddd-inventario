using InventarioDDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventarioDDD.Infrastructure.Configuration.EntityConfigurations
{
    public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
    {
        public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
        {
            builder.ToTable("MovimientosInventario");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Motivo)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(m => m.DocumentoReferencia)
                .HasMaxLength(100);

            builder.Property(m => m.Observaciones)
                .HasMaxLength(1000);

            builder.Property(m => m.Cantidad)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Configurar UnidadDeMedida como owned type (value object)
            builder.OwnsOne(m => m.UnidadDeMedida, u =>
            {
                u.Property(um => um.Nombre)
                    .HasColumnName("UnidadMedidaNombre")
                    .HasMaxLength(50)
                    .IsRequired();

                u.Property(um => um.Simbolo)
                    .HasColumnName("UnidadMedidaSimbolo")
                    .HasMaxLength(10)
                    .IsRequired();
            });

            // Configurar enum
            builder.Property(m => m.TipoMovimiento)
                .HasConversion<string>()
                .HasMaxLength(20);
        }
    }
}
