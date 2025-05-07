namespace Persistence.CreateStruture.Constants
{
    /// <summary>
    /// Represents a Database.
    /// </summary>
    internal static class Database
    {
        /// <summary>
        /// Contains names of database tables.
        /// </summary>
        internal static class Tables
        {
            /// <summary>
            /// Name of the Users table.
            /// </summary>
            public const string Users = "Users";
        }

        /// <summary>
        /// Contains names of database indexes.
        /// </summary>
        internal static class Index
        {
            /// <summary>
            ///  Unique index on the Email column in the Users table.
            /// </summary>
            public const string IndexEmail = "UC_Users_Email";
        }
    }
}
