namespace Infrastructure.Constants
{
    public static class Message
    {
        public static class GuidValidator
        {
            public const string InvalidGuid = "The submitted value was invalid.";
            public const string Success = "Success";
        }

        public static class JwtHelper
        {
            public const string BearerPrefix = "Bearer ";
            public const string UserDataClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata";
            public const string TokenCannotBeWhitespace = "Token cannot be null or whitespace.";
            public const string TokenMustStartWithBearer = "Token must start with 'Bearer ' prefix.";
            public const string InvalidBearerTokenLength = "The length of the 'Bearer' string is less than the 'Bearer token' string.";
            public const string InvalidJwtPayloadFormat = "Invalid JWT payload format.";
            public const string InvalidBase64UrlFormat = "Invalid Base64Url format.";
            public const string Success = "Success";
        }
    }
}
