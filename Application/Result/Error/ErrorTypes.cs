using Application.Result.EnumType.Extensions;

namespace Application.Result.Error
{
    public enum ErrorTypes
    {
        [EnumMetadata("NONE", "Represents no error.")]
        None,
        [EnumMetadata("BUSINESS_VALIDATION_ERROR", "Represents errors related to business logic validation.")]
        BusinessValidation,
        [EnumMetadata("DATABASE_ERROR", "Represents errors when interacting with the database.")]
        Database,
        [EnumMetadata("EXTERNAL_SERVICES_ERROR", "Represents errors when interacting with external services.")]
        ExternalService,
        [EnumMetadata("UNEXPECTED_ERROR", "Represents any unexpected or unclassified errors.")]
        Unexpected,
        [EnumMetadata("DATA_SUBMITTED_INVALID", "Represents errors due to invalid data submission.")]
        InvalidData,
        [EnumMetadata("CONFIGURATION_MISSING_ERROR", "Represents errors due to missing configurations.")]
        ConfigMissing,
        [EnumMetadata("NETWORK_ERROR", "Represents errors due to network issues.")]
        Network,
        [EnumMetadata("USER_INPUT_ERROR", "Represents errors related to user input.")]
        UserInput,
        [EnumMetadata("NONE_FOUND_ERROR", "Represents errors where a requested resource is not found.")]
        NotFound,
        [EnumMetadata("AUTHENTICATION_ERROR", "Represents errors related to user authentication.")]
        Authentication,
        [EnumMetadata("AUTHORIZATION_ERROR", "Represents errors related to user authorization or permissions.")]
        Authorization,
        [EnumMetadata("RESOURCE_ERROR", "Represents errors related to resource allocation or access.")]
        Resource,
        [EnumMetadata("TIMEOUT_ERROR", "Represents errors due to operation timeouts.")]
        Timeout,
        [EnumMetadata("NULL_EXCEPTION_STRATEGY", "The NullExceptionStrategy occurs when the ErrorMappings dictionary is not properly initialized.")]
        NullExceptionStrategy
    }
}
