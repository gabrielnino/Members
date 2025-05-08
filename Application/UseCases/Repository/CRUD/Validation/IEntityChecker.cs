using Application.Result;
using Domain.Interfaces.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Repository.CRUD.Validation
{
    public interface IEntityChecker<T> where T : class, IEntity
    {
        Task<Operation<T>> HasEntity(string id);
    }
}
