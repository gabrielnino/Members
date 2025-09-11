using Application.Result;

namespace LiveNetwork.Application.UseCases.CRUD.Profile
{
    using Profile = Domain.Profile;
    public interface IProfileUpdate
    {
        Task<Operation<bool>> UpdateProfileAsync(Profile entity);
    }
}
