using InventarioDDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventarioDDD.Infrastructure.Configuration.EntityConfigurations
{
    public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
    {
        public void Configure(EntityTypeBuilder<Proveedor> builder)
        {
            builder.ToTable("Proveedores");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.NIT)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(p => p.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Telefono)
                .HasMaxLength(20);

            builder.Property(p => p.PersonaContacto)
                .HasMaxLength(200);

            // Configurar Direccion como owned type (value object)
            builder.OwnsOne(p => p.Direccion, d =>
            {
                d.Property(dir => dir.Calle)
                    .HasColumnName("DireccionCalle")
                    .HasMaxLength(200);

                d.Property(dir => dir.Ciudad)
                    .HasColumnName("DireccionCiudad")
                    .HasMaxLength(100)
                    .IsRequired();

                d.Property(dir => dir.Pais)
                    .HasColumnName("DireccionPais")
                    .HasMaxLength(100)
                    .IsRequired();

                d.Property(dir => dir.CodigoPostal)
                    .HasColumnName("DireccionCodigoPostal")
                    .HasMaxLength(20);
            });

            // Ignorar propiedades calculadas o de navegación
            builder.Ignore(p => p.IngredientesSuministrados);
        }
    }
}
