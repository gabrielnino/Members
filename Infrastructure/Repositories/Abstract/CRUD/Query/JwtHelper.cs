using Application.Result;
using Infrastructure.Constants;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Abstract.CRUD.Query
{
    public static class JwtHelper
    {
        public static Operation<string> ExtractJwtPayload(string bearerToken)
        {
            var strategy = new BusinessStrategy<string>();
            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                var tokenCannotBeWhitespace = Message.JwtHelper.TokenCannotBeWhitespace;
                return OperationStrategy<string>.Fail(tokenCannotBeWhitespace, strategy);
            }

            if (!bearerToken.StartsWith(Message.JwtHelper.BearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var tokenMustStartWithBearer = Message.JwtHelper.TokenMustStartWithBearer;
                return OperationStrategy<string>.Fail(tokenMustStartWithBearer, strategy);
            }

            if (Message.JwtHelper.BearerPrefix.Length >= bearerToken.Length)
            {
                var invalidBearerTokenLength = Message.JwtHelper.InvalidBearerTokenLength;
                return OperationStrategy<string>.Fail(invalidBearerTokenLength, strategy);
            }

            var jwt = bearerToken[Message.JwtHelper.BearerPrefix.Length..].Trim();
            var result = ExtractPayloadFromJwt(jwt);
            if (!result.IsSuccessful)
            {
                return result;
            }

            var data = result.Data ?? string.Empty;
            return ParsePayloadForUserData(data);
        }

        private static Operation<string> ExtractPayloadFromJwt(string jwt)
        {
            var tokenParts = jwt.Split('.');
            if (tokenParts.Length != 3)
            {
                var invalidJwtPayloadFormat = Message.JwtHelper.InvalidJwtPayloadFormat;
                var strategy = new BusinessStrategy<string>();
                return OperationStrategy<string>.Fail(invalidJwtPayloadFormat, strategy);
            }

            return Base64UrlDecode(tokenParts[1]);
        }

        private static Operation<string> ParsePayloadForUserData(string payload)
        {
            try
            {
                var jsonObject = JObject.Parse(payload);
                var idPayload = jsonObject[Message.JwtHelper.UserDataClaim]?.ToString() ?? string.Empty;
                return Operation<string>.Success(RemoveFirstAndLastCharacters(idPayload), Message.JwtHelper.Success);
            }
            catch
            {
                var strategy = new BusinessStrategy<string>();
                var invalidBearerTokenLength = Message.JwtHelper.InvalidBearerTokenLength;
                return OperationStrategy<string>.Fail(invalidBearerTokenLength, strategy);
            }
        }

        private static string RemoveFirstAndLastCharacters(string input)
        {
            return input.Substring(1, input.Length - 2);
        }

        private static Operation<string> Base64UrlDecode(string base64Url)
        {
            var padded = base64Url.Length % 4 == 0 ? base64Url : GetBase64Url(base64Url);
            var base64 = padded.Replace("_", "/").Replace("-", "+");
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                return Operation<string>.Success(Encoding.UTF8.GetString(bytes), Message.JwtHelper.Success);
            }
            catch
            {
                var strategy = new BusinessStrategy<string>();
                return OperationStrategy<string>.Fail(Message.JwtHelper.InvalidBase64UrlFormat, strategy);
            }
        }

        private static string GetBase64Url(string base64Url)
        {
            return string.Concat(base64Url, "====".AsSpan(base64Url.Length % 4));
        }
    }
}
