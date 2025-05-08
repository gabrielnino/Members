namespace Application.Result
{
    using Application.Result.Error;
    using InvalidOperation = Exceptions.InvalidOperation;
    using static Application.Constants.Messages;

    public class Operation<T> : Result<T>
    {
        private Operation()
        {
        }

        public static Operation<T> Success(T? data, string? message = "")
        {
            return new Operation<T>
            {
                IsSuccessful = true,
                Data = data,
                Message = message ?? string.Empty,
                Type = ErrorTypes.None
            };
        }

        public static Operation<T> Failure(string message, ErrorTypes errorTypes)
        {
            return new Operation<T>
            {
                IsSuccessful = false,
                Message = message,
                Type = errorTypes
            };
        }

        public Operation<U> AsType<U>()
        {
            EnsureIsFailure();
            return new Operation<U>
            {
                IsSuccessful = false,
                Message = this.Message,
                Type = this.Type
            };
        }

        public Operation<U> ConvertTo<U>() => AsType<U>();
        private void EnsureIsFailure()
        {
            if (IsSuccessful)
            {
                throw new InvalidOperation(Operation.InvalidOperation);
            }
        }
    }
}
