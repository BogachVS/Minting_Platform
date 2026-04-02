using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace PiggyGame.Common.WebExtensions;

public static class MvcBuilderExtensions
{
    public static IMvcBuilder SetCustomValidationErrorsFormat(this IMvcBuilder mvcBuilder)
    {
        return mvcBuilder.ConfigureApiBehaviorOptions(opts =>
        {
            opts.InvalidModelStateResponseFactory = (errorContext) =>
            {
                var errorsMessages = errorContext.ModelState.Values.SelectMany(e => e.Errors.Select(m => m.ErrorMessage));
                var result = new
                {
                    Errors = errorsMessages.ToList()
                };

                return new BadRequestObjectResult(result);
            };
        });
    }
    
    public static IMvcBuilder AddEnumSerialization(this IMvcBuilder mvcBuilder)
    {
        return mvcBuilder.AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }
}