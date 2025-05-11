using Autodesk.Shared.Pagination;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.Shared.Models
{
    public class UserQueryParams
    {
        public string? Id { get; set; }

        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string? Name { get; set; }

        [Range(1, PaginationDefaults.MaxPageSize,
           ErrorMessage = "PageSize must be between 1 and " + nameof(PaginationDefaults.MaxPageSize))]
        public int PageSize { get; set; } = PaginationDefaults.DefaultPageSize;

        public string? Cursor { get; set; }
    }
}
