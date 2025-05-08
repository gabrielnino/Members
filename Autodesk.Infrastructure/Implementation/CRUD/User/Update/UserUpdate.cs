using Application.Result;
using Application.UseCases.Repository.CRUD;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Persistence.Context;
using Infrastructure.Repositories.Abstract.CRUD.Update;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Update
{
    using User = Domain.User;
    public class UserUpdate(DataContext context, IErrorStrategyHandler errorStrategyHandler, IUtilEntity<User> utilEntity) : UpdateRepository<User>(context, errorStrategyHandler, utilEntity), IUserUpdate
    {
        public override async Task<Operation<User>> UpdateEntity(User entityModified, User entityUnmodified)
        {
            var email = entityModified?.Email ?? string.Empty;
            var id = entityModified?.Id ?? string.Empty;
            var userByEmail = await ReadFilter(p => (p.Email ?? string.Empty).Equals(email) && !p.Id.Equals(id));
            var updateSuccessfullySearchGeneric = UserUpdateLabels.UpdateSuccessfullySearchGeneric;
            var successMessage = string.Format(updateSuccessfullySearchGeneric, typeof(User).Name);
            return Operation<User>.Success(entityUnmodified, successMessage);
        }

    }
}
