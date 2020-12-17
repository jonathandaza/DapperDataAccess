using System.Web.Http;
using Models;
using app;

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
		public IHttpActionResult Post([FromBody] Currencies currency)
		{
			if (ModelState.IsValid)
			{
                var newCurrency = _app.Add(currency);					
				return Ok(newCurrency);
			}

			return BadRequest();

		}
    }
}
