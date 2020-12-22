using System.Collections.Generic;
using Console.Filters;
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
        [RestfulModelStateFilter] 
        public IHttpActionResult Post([FromBody] Currencies currency)
		{
            var response = _app.Add(currency);
            if (response.WasSuccessful())
            {
                return Ok(currency);
            }

			return BadRequest(response.SerializeMessage);
		}

        [HttpPost]
        [Route("currencies")]
        [RestfulModelStateFilter]
        public IHttpActionResult Post([FromBody] IEnumerable<Currencies> currency)
        {
            var response = _app.Add(currency);
            if (response.WasSuccessful())
            {
                return Ok(currency);
            }

            return BadRequest(response.SerializeMessage);
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
