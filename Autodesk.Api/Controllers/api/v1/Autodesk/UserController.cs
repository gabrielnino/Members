using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Autodesk.Api.Controllers.api.v1.Autodesk
{
    [ApiController]
    [Route("api/v1/users")]
    public class UserController(
        IUserCreate userCreate,
        IUserReadFilter userReadFilter,
        IUserReadFilterCount userReadFilterCount,
        IUserReadFilterPage userReadFilterPage,
        IUserReadById userReadById,
        IUserUpdate userUpdate,
        IUserDelete userDelete) : ControllerBase
    {
        private readonly IUserCreate _create = userCreate;
        private readonly IUserReadFilter _readFilter = userReadFilter;
        private readonly IUserReadFilterCount _readCount = userReadFilterCount;
        private readonly IUserReadFilterPage _readPage = userReadFilterPage;
        private readonly IUserReadById _readById = userReadById;
        private readonly IUserUpdate _update = userUpdate;
        private readonly IUserDelete _delete = userDelete;

        /// <summary>
        /// Create a new user.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            var op = await _create.Create(user);
            if (!op.IsSuccessful)
            {
                return BadRequest(op.Message);
            }

            return CreatedAtAction(nameof(ReadById), new { id = user.Id }, user);
        }

        /// <summary>
        /// Retrieve users whose names contain the given filter.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReadFilter([FromQuery] string? name)
        {
            name ??= string.Empty;
            var op = await _readFilter.ReadFilter(u => u.Name != null && u.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            if (!op.IsSuccessful)
            {
                return BadRequest(op.Message);
            }

            return Ok(op.Data.ToArray() ?? []);
        }

        /// <summary>
        /// Retrieve a single user by its identifier.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReadById(string id)
        {
            var op = await _readById.ReadById(id);
            if (!op.IsSuccessful)
            {
                return BadRequest(op.Message);
            }

            if (op.Data == null)
            {
                return NotFound();
            }


            return Ok(op.Data);
        }

        /// <summary>
        /// Update an existing user.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(string id, [FromBody] User user)
        {
            if (id != user.Id)
            {
                return BadRequest("ID in URL and payload must match.");
            }

            var op = await _update.Update(user);
            if (!op.IsSuccessful)
            {
                return BadRequest(op.Message);
            }

            return Ok(user);
        }

        /// <summary>
        /// Delete a user by its identifier.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            var op = await _delete.Delete(id);
            if (!op.IsSuccessful)
            {
                return BadRequest(op.Message);
            }

            if (!op.Data)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}