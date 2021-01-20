using Console.Controllers.Validators;
using Newtonsoft.Json.Serialization;
//using WebApi.Controllers.Validators;
using System.Net.Http.Formatting;
using System.Collections.Generic;
using Console.Models.Validators;
using Microsoft.Practices.Unity;
using FluentValidation.WebApi;
using FluentValidation;
using System.Web.Http;
using Console.Filters;
using Newtonsoft.Json;
//using WebApi.Models;
using System.Linq;
using Owin;
using app;
using Models;
using Application.Validators;

namespace WebApi
{
    public class Startup
	{
		private UnityContainer _container;

		public void Configuration(IAppBuilder appBuilder)
		{
			var config = new HttpConfiguration();
			_container = new UnityContainer();

			UnityResolver();

			var json = config.Formatters.JsonFormatter;
			var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();

			jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			json.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;

			SettingConfig(config);

			appBuilder.UseWebApi(config);
		}

		private void SettingConfig(HttpConfiguration config)
		{
			config.DependencyResolver = new UnityResolver(_container);
			config.Filters.Add(new RestfulModelStateFilterAttribute());
			//FluentValidationModelValidatorProvider.Configure(config, p=> p.ValidatorFactory = new UnityValidatorFactory(_container));
			config.MapHttpAttributeRoutes();
		}

		private void UnityResolver()
		{
			ConfigureApplicationServices();
			ConfigureValidators();
		}

		private void ConfigureApplicationServices()
		{
			_container.RegisterType<IApp, App>();
		}

		private void ConfigureValidators()
		{
			//_container.RegisterType(typeof(IValidator<>), typeof(EmployeeValidator));
			_container.RegisterType<IValidator<IEnumerable<Currencies>>, CurrenciesListValidator>();
			_container.RegisterType<IValidator<Currencies>, CurrenciesValidator>();
		}
		
	}
}