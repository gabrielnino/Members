using Persistence.CreateStructure.Constants.ColumnType.Database;

namespace Persistence.Test.CreateStructure.Constants.ColumnType.Database
{
    public class SQLiteColumnTypesTests
    {
        private readonly SQLite _types = new SQLite();

        [Fact]
        public void Integer_Returns_INTEGER()
        {
            Assert.Equal("INTEGER", _types.Integer);
        }

        [Fact]
        public void Long_Returns_INTEGER()
        {
            Assert.Equal("INTEGER", _types.Long);
        }

        [Fact]
        public void TypeBool_Returns_INTEGER()
        {
            Assert.Equal("INTEGER", _types.TypeBool);
        }

        [Fact]
        public void TypeTime_Returns_TEXT()
        {
            Assert.Equal("TEXT", _types.TypeTime);
        }

        [Theory]
        [InlineData("TypeVar", "TEXT")]
        [InlineData("TypeVar50", "TEXT")]
        [InlineData("TypeVar150", "TEXT")]
        [InlineData("TypeVar64", "TEXT")]
        public void TypeVarieties_Returns_TEXT(string propertyName, string expected)
        {
            var actual = propertyName switch
            {
                nameof(_types.TypeVar) => _types.TypeVar,
                nameof(_types.TypeVar50) => _types.TypeVar50,
                nameof(_types.TypeVar150) => _types.TypeVar150,
                nameof(_types.TypeVar64) => _types.TypeVar64,
                _ => null
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TypeBlob_Returns_BLOB()
        {
            Assert.Equal("BLOB", _types.TypeBlob);
        }

        [Fact]
        public void Strategy_Returns_SqliteAutoincrement()
        {
            Assert.Equal("Sqlite:Autoincrement", _types.Strategy);
        }

        [Fact]
        public void SqlStrategy_IsTrue()
        {
            Assert.True((bool)(_types.SqlStrategy ?? false));
        }

        [Fact]
        public void Name_IsEmptyString()
        {
            Assert.Equal(string.Empty, _types.Name);
        }

        [Fact]
        public void Value_IsNull()
        {
            Assert.Null(_types.Value);
        }
    }
}
