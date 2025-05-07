using Domain.Interfaces.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Entity : IEntity
    {

        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// Required by the <see cref="IIdentifiable"/> interface.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier for the entity.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is empty or consists only of whitespace.</exception>
        [SetsRequiredMembers]
        public Entity(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id cannot be empty or whitespace.", nameof(id));
            }
            Id = id;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this entity is active.
        /// </summary>
        /// <value>
        /// <c>true</c> if the entity is active; otherwise, <c>false</c>.
        /// </value>
        public bool Active { get; set; }
    }
}
