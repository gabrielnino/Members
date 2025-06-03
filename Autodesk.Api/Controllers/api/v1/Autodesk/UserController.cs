using Application.Common.Pagination;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Domain;
using Autodesk.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace Autodesk.Api.Controllers.api.v1.Autodesk
{
    /// <summary>
    /// Controller for managing User entities via REST API.
    /// </summary>
    [ApiController]
    [Route("api/v1/users")]
    public class UserController(
        IUserCreate userCreate,
        IUserRead readFilterCursor,
        IUserUpdate userUpdate,
        IUserDelete userDelete) : ControllerBase
    {
        // Dependency-injected use-case services
        private readonly IUserCreate _create = userCreate;
        private readonly IUserRead _read = readFilterCursor;
        private readonly IUserUpdate _update = userUpdate;
        private readonly IUserDelete _delete = userDelete;

        /// <summary>
        /// Create a new User.
        /// </summary>
        /// <param name="user">The User object from request body.</param>
        /// <returns>Created User or BadRequest on failure.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            // Invoke create use-case
            var op = await _create.CreateUserAsync(user);
            if (!op.IsSuccessful)
            {
                // Return 400 with error message if creation fails
                return BadRequest(op.Message);
            }

            // Return 201 Created with location header pointing to ReadFilterCursor
            return CreatedAtAction(nameof(ReadFilterCursor), new { id = user.Id }, user);
        }

        /// <summary>
        /// Retrieve a paged list of Users using cursor-based pagination.
        /// </summary>
        /// <param name="qp">Query parameters for paging and filtering.</param>
        /// <returns>PagedResult of User or BadRequest.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<User>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReadFilterCursor([FromQuery] UserQueryParams qp)
        {
            // Validate query parameters
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Invoke read use-case
            var op = await _read.GetUsersPageAsync(qp.Id, qp.Name, qp.Cursor, qp.PageSize);
            if (!op.IsSuccessful)
            {
                // Return 400 with error message if read fails
                return BadRequest(op.Message);
            }

            // Return 200 OK with paginated data
            return Ok(op.Data);
        }

        [HttpGet("reactive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IObservable<User> Read(int maxUsers, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (maxUsers <= 0)
            {
                // If maxUsers is zero or negative, immediately complete.
                return Observable.Empty<User>();
            }

            return _read.GetStreamUsers(maxUsers);
        }

        /// <summary>
        /// Update an existing User.
        /// </summary>
        /// <param name="id">The User ID in the URL.</param>
        /// <param name="user">The updated User object from request body.</param>
        /// <returns>Updated User or BadRequest.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(string id, [FromBody] User user)
        {
            // Ensure URL ID matches payload ID
            if (id != user.Id)
            {
                return BadRequest("ID in URL and payload must match.");
            }

            // Invoke update use-case
            var op = await _update.UpdateUserAsync(user);
            if (!op.IsSuccessful)
            {
                // Return 400 with error message if update fails
                return BadRequest(op.Message);
            }

            // Return 200 OK with the updated user
            return Ok(user);
        }

        /// <summary>
        /// Delete a User by ID.
        /// </summary>
        /// <param name="id">The User ID to delete.</param>
        /// <returns>NoContent if deleted, NotFound or BadRequest otherwise.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            // Invoke delete use-case
            var op = await _delete.DeleteUserAsync(id);
            if (!op.IsSuccessful)
            {
                // Return 400 for general failure
                return BadRequest(op.Message);
            }

            if (!op.Data)
            {
                // Return 404 if no entity was deleted (not found)
                return NotFound();
            }

            // Return 204 No Content for successful deletion
            return NoContent();
        }
    }
}