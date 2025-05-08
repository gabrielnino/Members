using Application.Result;

namespace Application.UseCases.Repository.Status
{
    public interface IActivate
    {
        Task<Operation<bool>> Activate(string id);
    }
}
