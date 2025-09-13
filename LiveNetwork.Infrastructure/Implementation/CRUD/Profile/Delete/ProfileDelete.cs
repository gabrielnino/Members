using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Infrastructure.Repositories.Abstract.CRUD.Delete;
using LiveNetwork.Application.UseCases.CRUD.Profile;
using LiveNetwork.Application.UseCases.CRUD.Profile.Query;
using Persistence.Context.Interface;

namespace LiveNetwork.Infrastructure.Implementation.CRUD.Profile.Delete
{
    using Profile = Domain.Profile;
    public class ProfileDelete(IUnitOfWork unitOfWork, IErrorHandler errorHandler, IErrorLogCreate errorLogCreate, IProfileRead profileRead) : DeleteRepository<Profile>(unitOfWork), IProfileDelete
    {
        public async Task<Operation<bool>> DeleteProfileAsync(string id)
        {
            try
            {
                var result = await DeleteEntity(id);
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
