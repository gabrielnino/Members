using Application.Result;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Persistence.Context;
using Infrastructure.Repositories.Abstract.CRUD.Delete;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Delete
{
    using User = Domain.User;
    public class UserDelete(DataContext context, IErrorStrategyHandler errorStrategyHandler) : DeleteRepository<User>(context, errorStrategyHandler), IUserDelete
    {
       
    }
}
