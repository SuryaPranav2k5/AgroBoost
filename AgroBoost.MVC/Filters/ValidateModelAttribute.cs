using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgroBoost.MVC.Filters;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e => new
                    {
                        Field = e.Key,
                        Errors = e.Value!.Errors.Select(err => err.ErrorMessage).ToArray()
                    })
                    .ToList();

                context.Result = new BadRequestObjectResult(new
                {
                    error = "Validation failed",
                    details = errors
                });
            }
            else
            {
                // For MVC, let the default validation handle it
                // The view will show validation errors
            }
        }

        base.OnActionExecuting(context);
    }
}