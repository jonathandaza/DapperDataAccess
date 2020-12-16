using Models;
using Infraestructure.Api.DapperDataAccess;
using System.Collections.Generic;

namespace app
{
    public class App : IApp
    {
        DPGenericRepository<Currencies> _repositoryCurrency = new DPGenericRepository<Currencies>("Main");

        public Currencies Add(Currencies currency)
	    {
            _repositoryCurrency.Create(currency);
            return currency;
        }

        public IEnumerable<Currencies> Get()
        {
            return _repositoryCurrency.GetAll();
        }
    }
}
