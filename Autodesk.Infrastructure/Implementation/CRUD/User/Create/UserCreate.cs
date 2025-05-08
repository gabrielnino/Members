using Application.Result;
using Application.UseCases.Repository.CRUD;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Persistence.Context;
using Infrastructure.Repositories.Abstract.CRUD.Create;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Create
{
    using User = Domain.User;
    public class UserCreate(DataContext context, IUtilEntity<User> utilEntity, IErrorStrategyHandler errorStrategyHandler) : CreateRepository<User>(context, utilEntity, errorStrategyHandler), IUserCreate
    {
        protected override async Task<Operation<User>> CreateEntity(User entity)
        {
            var email = entity?.Email ?? string.Empty;
            var userByEmail = await ReadFilter(p => p.Email == email);
            var userExistByEmail = userByEmail.FirstOrDefault();
            if (userExistByEmail != null)
            {
                var createFailedAlreadyRegisteredEmail = UserCreateLabels.CreateAlreadyRegisteredErrorEmail;
                return OperationStrategy<User>.Fail(createFailedAlreadyRegisteredEmail, new BusinessStrategy<User>());
            }

            return Operation<User>.Success(entity);
        }
    }
}
