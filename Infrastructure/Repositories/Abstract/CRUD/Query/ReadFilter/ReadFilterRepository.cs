using Application.Result;
using Application.UseCases.Repository.CRUD.Query;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Abstract.CRUD.Query.ReadFilter
{
    public abstract class ReadFilterRepository<T>(DbContext context, IErrorStrategyHandler errorStrategyHandler) : Read<T>(context), IReadFilter<T> where T : class
    {
        public new async Task<Operation<IQueryable<T>>> ReadFilter(Expression<Func<T, bool>> predicate)
        {
            try
            {
                IQueryable<T> result = await base.ReadFilter(predicate);
                var readFilterSuccess = ReadFilterLabels.ReadFilterSuccess;
                var messageSuccessfully = string.Format(readFilterSuccess, typeof(T).Name);
                return Operation<IQueryable<T>>.Success(result, messageSuccessfully);
            }
            catch (Exception ex)
            {
                return errorStrategyHandler.Fail<IQueryable<T>>(ex);
            }
        }
    }
}
