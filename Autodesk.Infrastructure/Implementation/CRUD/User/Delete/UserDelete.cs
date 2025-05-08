using Application.Result;
using Application.UseCases.Repository.CRUD;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Infrastructure.Implementation.CRUD.User.Create;
using Autodesk.Persistence.Context;
using Infrastructure.Repositories.Abstract.CRUD.Create;
using Infrastructure.Repositories.Abstract.CRUD.Delete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Delete
{
    using User = Domain.User;
    public class UserDelete(DataContext context, IErrorStrategyHandler errorStrategyHandler) : DeleteRepository<User>(context, errorStrategyHandler), IUserDelete
    {
       
    }
}
