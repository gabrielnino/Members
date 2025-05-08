using Application.Constants;

namespace Application.Result.EnumType
{
    public class ActionType
    {
        private static readonly Dictionary<string, ActionType> _operations = [];
        public string Name { get; private set; }
        public string Description { get; private set; }

        private ActionType(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public static ActionType Add => Register(Messages.ActionType.KeyAdd, Messages.ActionType.Add);
        public static ActionType Modified => Register(Messages.ActionType.KeyModified, Messages.ActionType.Modified);
        public static ActionType Remove => Register(Messages.ActionType.KeyRemove, Messages.ActionType.Remove);
        public static ActionType Deactivate => Register(Messages.ActionType.KeyDeactivate, Messages.ActionType.Deactivate);
        public static ActionType Activate => Register(Messages.ActionType.KeyActivate, Messages.ActionType.Activate);
        public static ActionType GetUserById => Register(Messages.ActionType.KeyGetUserById, Messages.ActionType.GetUserById);
        public static ActionType GetAllByFilter => Register(Messages.ActionType.KeyGetAllByFilter, Messages.ActionType.GetAllByFilter);
        public static ActionType GetPageByFilter => Register(Messages.ActionType.KeyGetPageByFilter, Messages.ActionType.GetPageByFilter);
        public static ActionType GetCountFilter => Register(Messages.ActionType.KeyGetCountFilter, Messages.ActionType.GetCountFilter);

        private static ActionType Register(string name, string description)
        {
            if (_operations.TryGetValue(name, out ActionType? value))
            {
                return value;
            }

            var operation = new ActionType(name, description);
            _operations[name] = operation;
            return operation;
        }

        public static ActionType CreateCustomOperation(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), Messages.ActionType.ArgumentNullExceptionName);
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description), Messages.ActionType.ArgumentNullExceptionDescription);
            }

            if (_operations.ContainsKey(name))
            {
                throw new InvalidOperationException(string.Format(Messages.ActionType.InvalidOperationException, name));
            }

            return Register(name, description);
        }

        public static string? GetName(ActionType enumType)
        {
            ArgumentNullException.ThrowIfNull(enumType);
            return enumType.Name;
        }
    }
}
