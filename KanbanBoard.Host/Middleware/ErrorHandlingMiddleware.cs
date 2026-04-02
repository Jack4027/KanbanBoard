namespace KanbanBoard.Host.Middleware
{

    // Middleware responsible for handling exceptions globally in the application,
    // it catches specific exceptions such as KeyNotFoundException, UnauthorizedAccessException and InvalidOperationException
    // and returns appropriate error responses to the client with the corresponding status codes (404 Not Found, 403 Forbidden and 400 Bad Request respectively).
    public class ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        //Error handling middleware that catches exceptions thrown during request processing and returns appropriate HTTP responses based on the exception type.
        //Exceptions will bubble up through the controller and routing layers until they are caught here, allowing for centralized error handling and consistent error responses across the application.
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                //Pass on the request to the next middleware in the pipeline, allowing it to be processed further down the line. If any exceptions are thrown during this process, they will be caught and handled by the catch blocks below.
                await next(context);
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                context.Response.StatusCode = StatusCodes.Status404NotFound;

                //Specific error message is returned in development environment to aid debugging, while a generic message is returned in production to avoid exposing sensitive information about the application's internals.
                var message = environment.IsDevelopment()
                    ? ex.Message
                    : "Resource not found.";

                await context.Response.WriteAsJsonAsync(new { message });
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning(ex.Message);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                var message = environment.IsDevelopment()
                    ? ex.Message
                    : "You do not have permission to perform this action.";

                await context.Response.WriteAsJsonAsync(new { message });
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                var message = environment.IsDevelopment()
                    ? ex.Message
                    : "The request could not be completed.";

                await context.Response.WriteAsJsonAsync(new { message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var message = environment.IsDevelopment()
                    ? ex.Message
                    : "An unexpected error occurred. Please try again later.";

                await context.Response.WriteAsJsonAsync(new { message });
            }
        }
    }
}
