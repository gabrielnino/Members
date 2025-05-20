using Autodesk.Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStruture.Constants.ColumnType;
using Persistence.CreateStruture.Constants;

namespace Persistence.Context.Implementation
{
    public static class TableProducts
    {
        public static void SetTableProducts(ModelBuilder modelBuilder, IColumnTypes columnTypes)
        {
            // Map Product → Products table
            modelBuilder.Entity<Product>().ToTable(Database.Tables.Products);
            modelBuilder.Entity<Product>(entity =>
            {
                // Primary key
                entity.Property(p => p.Id)
                      .HasColumnType(columnTypes.TypeVar)
                      .IsRequired();
                entity.HasKey(p => p.Id);

                // Name: varchar(100), required
                entity.Property(p => p.Name)
                      .HasColumnType(columnTypes.TypeVar)
                      .HasMaxLength(100)
                      .IsRequired();

                // Description: varchar(500), optional
                entity.Property(p => p.Description)
                      .HasColumnType(columnTypes.TypeVar150)
                      .HasMaxLength(500);

                // Price: decimal → stored as TEXT or REAL, required
                entity.Property(p => p.Price)
                      .HasColumnType(columnTypes.TypeVar)
                      .IsRequired();

                // Foreign key back to Invoice
                entity.Property<string>("InvoiceId")
                      .HasColumnType(columnTypes.TypeVar)
                      .IsRequired();
                entity.HasIndex("InvoiceId"); // for fast lookups

                entity.HasOne<Invoice>()
                      .WithMany(i => i.Products)
                      .HasForeignKey("InvoiceId")
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
