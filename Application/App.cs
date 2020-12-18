using Infraestructure.Api.DapperDataAccess;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace app
{
    public class App : IApp
    {
        DPGenericRepository<Currencies> _repositoryCurrency = new DPGenericRepository<Currencies>("Main");

        public Currencies Add(Currencies currency)
	    {
            var id = _repositoryCurrency.Create(currency);
            return currency;
        }

        public IEnumerable<Currencies> Get()
        {
            return _repositoryCurrency.GetAll();
        }

        public Currencies Get(int id)
        {
            Filter[] fiter = { new Filter { Field = "curId", Operator = "=", Value = id.ToString(), HasQuotes = false } }; 
            var currencies = _repositoryCurrency.GetBy(fiter);

            return currencies.FirstOrDefault();
        }

        public Currencies Update(Currencies currencies)
        {
            Filter[] fiter = { new Filter { Field = "curId", Operator = "=", Value = currencies.Id.ToString(), HasQuotes = false } };

            var ids = _repositoryCurrency.Update(fiter, currencies);
            return currencies;
        }
    }
}
