using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace ThreeL.Blob.Shared.Application.Contract.Interceptors.Attributes
{
    public class ParamValidateAttribute : ActionFilterAttribute
    {
        private readonly Type _type;
        private readonly int _index;
        public ParamValidateAttribute(Type validatorType, int parameterIndex = 0)
        {
            _type = validatorType;
            _index = parameterIndex;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var service = context.HttpContext.RequestServices.GetRequiredService(_type) as IValidator;
            if (service != null)
            {
                try
                {
                    var parameter = context.ActionArguments.Skip(_index).FirstOrDefault();
                    var validationContext = new ValidationContext<object>(parameter.Value);
                    ValidationResult result;
                    if (!(result = service.Validate(validationContext)).IsValid)
                    {
                        context.Result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(result.Errors.First().ErrorMessage);
                    }
                }
                catch { }
            }

            base.OnActionExecuting(context);
        }
    }
}
