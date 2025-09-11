using Autodesk.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants;
using Persistence.Context.Implementation;

namespace Autodesk.Persistence.Test.Context
{
    public class DataHelperTests
    {
        // Fake IColumnTypes for testing
        private class FakeColumnTypes : IColumnTypes
        {
            public string Integer => "INTEGER";
            public string Long => "INTEGER";
            public string TypeBool => "INTEGER";
            public string TypeTime => "TEXT";
            public string TypeVar => "TEXT";
            public string TypeVar50 => "TEXT";
            public string TypeVar150 => "TEXT";
            public string TypeVar64 => "TEXT";
            public string TypeBlob => "BLOB";
            public string Strategy => "Sqlite:Autoincrement";
            public object? SqlStrategy => true;
            public string Name => string.Empty;
            public object? Value => null;
        }

        [Fact]
        public void SetTableUsers_ConfiguresEntityCorrectly()
        {
            // Arrange
            var conventions = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventions);
            var columnTypes = new FakeColumnTypes();

            // Act
            UserTable.Create(modelBuilder, columnTypes);
            var entityType = modelBuilder.Model.FindEntityType(typeof(User));
            Assert.NotNull(entityType);

            // Assert: table name
            Assert.Equal("Users", entityType.GetTableName());

            // Assert: Id column
            var idProp = entityType.FindProperty(nameof(User.Id));
            Assert.NotNull(idProp);
            Assert.Equal(columnTypes.TypeVar, idProp.GetColumnType());
            Assert.Contains(idProp, entityType.FindPrimaryKey().Properties);

            // Assert: Email column and unique index
            var emailProp = entityType.FindProperty(nameof(User.Email));
            Assert.NotNull(emailProp);
            Assert.False(emailProp.IsNullable);
            Assert.Equal(columnTypes.TypeVar, emailProp.GetColumnType());

            var emailIndex = entityType.GetIndexes()
                .Single(i => i.Properties.Any(p => p.Name == nameof(User.Email)));
            Assert.True(emailIndex.IsUnique);
            Assert.Equal(Database.Index.IndexEmail, emailIndex.GetDatabaseName());

            // Assert: Active column
            var activeProp = entityType.FindProperty(nameof(User.Active));
            Assert.NotNull(activeProp);
            Assert.False(activeProp.IsNullable);
            Assert.Equal(columnTypes.TypeBool, activeProp.GetColumnType());
        }
    }
}
