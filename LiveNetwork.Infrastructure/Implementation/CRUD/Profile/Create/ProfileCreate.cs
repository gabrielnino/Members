using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Infrastructure.Repositories.Abstract.CRUD.Create;
using LiveNetwork.Application.UseCases.CRUD.Profile;
using Infrastructure.Repositories.Abstract.CRUD.Create;
    using LiveNetwork.Application.UseCases.CRUD.Profile.Query;
using Persistence.Context.Interface;

namespace LiveNetwork.Infrastructure.Implementation.CRUD.Profile.Create
{
    using Profile = Domain.Profile;
    public class ProfileCreate(IUnitOfWork unitOfWork, IErrorHandler errorHandler, IErrorLogCreate errorLogCreate, IProfileRead profileRead) : CreateRepository<Profile>(unitOfWork), IProfileCreate
    {
        public async Task<Operation<Profile>> CreateProfileAsync(Profile entity)
        {
            try
            {
                await CreateEntity(entity);
                await unitOfWork.CommitAsync();
                return Operation<Profile>.Success(entity);
            }
            catch (Exception ex)
            {
                return errorHandler.Fail<Profile>(ex, errorLogCreate);
            }
        }

        public async Task<Operation<List<Profile>>> CreateProfilesAsync(List<Profile> entities)
        {
            try
            {
                await CreateEntities(entities);
                await unitOfWork.CommitAsync();
                return Operation<List<Profile>>.Success(entities);
            }
            catch (Exception ex)
            {
                return errorHandler.Fail<List<Profile>>(ex, errorLogCreate);
            }
        }
    }
}
