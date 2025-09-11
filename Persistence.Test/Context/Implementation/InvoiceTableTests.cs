using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Persistence.Context.Implementation;
using Persistence.CreateStructure.Constants;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Autodesk.Domain;
using Xunit;

namespace Persistence.Context.Implementation.Tests
{
    public class InvoiceTableTests
    {
        private static ModelBuilder CreateModelBuilder()
        {
            var conventions = new ConventionSet();
            return new ModelBuilder(conventions);
        }

        [Fact]
        public void Create_ConfiguresInvoiceEntityCorrectly()
        {
            // Arrange
            var modelBuilder = CreateModelBuilder();
            var columnTypes = new SQLite();

            // Act
            InvoiceTable.Create(modelBuilder, columnTypes);
            var model = modelBuilder.Model;
            var invoiceEntity = model.FindEntityType(typeof(Invoice));

            // Assert: table name
            Assert.NotNull(invoiceEntity);
            Assert.Equal(Database.Tables.Invoices, invoiceEntity.GetTableName());

            // Id property
            var idProp = invoiceEntity.FindProperty(nameof(Invoice.Id));
            Assert.Equal(columnTypes.TypeVar, idProp.GetColumnType());
            Assert.False(idProp.IsNullable);
            Assert.Equal(nameof(Invoice.Id), invoiceEntity.FindPrimaryKey().Properties.Single().Name);

            // InvoiceNumber
            var numProp = invoiceEntity.FindProperty(nameof(Invoice.InvoiceNumber));
            Assert.Equal(columnTypes.TypeVar, numProp.GetColumnType());
            Assert.False(numProp.IsNullable);
            Assert.Equal(50, numProp.GetMaxLength());

            // InvoiceDate
            var dateProp = invoiceEntity.FindProperty(nameof(Invoice.InvoiceDate));
            Assert.Equal(columnTypes.TypeTime, dateProp.GetColumnType());
            Assert.False(dateProp.IsNullable);

            // CustomerName
            var nameProp = invoiceEntity.FindProperty(nameof(Invoice.CustomerName));
            Assert.Equal(columnTypes.TypeVar150, nameProp.GetColumnType());
            Assert.False(nameProp.IsNullable);
            Assert.Equal(150, nameProp.GetMaxLength());

            // TotalAmount
            var amtProp = invoiceEntity.FindProperty(nameof(Invoice.TotalAmount));
            Assert.Equal(columnTypes.TypeVar, amtProp.GetColumnType());
            Assert.False(amtProp.IsNullable);
        }

        [Fact]
        public void Create_SetsUpProductsNavigationAndForeignKey()
        {
            // Arrange
            var modelBuilder = CreateModelBuilder();
            var columnTypes = new SQLite();

            // Act
            InvoiceTable.Create(modelBuilder, columnTypes);
            var model = modelBuilder.Model;
            var invoiceEntity = model.FindEntityType(typeof(Invoice));

            // The relationship to Products should be discovered
            var navigation = invoiceEntity.GetNavigations()
                .SingleOrDefault(n => n.Name == nameof(Invoice.Products));

            Assert.NotNull(navigation);

            // The dependent type should be Product
            Assert.Equal(typeof(Product), navigation.TargetEntityType.ClrType);

            // The foreign key property on Product should be "InvoiceId"
            var fk = navigation.ForeignKey;
            Assert.Single(fk.Properties);
            Assert.Equal("InvoiceId", fk.Properties.Single().Name);
        }
    }
}
