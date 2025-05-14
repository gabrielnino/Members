using Autodesk.Shared.Pagination;
using System.ComponentModel.DataAnnotations;

namespace Autodesk.Shared.Models
{
    /// <summary>
    /// Parameters for retrieving users with optional filters and paging.
    /// </summary>
    public class UserQueryParams
    {
        /// <summary>
        /// Optional user identifier to filter the results.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Optional user name filter (max 100 characters).
        /// </summary>
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string? Name { get; set; }

        /// <summary>
        /// Number of items per page (1 to MaxPageSize).
        /// </summary>
        [Range(1, PaginationDefaults.MaxPageSize,
           ErrorMessage = "PageSize must be between 1 and " + nameof(PaginationDefaults.MaxPageSize))]
        public int PageSize { get; set; } = PaginationDefaults.DefaultPageSize;

        /// <summary>
        /// Cursor for fetching the next page of results.
        /// </summary>
        public string? Cursor { get; set; }
    }
}
