using InventarioDDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventarioDDD.Infrastructure.Configuration.EntityConfigurations
{
    public class IngredienteConfiguration : IEntityTypeConfiguration<Ingrediente>
    {
        public void Configure(EntityTypeBuilder<Ingrediente> builder)
        {
            builder.ToTable("Ingredientes");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Nombre)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.Descripcion)
                .HasMaxLength(500);

            // Configurar CantidadEnStock como owned type (value object)
            builder.OwnsOne(i => i.CantidadEnStock, c =>
            {
                c.Property(cd => cd.Valor)
                    .HasColumnName("CantidadEnStock")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
            });

            // Configurar RangoDeStock como owned type (value object)
            builder.OwnsOne(i => i.RangoDeStock, r =>
            {
                r.Property(rd => rd.StockMinimo)
                    .HasColumnName("StockMinimo")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                r.Property(rd => rd.StockMaximo)
                    .HasColumnName("StockMaximo")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
            });

            // Configurar UnidadDeMedida como owned type (value object)
            builder.OwnsOne(i => i.UnidadDeMedida, u =>
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

            // Ignorar la colección de lotes (se maneja por el aggregate)
            builder.Ignore(i => i.Lotes);
        }
    }
}
