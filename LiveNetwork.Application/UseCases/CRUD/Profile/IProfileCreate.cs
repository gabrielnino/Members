
using Application.Result;
namespace LiveNetwork.Application.UseCases.CRUD.Profile
{
    using Profile = Domain.Profile;
    public interface IProfileCreate
    {
        Task<Operation<Profile>> CreateProfileAsync(Profile entity);

        Task<Operation<List<Profile>>> CreateProfilesAsync(List<Profile> entities);
    }
}
