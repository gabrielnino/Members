using Domain;
using Domain.Interfaces.Entity;
using System.Diagnostics.CodeAnalysis;

namespace Autodesk.Domain
{
    // <summary>
    /// Represents a user.
    /// </summary>
    [method: SetsRequiredMembers]    // <summary>
    /// Represents a user.
    /// </summary>
    public class User(string id) : Entity(id)
    {

        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the user's display lastname.
        /// </summary>
        public string? Lastname { get; set; }

    }
}
