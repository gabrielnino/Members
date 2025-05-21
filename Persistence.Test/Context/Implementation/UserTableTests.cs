using System.Linq;
using Autodesk.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Persistence.Context.Implementation;
using Persistence.CreateStruture.Constants;
using Persistence.CreateStruture.Constants.ColumnType.Database;
using Xunit;

namespace Persistence.Context.Implementation.Tests
{
    public class UserTableTests
    {
        private static ModelBuilder CreateModelBuilder()
        {
            var conventions = new ConventionSet();
            return new ModelBuilder(conventions);
        }

        [Fact]
        public void Create_ConfiguresUserEntityCorrectly()
        {
            // Arrange
            var modelBuilder = CreateModelBuilder();
            var columnTypes = new SQLite();

            // Act
            UserTable.Create(modelBuilder, columnTypes);
            var model = modelBuilder.Model;
            var userEntity = model.FindEntityType(typeof(User));

            // Assert: table name
            Assert.NotNull(userEntity);
            Assert.Equal(Database.Tables.Users, userEntity.GetTableName());

            // Id property and primary key
            var idProp = userEntity.FindProperty(nameof(User.Id));
            Assert.Equal(columnTypes.TypeVar, idProp.GetColumnType());
            Assert.False(idProp.IsNullable);
            var pk = userEntity.FindPrimaryKey();
            Assert.Single(pk.Properties);
            Assert.Equal(nameof(User.Id), pk.Properties[0].Name);

            // Email property
            var emailProp = userEntity.FindProperty(nameof(User.Email));
            Assert.Equal(columnTypes.TypeVar, emailProp.GetColumnType());
            Assert.False(emailProp.IsNullable);

            // Active property
            var activeProp = userEntity.FindProperty(nameof(User.Active));
            Assert.Equal(columnTypes.TypeBool, activeProp.GetColumnType());
            Assert.False(activeProp.IsNullable);

            // Unique index on Email
            var emailIndex = userEntity.GetIndexes()
                .SingleOrDefault(ix => ix.Properties.Single().Name == nameof(User.Email));
            Assert.NotNull(emailIndex);
            Assert.True(emailIndex.IsUnique);

        }
    }
}
