using Application.Common.Pagination;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Reactive.Linq;

namespace Autodesk.Api.Controllers.api.v1.Autodesk
{
    /// <summary>
    /// Controller for managing User entities via REST API.
    /// </summary>
    [ApiController]
    [Route("api/v1/reactive")]
    public class ReactiveController(IUserRead readFilterCursor) : ControllerBase
    {
        // Dependency-injected use-case services
        private readonly IUserRead _read = readFilterCursor;

        /// <summary>
        /// Retrieve a paged list of Users using cursor-based pagination.
        /// </summary>
        /// <param name="qp">Query parameters for paging and filtering.</param>
        /// <returns>PagedResult of User or BadRequest.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async IAsyncEnumerable<User> Read([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var userObservable = _read.StreamUsers();

            // 2) Convert IObservable<User> into IAsyncEnumerable<User> using the channel-based extension
            await foreach (var user in userObservable
                                     .ToAsyncEnumerableViaChannel(cancellationToken)
                                     .WithCancellation(cancellationToken))
            {
                yield return user;
            }
        }

    }
}