using System.Web.Http;
using Models;
using app;

namespace WebApi.Controllers
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
		[Route("currencies")]
		public IHttpActionResult Post([FromBody] Currencies currency)
		{
			if (ModelState.IsValid)
			{
                var newCurrency = _app.Add(currency);					
				return Ok(newCurrency);
			}

			return BadRequest();
		}

        [HttpPut]
        [Route("currencies")]
        public IHttpActionResult Put([FromBody] Currencies currency)
        {
            if (ModelState.IsValid)
            {
                var newCurrency = _app.Update(currency);
                return Ok(newCurrency);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("currencies")]
        public IHttpActionResult Get()
        {
            if (ModelState.IsValid)
            {
                var newCurrency = _app.Get();
                return Ok(newCurrency);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("currencies/{id}")]
        public IHttpActionResult Get(int id)
        {
            if (ModelState.IsValid)
            {
                var newCurrency = _app.Get(id);
                return Ok(newCurrency);
            }

            return BadRequest();
        }
    }
}
