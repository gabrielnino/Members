using Autodesk.Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStruture.Constants.ColumnType;
using Persistence.CreateStruture.Constants;
namespace Persistence.Context.Implementation
{
    public static class InvoiceTable
    {
        public static void Invoices(ModelBuilder modelBuilder, IColumnTypes columnTypes)
        {
            modelBuilder.Entity<Invoice>().ToTable(Database.Tables.Invoices);
            modelBuilder.Entity<Invoice>(entity =>
            {
                // Primary key
                entity.Property(i => i.Id)
                      .HasColumnType(columnTypes.TypeVar)
                      .IsRequired();
                entity.HasKey(i => i.Id);

                // InvoiceNumber: varchar(50), required
                entity.Property(i => i.InvoiceNumber)
                      .HasColumnType(columnTypes.TypeVar)
                      .HasMaxLength(50)
                      .IsRequired();

                // InvoiceDate: stored as TEXT (ISO date), required
                entity.Property(i => i.InvoiceDate)
                      .HasColumnType(columnTypes.TypeTime)
                      .IsRequired();

                // CustomerName: varchar(150), required
                entity.Property(i => i.CustomerName)
                      .HasColumnType(columnTypes.TypeVar150)
                      .HasMaxLength(150)
                      .IsRequired();

                // TotalAmount: stored as TEXT (or REAL), required
                entity.Property(i => i.TotalAmount)
                      .HasColumnType(columnTypes.TypeVar)
                      .IsRequired();

                // One-to-many: Invoice → Products
                entity.HasMany(i => i.Products)
                      .WithOne()
                      .HasForeignKey("InvoiceId")
                      .IsRequired(false);
            });
        }
    }

}
