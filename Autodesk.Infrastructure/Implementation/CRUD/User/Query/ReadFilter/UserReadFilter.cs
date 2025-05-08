using Application.Result;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Persistence.Context;
using Infrastructure.Repositories.Abstract.CRUD.Query.ReadFilter;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Query.ReadFilter
{
    using User = Domain.User;
    public class UserReadFilter(DataContext context, IErrorStrategyHandler errorStrategyHandler) : ReadFilterRepository<User>(context, errorStrategyHandler), IUserReadFilter
    {

    }
}
