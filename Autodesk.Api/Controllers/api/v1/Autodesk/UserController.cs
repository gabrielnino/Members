using Application.Common.Pagination;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Domain;
using Autodesk.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Autodesk.Api.Controllers.api.v1.Autodesk
{
    [ApiController]
    [Route("api/v1/users")]
    public class UserController(
        IUserCreate userCreate,
        IUserReadFilterCursor readFilterCursor,
        IUserUpdate userUpdate,
        IUserDelete userDelete) : ControllerBase
    {
        private readonly IUserCreate _create = userCreate;
        private readonly IUserReadFilterCursor _readFilterCursor = readFilterCursor;
        private readonly IUserUpdate _update = userUpdate;
        private readonly IUserDelete _delete = userDelete;


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

            return CreatedAtAction(nameof(ReadFilterCursor), new { id = user.Id }, user);
        }

        [HttpGet("cursor")]
        [ProducesResponseType(typeof(PagedResult<User>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReadFilterCursor([FromQuery] UserQueryParams qp)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
                
            var op = await readFilterCursor.ReadFilterCursor(qp.Id, qp.Name, qp.Cursor, qp.PageSize);
            if (!op.IsSuccessful) return BadRequest(op.Message);
            return Ok(op.Data);
        }


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