namespace Application.Result
{
    public interface IErrorStrategyHandler
    {
        void LoadErrorMappings(string filePath);
        Operation<T> Fail<T>(Exception? ex, string errorMessage);

        Operation<T> Fail<T>(Exception? ex);
        Operation<T> Business<T>(string errorMessage);
        bool Any();
    }
}
