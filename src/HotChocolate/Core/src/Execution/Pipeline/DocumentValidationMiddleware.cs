using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Validation;
using static HotChocolate.WellKnownContextData;
using static HotChocolate.Execution.ErrorHelper;

namespace HotChocolate.Execution.Pipeline;

internal sealed class DocumentValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IExecutionDiagnosticEvents _diagnosticEvents;
    private readonly IDocumentValidator _documentValidator;

    public DocumentValidationMiddleware(
        RequestDelegate next,
        IExecutionDiagnosticEvents diagnosticEvents,
        IDocumentValidator documentValidator)
    {
        _next = next ??
            throw new ArgumentNullException(nameof(next));
        _diagnosticEvents = diagnosticEvents ??
            throw new ArgumentNullException(nameof(diagnosticEvents));
        _documentValidator = documentValidator ??
            throw new ArgumentNullException(nameof(documentValidator));
    }

    public async ValueTask InvokeAsync(IRequestContext context)
    {
        if (context.Document is null || context.DocumentId is null)
        {
            context.Result = StateInvalidForDocumentValidation();
        }
        else
        {
            if (context.ValidationResult is null || _documentValidator.HasDynamicRules)
            {
                using (_diagnosticEvents.ValidateDocument(context))
                {
                    context.ValidationResult =
                        await _documentValidator
                            .ValidateAsync(
                                context.Schema,
                                context.Document,
                                context.DocumentId,
                                context.ContextData,
                                context.ValidationResult is not null,
                                context.RequestAborted)
                            .ConfigureAwait(false);

                    if (!context.IsValidDocument)
                    {
                        // if the validation failed we will report errors within the validation
                        // span and we will complete the pipeline since we do not have a valid
                        // GraphQL request.
                        var validationResult = context.ValidationResult;

                        // create result context data that indicate that validation has failed.
                        var resultContextData = new Dictionary<string, object?>
                        {
                            { ValidationErrors, true }
                        };

                        // if one of the validation rules proposed a status code we will add
                        // it as a proposed status code to the result context data.
                        // depending on the transport this code might not be relevant or
                        // is even overruled.
                        if (context.ContextData.TryGetValue(HttpStatusCode, out var value))
                        {
                            resultContextData.Add(HttpStatusCode, value);
                        }

                        context.Result = QueryResultBuilder.CreateError(
                            validationResult.Errors,
                            resultContextData);

                        _diagnosticEvents.ValidationErrors(context, validationResult.Errors);
                        return;
                    }
                }
            }

            await _next(context).ConfigureAwait(false);
        }
    }
}
