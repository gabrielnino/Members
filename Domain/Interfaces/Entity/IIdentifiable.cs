using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Entity
{
    /// <summary>
    /// Defines a contract for entities that can be unique.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        ///  Gets or sets the entity’s ID.
        /// </summary>
        string Id { get; }
    }
}
