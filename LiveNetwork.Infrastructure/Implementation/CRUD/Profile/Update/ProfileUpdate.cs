using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Infrastructure.Repositories.Abstract.CRUD.Update;
using LiveNetwork.Application.UseCases.CRUD.Profile;
using LiveNetwork.Application.UseCases.CRUD.Profile.Query;
using Persistence.Context.Interface;

namespace LiveNetwork.Infrastructure.Implementation.CRUD.Profile.Update
{
    using Profile = Domain.Profile;

    /// <summary>
    /// Handles updating a user record.
    /// </summary>
    public class ProfileUpdate(
        IUnitOfWork unitOfWork,
        IErrorHandler errorHandler,
        IErrorLogCreate errorLogCreate,
        IProfileRead ProfileRead
    ) : UpdateRepository<Profile>(unitOfWork), IProfileUpdate
    {
        public override Profile ApplyUpdates(Profile modified, Profile unmodified)
        {
            unmodified.FullName = modified.FullName;
            return unmodified;
        }

        public async Task<Operation<bool>> UpdateProfileAsync(Profile entity)
        {
            try
            {
                var result = await UpdateEntity(entity);
                await unitOfWork.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                return errorHandler.Fail<bool>(ex, errorLogCreate);
            }
        }
    }
}
