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

        readonly IValidator<Currencies> _currenciesValidator;
        readonly IValidator<IEnumerable<Currencies>> _currenciesListValidator;

        public App(IValidator<Currencies> currenciesValidator,
                   IValidator<IEnumerable<Currencies>> currenciesListValidator)
        {
            _currenciesValidator = currenciesValidator;
            _currenciesListValidator = currenciesListValidator;
        }

        public ResponseMessage Add(Currencies currency)
	    {
            ResponseMessage responseMessage = new ResponseMessage();
            Currencies currencyResult = null;

            var validation = _currenciesValidator.Validate(currency);
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

        public ResponseMessage Add(IEnumerable<Currencies> currencies)
        {
            ResponseMessage responseMessage = new ResponseMessage();
            Currencies currencyResult = null;

            var validation = _currenciesListValidator.Validate(currencies);
            if (!validation.IsValid)
            {
                responseMessage.Messages.Add(string.Join("|", validation.Errors));
                responseMessage.TypeEnum = ResponseMessage.Types.Error;
                return responseMessage;
            }

            foreach (var currency in currencies)
            {
                _repositoryCurrency.Create(currency, out int idCurrency);
                if (idCurrency > 0)
                {
                    currencyResult = Get(idCurrency);
                }
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
