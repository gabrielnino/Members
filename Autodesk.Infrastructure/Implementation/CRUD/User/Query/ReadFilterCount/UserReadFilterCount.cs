using Application.Result;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Persistence.Context;
using Infrastructure.Repositories.Abstract.CRUD.Query.ReadFilterCount;
using System.Linq.Expressions;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Query.ReadFilterCount
{
    using User = Domain.User;
    public class UserReadFilterCount(DataContext context, IErrorStrategyHandler errorStrategyHandler) : ReadFilterCountRepository<User>(context, errorStrategyHandler), IUserReadFilterCount
    {
        public override Expression<Func<User, bool>> GetPredicate(string filter)
        {
            return user => user.Name.Contains(filter);
        }
    }
}
