namespace Persistence.CreateStruture.Constants
{
    /// <summary>
    /// Represents a Database.
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Contains names of database tables.
        /// </summary>
        public static class Tables
        {
            /// <summary>
            /// Name of the Users table.
            /// </summary>
            public const string Users = "Users";
            public const string Invoices = "Invoices";
            public const string Products = "Products";
        }

        /// <summary>
        /// Contains names of database indexes.
        /// </summary>
        public static class Index
        {
            /// <summary>
            ///  Unique index on the Email column in the Users table.
            /// </summary>
            public const string IndexEmail = "UC_Users_Email";
        }
    }
}
