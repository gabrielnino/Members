namespace Application.Result.EnumType.Extensions
{
    using Application.Constants;
    using System;
    using System.Reflection;

    public static class EnumExtensions
    {
        public static string GetCustomName<TEnum>(this TEnum enumValue)
            where TEnum : struct, Enum
        {
            return GetEnumMetadata(enumValue)?.Name ?? Messages.EnumExtensions.Unknown;
        }

        public static string GetDescription<TEnum>(this TEnum enumValue)
            where TEnum : struct, Enum
        {
            return GetEnumMetadata(enumValue)?.Description ?? Messages.EnumExtensions.DescriptionNotAvailable;
        }

        private static EnumMetadata? GetEnumMetadata<TEnum>(TEnum enumValue)
            where TEnum : Enum
        {
            var type = enumValue.GetType();
            var name = Enum.GetName(type, enumValue);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field?.GetCustomAttribute<EnumMetadata>(false) is EnumMetadata attribute)
                {
                    return attribute;
                }
            }

            return null;
        }
    }
}
