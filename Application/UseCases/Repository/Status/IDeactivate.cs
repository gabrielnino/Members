using Application.Result;

namespace Application.UseCases.Repository.Status
{
    public interface IDeactivate
    {
        Task<Operation<bool>> Deactivate(string id);
    }
}
