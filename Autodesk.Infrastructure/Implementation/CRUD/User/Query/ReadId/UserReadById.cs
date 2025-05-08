using Application.Result;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Persistence.Context;
using Infrastructure.Repositories.Abstract.CRUD.Query.ReadId;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Query.ReadId
{
    using User = Domain.User;
    public class UserReadById(DataContext context, IErrorStrategyHandler errorStrategyHandler) : ReadByIdRepository<User>(context, errorStrategyHandler), IUserReadById
    {
    }
}
