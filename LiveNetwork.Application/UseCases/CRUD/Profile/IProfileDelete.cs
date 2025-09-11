using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Result;

namespace LiveNetwork.Application.UseCases.CRUD.Profile
{
    public interface IProfileDelete
    {
        Task<Operation<bool>> DeleteProfileAsync(string id);
    }
}
