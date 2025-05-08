using Application.Result.EnumType.Extensions;
using Application.Result.Error;

namespace Application.Result
{
    public class Result<T>
    {
        public ErrorTypes Type { get; set; }
        public bool IsSuccessful { get; protected set; }
        public T? Data { get; protected set; }
        public string? Message { get; protected set; }
        public string Error => this.Type.GetCustomName();
    }
}
