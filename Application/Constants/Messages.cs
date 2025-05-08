namespace Application.Constants
{
    public static class Messages
    {
        public static class InvalidOperation
        {
            public const string NullMessage = "The 'message' parameter cannot be null, empty, or whitespace.";
        }

        public static class Operation
        {
            public const string InvalidOperation = "This method can only be used if the value of IsSuccessful is false.";
        }


        public static class EnumExtensions
        {
            public const string Unknown = "UNKNOWN";
            public const string DescriptionNotAvailable = "Description not available.";
            public const string NoEnumValueFound = "No enum value found for {0} in {1}";
        }

        public static class EnumMetadata
        {
            public const string ForNameOrDescription = "For name or description, null, empty, and whitespace are not allowed.";
        }

        public static class ActionType
        {
            public const string KeyAdd = "Add";
            public const string Add = "Add a new record.";
            public const string KeyModified = "Modified";
            public const string Modified = "Modify an existing record.";
            public const string KeyRemove = "Remove";
            public const string Remove = "Remove an existing record.";
            public const string KeyDeactivate = "Deactivate";
            public const string Deactivate = "Deactivate an existing record.";
            public const string KeyActivate = "Activate";
            public const string Activate = "Activate a deactivated record.";
            public const string KeyGetUserById = "GetUserById";
            public const string GetUserById = "Retrieve a user by their ID.";
            public const string KeyGetAllByFilter = "GetAllByFilter";
            public const string GetAllByFilter = "Retrieve all records that match a given filter.";
            public const string KeyGetPageByFilter = "GetPageByFilter";
            public const string GetPageByFilter = "Retrieve a page of records that match a given filter.";
            public const string KeyGetCountFilter = "GetCountFilter";
            public const string GetCountFilter = "Get the count of records that match a given filter.";
            public const string ArgumentNullExceptionName = "The 'name' parameter cannot be null, empty, or whitespace.";
            public const string ArgumentNullExceptionDescription = "The 'description' parameter cannot be null, empty, or whitespace.";
            public const string InvalidOperationException = "An operation with the name '{0}' already exists.";
        }
    }
}
