using Application.Result;
using Application.UseCases.Repository.CRUD.Query;
using Infrastructure.Constants;
using Infrastructure.Result;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Abstract.CRUD.Query.ReadFilterCount
{
    public abstract class ReadFilterCountRepository<T>(DbContext context, IErrorStrategyHandler errorStrategyHandler) : Read<T>(context), IReadFilterCount<T> where T : class
    {
        public async Task<Operation<int>> ReadFilterCount(string filter)
        {
            try
            {
                Expression<Func<T, bool>> predicate = GetPredicate(filter);
                int result = await ReadCountFilter(predicate);
                var readFilterCountSuccess = "ReadFilterCountSuccess";
                return Operation<int>.Success(result, readFilterCountSuccess);
            }
            catch (Exception ex)
            {
                return errorStrategyHandler.Fail<int>(ex);
            }
        }

        public abstract Expression<Func<T, bool>> GetPredicate(string filter);
    }
}
