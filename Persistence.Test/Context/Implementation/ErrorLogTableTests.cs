using System.Linq;
using Autodesk.Domain;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Persistence.Context.Implementation;
using Persistence.CreateStructure.Constants;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Xunit;

namespace Persistence.Context.Implementation.Tests
{
    public class ErrorLogTableTests
    {
        private static ModelBuilder CreateModelBuilder()
        {
            var conventions = new ConventionSet();
            return new ModelBuilder(conventions);
        }

        [Fact]
        public void Create_ConfiguresErrorLogEntityCorrectly()
        {
            // Arrange
            var modelBuilder = CreateModelBuilder();
            var columnTypes = new SQLite();

            // Act
            ErrorLogTable.Create(modelBuilder, columnTypes);
            var model = modelBuilder.Model;
            var entityType = model.FindEntityType(typeof(ErrorLog));

            // Assert: entity mapped to correct table
            Assert.NotNull(entityType);
            Assert.Equal(Database.Tables.ErrorLogs, entityType.GetTableName());

            // Assert: primary key on Id
            var pk = entityType.FindPrimaryKey();
            Assert.NotNull(pk);
            Assert.Single(pk.Properties);
            Assert.Equal(nameof(ErrorLog.Id), pk.Properties[0].Name);

            // Id column
            var idProp = entityType.FindProperty(nameof(ErrorLog.Id));
            Assert.Equal(columnTypes.TypeVar, idProp.GetColumnType());
            Assert.False(idProp.IsNullable);

            // Timestamp column
            var tsProp = entityType.FindProperty(nameof(ErrorLog.Timestamp));
            Assert.Equal(columnTypes.TypeTime, tsProp.GetColumnType());
            Assert.False(tsProp.IsNullable);

            // All other string properties should be TypeVar150, required, max length 150
            var stringProps = new[]
            {
                nameof(ErrorLog.Level),
                nameof(ErrorLog.Message),
                nameof(ErrorLog.ExceptionType),
                nameof(ErrorLog.StackTrace),
                nameof(ErrorLog.Context)
            };

            foreach (var propName in stringProps)
            {
                var prop = entityType.FindProperty(propName);
                Assert.Equal(columnTypes.TypeVar150, prop.GetColumnType());
                Assert.False(prop.IsNullable);
                Assert.Equal(150, prop.GetMaxLength());
            }
        }
    }
}
