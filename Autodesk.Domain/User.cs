using Domain.Interfaces.Entity;
using System.Diagnostics.CodeAnalysis;

namespace Autodesk.Domain
{
    // <summary>
    /// Represents a user.
    /// </summary>
    public class User : IEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier for the user.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is empty or consists only of whitespace.</exception>
        [SetsRequiredMembers]
        public User(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id cannot be empty or whitespace.", nameof(id));
            }
            Id = id;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// Required by the <see cref="IIdentifiable"/> interface.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the user's display lastname.
        /// </summary>
        public string? Lastname { get; set; }

        public bool Active { get; set; }
    }
}
