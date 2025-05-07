using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Entity
{
    /// <summary>
    /// Defines a contract for be an entity.
    /// </summary>
    public interface IEntity : IIdentifiable, IActivatable {}
}
