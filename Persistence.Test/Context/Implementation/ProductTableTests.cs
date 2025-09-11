using System.Linq;
using Autodesk.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Persistence.Context.Implementation;
using Persistence.CreateStructure.Constants;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Xunit;

namespace Persistence.Context.Implementation.Tests
{
    public class ProductTableTests
    {
        private static ModelBuilder CreateModelBuilder()
        {
            var conventions = new ConventionSet();
            return new ModelBuilder(conventions);
        }

        [Fact]
        public void Create_ConfiguresProductEntityCorrectly()
        {
            // Arrange
            var modelBuilder = CreateModelBuilder();
            var columnTypes = new SQLite();

            // Act
            ProductTable.Create(modelBuilder, columnTypes);
            var model = modelBuilder.Model;
            var productType = model.FindEntityType(typeof(Product));

            // Assert: table name
            Assert.NotNull(productType);
            Assert.Equal(Database.Tables.Products, productType.GetTableName());

            // Id property
            var idProp = productType.FindProperty(nameof(Product.Id));
            Assert.Equal(columnTypes.TypeVar, idProp.GetColumnType());
            Assert.False(idProp.IsNullable);
            Assert.Equal(nameof(Product.Id), productType.FindPrimaryKey().Properties.Single().Name);

            // Name property
            var nameProp = productType.FindProperty(nameof(Product.Name));
            Assert.Equal(columnTypes.TypeVar, nameProp.GetColumnType());
            Assert.False(nameProp.IsNullable);
            Assert.Equal(100, nameProp.GetMaxLength());

            // Description property (optional)
            var descProp = productType.FindProperty(nameof(Product.Description));
            Assert.Equal(columnTypes.TypeVar150, descProp.GetColumnType());
            Assert.True(descProp.IsNullable);
            Assert.Equal(500, descProp.GetMaxLength());

            // Price property
            var priceProp = productType.FindProperty(nameof(Product.Price));
            Assert.Equal(columnTypes.TypeVar, priceProp.GetColumnType());
            Assert.False(priceProp.IsNullable);
        }

        [Fact]
        public void Create_SetsUpInvoiceForeignKeyAndIndex()
        {
            // Arrange
            var modelBuilder = CreateModelBuilder();
            var columnTypes = new SQLite();

            // Act
            ProductTable.Create(modelBuilder, columnTypes);
            var model = modelBuilder.Model;
            var productType = model.FindEntityType(typeof(Product));

            // Shadow FK property InvoiceId
            var fkProp = productType.FindProperty("InvoiceId");
            Assert.Equal(columnTypes.TypeVar, fkProp.GetColumnType());
            Assert.False(fkProp.IsNullable);

            // Index on InvoiceId
            var index = productType.GetIndexes()
                .SingleOrDefault(ix => ix.Properties.Single().Name == "InvoiceId");
            Assert.NotNull(index);

            // Foreign key relationship to Invoice
            var fk = productType.GetForeignKeys()
                .SingleOrDefault(f => f.PrincipalEntityType.ClrType == typeof(Invoice));
            Assert.NotNull(fk);
            Assert.Single(fk.Properties);
            Assert.Equal("InvoiceId", fk.Properties.Single().Name);
            Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
        }
    }
}
