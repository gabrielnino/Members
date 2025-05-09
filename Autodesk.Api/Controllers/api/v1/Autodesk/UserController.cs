using Application.Result;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Autodesk.Api.Controllers.api.v1.Autodesk
{
    [Route("api/v1/auth/")]
    [ApiController]
    public class UserController(IUserCreate userCreate, IUserReadById userReadById) : ControllerBase
    {
        [HttpPost("Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<User>> Create([FromBody] User user)
        {
            var result = await userCreate.Create(user);
            if (!result.IsSuccessful)
            {
                return BadRequest(result.Message);
            }
            return CreatedAtAction(nameof(ReadById), new { id = user.Id }, user);
        }

        [HttpGet("{id}", Name = "ReadById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> ReadById(string id)
        {
            var result = await userReadById.ReadById(id);
            if (!result.IsSuccessful)
            {
                return BadRequest(result.Error);
            }
            return result.Data == null ? NotFound() : result.Data;
        }


    }
}
