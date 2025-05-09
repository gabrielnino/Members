using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Pagination
{
    /// <summary>
    /// A page of items of type T plus the cursor to fetch the next page.
    /// </summary>
    public sealed record PagedResult<T>
    {
        /// <summary>
        /// The items in the current page.
        /// </summary>
        public IEnumerable<T> Items { get; init; } = Array.Empty<T>();

        /// <summary>
        /// The opaque cursor to send back in the next request, or null if there is no more data.
        /// </summary>
        public string? NextCursor { get; init; }
    }
}
