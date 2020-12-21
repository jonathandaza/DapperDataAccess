using Infraestructure.Api.DapperDataAccess;
using System.Collections.Generic;
using FluentValidation;
using Models.Output;
using System.Linq;
using Models;

namespace app
{
    public class App : IApp
    {
        readonly DPGenericRepository<Currencies> _repositoryCurrency = new DPGenericRepository<Currencies>("Main");

        readonly IValidator<Currencies> _validatorCurrencies;

        public App(IValidator<Currencies> validatorCurrencies)
        {
            _validatorCurrencies = validatorCurrencies;
        }

        public ResponseMessage Add(Currencies currency)
	    {
            ResponseMessage responseMessage = new ResponseMessage();
            Currencies currencyResult = null;

            var validation = _validatorCurrencies.Validate(currency);
            if (!validation.IsValid)
            {
                responseMessage.Messages.Add(string.Join("|", validation.Errors));
                responseMessage.TypeEnum = ResponseMessage.Types.Error;
                return responseMessage;
            }

            _repositoryCurrency.Create(currency, out int idCurrency);
            if (idCurrency > 0)
            {
                currencyResult = Get(idCurrency);
            }

            return responseMessage;
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

        public ResponseMessage Update(Currencies currencyModel)
        {
            ResponseMessage responseMessage = new ResponseMessage();

            var currency = Get(currencyModel.Id);
            if (currency == null)
            {
                responseMessage.Messages.Add($"Currency code {currencyModel.Code} does not exist");
                responseMessage.TypeEnum = ResponseMessage.Types.Error;
                return responseMessage;
            }

            Filter[] fiter = { new Filter { Field = "curId", Operator = "=", Value = currency.Id.ToString(), HasQuotes = false } };

            var ids = _repositoryCurrency.Update(fiter, currency);

            return responseMessage;
        }
    }
}
