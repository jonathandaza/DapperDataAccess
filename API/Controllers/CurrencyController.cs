using System.Web.Http;
using System.Net;
using Models;
using app;

namespace WebApi.Controllers
{
	[RoutePrefix("api/v1")]
    public class CurrencyController : ApiController
	{
		private readonly IApp _app;

		public CurrencyController(IApp app)
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
            var currency = _app.Get();
            if (currency == null)
                return StatusCode(HttpStatusCode.NoContent);

            return Ok(currency);
        }

        [HttpGet]
        [Route("currencies/{id}")]
        public IHttpActionResult Get(int id)
        {
            var currency = _app.Get(id);
            if (currency == null)
                return StatusCode(HttpStatusCode.NoContent);

            return Ok(currency);
        }
    }
}
