using Application.Result;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Persistence.Context;
using Infrastructure.Repositories.Abstract.CRUD.Query.ReadFilterPage;
using System.Linq.Expressions;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Query.ReadFilterPage
{
    using User = Domain.User;
    public class UserReadFilterPage(DataContext context, IErrorStrategyHandler errorStrategyHandler) : ReadFilterPageRepository<User>(context, errorStrategyHandler), IUserReadFilterPage
    {
        public override Expression<Func<User, bool>> GetPredicate(string filter)
        {
            return user => user.Name.Contains(filter);
        }
    }
}
