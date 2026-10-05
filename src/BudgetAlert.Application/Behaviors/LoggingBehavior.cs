using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BudgetAlert.Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            TResponse response;
            logger.LogInformation("[MediatR] Handling {RequestName}", request.GetType().Name);
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                response = await next(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[MediatR] Exception handling {RequestName}", request.GetType().Name);
                throw;
            }
            stopwatch.Stop();

            logger.LogInformation("[MediatR] Handled {RequestName} in {ElapsedMs}ms", request.GetType().Name, stopwatch.ElapsedMilliseconds);

            return response;
        }
    }
}