namespace Application.Result.EnumType.Extensions
{
    using Application.Constants;
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumMetadata : Attribute
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public EnumMetadata(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(Messages.EnumMetadata.ForNameOrDescription);
            }

            Name = name;
            Description = description;
        }
    }
}
