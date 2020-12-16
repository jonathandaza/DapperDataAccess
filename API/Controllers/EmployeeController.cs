using System.Collections.Generic;
using System.Web.Http;
using app;
using webapi.Models;

namespace webapi.Controllers
{
	[RoutePrefix("api/v1")]
    public class EmployeeController : ApiController
	{
		private readonly IApp _app;

		public EmployeeController(IApp app)
		{
			_app = app;
		}

		[HttpPost]
		[Route("employees")]
		public IHttpActionResult Post([FromBody] IEnumerable<Employee> employee)
		{
			if (ModelState.IsValid)
			{
				//Mapper.CreateMap<Employee, EmployeeDTO>();
				//var newEmployee = _app.Add(Mapper.Map<EmployeeDTO>(employee));
					
				return Ok();
			}

			return BadRequest();

		}
    }
}
