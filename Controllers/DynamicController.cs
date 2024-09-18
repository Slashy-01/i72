using I72_Backend.Interfaces;
using I72_Backend.Model;
using I72_Backend.Repository;
using I72_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace I72_Backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DynamicController : ControllerBase
	{
		private readonly IDynamicRepository _dynamicRepository;

		public DynamicController(IDynamicRepository dynamicRepository)
		{
			_dynamicRepository = dynamicRepository;
		}

		[HttpGet]
		public ActionResult<IEnumerable<Dynamic>> GetDynamic()
		{
			var dynamic = _dynamicRepository.GetDynamic();
			return Ok(dynamic);
		}

		[HttpPost("Add")]
		public ActionResult Add([FromBody] Dynamic dynamic)
		{
			_dynamicRepository.AddDynamic(dynamic);

			return Ok(new { Message = "User added successfully" });
		}

		[HttpDelete("{id}")]
		public ActionResult Delete(int id)
		{
			var dynamic = _dynamicRepository.GetDynamicById(id);
			if (dynamic == null)
			{
				return NotFound($"Failed to delete the user with ID {id}. User not found.");
			}

			try
			{
				// Delete the user from the repository
				_dynamicRepository.DeleteDynamic(dynamic);
				return Ok($"User with ID {id} deleted successfully.");
			}
			catch (Exception ex)
			{
				// Handle any errors that occur during deletion
				return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to delete the user with ID {id}. Error: {ex.Message}");
			}
		}
	}
}