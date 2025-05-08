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

namespace Infrastructure.Repositories.Abstract.CRUD.Query.ReadFilterPage
{
    public abstract class ReadFilterPageRepository<T>(DbContext context, IErrorStrategyHandler errorStrategyHandler) : Read<T>(context), IReadFilterPage<T> where T : class
    {
        public async Task<Operation<IQueryable<T>>> ReadFilterPage(int pageNumber, int pageSize, string filter)
        {
            try
            {
                Expression<Func<T, bool>> predicate = GetPredicate(filter);
                IQueryable<T> result = await ReadPageByFilter(predicate, pageNumber, pageSize);
                var readFilterPageSuccess = ReadFilterPageLabels.ReadFilterPageSuccess;
                var messageSuccessfully = string.Format(readFilterPageSuccess, typeof(T).Name);
                return Operation<IQueryable<T>>.Success(result, messageSuccessfully);
            }
            catch (Exception ex)
            {
                return errorStrategyHandler.Fail<IQueryable<T>>(ex, "failedToUploadImage");
            }
        }

        public abstract Expression<Func<T, bool>> GetPredicate(string filter);
    }
}
