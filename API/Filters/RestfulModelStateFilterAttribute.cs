using System.Web.Http.ModelBinding;
using System.Web.Http.Controllers;
using System.Collections.Generic;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Linq;
using System.Net;
using System;

namespace Console.Filters
{
	public class RestfulModelStateFilterAttribute : ActionFilterAttribute
	{
        private readonly Func<Dictionary<string, object>, bool> _validate;

        public RestfulModelStateFilterAttribute()
            : this(a => a.ContainsValue(null))
        {
        }

        public RestfulModelStateFilterAttribute(Func<Dictionary<string, object>, bool> checkCondition)
        {
            _validate = checkCondition;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
		{
            if (_validate(actionContext.ActionArguments))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, new ApiResourceValidationErrorWrapper("The argument cannot be null.", actionContext.ModelState));
            }

            if (!actionContext.ModelState.IsValid)
			{
				actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, new ApiResourceValidationErrorWrapper(actionContext.ModelState));
			}
		}
	}

    public class ApiResourceValidationErrorWrapper
    {
        private const string ErrorMessage = "The request is invalid.";

        private const string MissingPropertyError = "Undefined error.";

        public ApiResourceValidationErrorWrapper(ModelStateDictionary modelState)
        {
            Message = ErrorMessage;
            SerializeModelState(modelState);
        }

        public ApiResourceValidationErrorWrapper(string message, ModelStateDictionary modelState)
        {
            Message = message;
            SerializeModelState(modelState);
        }

        public string Message { get; private set; }

        public IDictionary<string, IEnumerable<string>> Errors { get; private set; }

        private void SerializeModelState(ModelStateDictionary modelState)
        {
            Errors = new Dictionary<string, IEnumerable<string>>();

            foreach (var keyModelStatePair in modelState)
            {
                var key = keyModelStatePair.Key;

                var errors = keyModelStatePair.Value.Errors;

                if (errors != null && errors.Count > 0)
                {
                    IEnumerable<string> errorMessages = errors.Select(
                            error => string.IsNullOrEmpty(error.ErrorMessage)
                                                     ? MissingPropertyError
                                                     : error.ErrorMessage).ToArray();

                    Errors.Add(key, errorMessages);
                }
            }
        }
    }
}
