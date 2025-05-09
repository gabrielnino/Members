using Application.Result.EnumType.Extensions;
using Application.Result.Error;

namespace Application.Result
{
    public interface IErrorCreationStrategy<T>
    {
        Operation<T> CreateFailure(string message);
        Operation<T> CreateFailure();
    }
    public abstract class ErrorStrategyBase<T>(ErrorTypes errorType) : IErrorCreationStrategy<T>
    {
        private readonly ErrorTypes _errorType = errorType;

        public Operation<T> CreateFailure(string message) => Operation<T>.Failure(message, _errorType);
        public Operation<T> CreateFailure() => Operation<T>.Failure(_errorType.GetDescription(), _errorType);
    }
    public class BusinessStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.BusinessValidation);
    public class ConfigMissingStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.ConfigMissing);
    public class DatabaseStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Database);
    public class InvalidDataStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.InvalidData);
    public class ExternalServiceStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.ExternalService);
    public class UnexpectedErrorStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Unexpected);
    public class NetworkErrorStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Network);
    public class NullExceptionStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.NullExceptionStrategy);
    public class UserInputStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.UserInput);
    public class NotFoundStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.NotFound);
    public class AuthenticationStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Authentication);
    public class AuthorizationStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Authorization);
    public class ResourceStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Resource);
    public class TimeoutStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.Timeout);
    public class NoneStrategy<T>() : ErrorStrategyBase<T>(ErrorTypes.None);
    public static class OperationStrategy<T>
    {
        private const string DefaultErrorMessage = "Unknown Error";
        public static Operation<T> Fail(string? message, IErrorCreationStrategy<T> strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy), "Strategy cannot be null.");
            var finalMessage = string.IsNullOrWhiteSpace(message) ? DefaultErrorMessage : message;
            return strategy.CreateFailure(finalMessage);
        }
    }
}
