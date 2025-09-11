using Microsoft.EntityFrameworkCore;
using Persistence.Context.Implementation;
using Persistence.CreateStructure.Constants.ColumnType;

namespace Autodesk.Persistence.Test.Context
{
    public class DataContextTests
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

            public string TypeDateTime => "TEXT";

            public string TypeDateTimeOffset => "TEXT";
        }
    }
}
