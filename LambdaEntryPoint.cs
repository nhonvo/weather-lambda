using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.AspNetCoreServer;
using Amazon.Lambda.Core;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Weather
{
    public class LambdaEntryPoint : APIGatewayHttpApiV2ProxyFunction
    {
        protected override void Init(IWebHostBuilder builder)
        {
            builder.UseStartup<Startup>();
        }
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        /// <summary>
        /// Handles incoming API Gateway requests.
        /// </summary>
        /// <param name="request">The APIGatewayProxyRequest</param>
        /// <param name="context">The Lambda context</param>
        /// <returns>APIGatewayProxyResponse</returns>
        public override Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandlerAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogInformation("Processing request...");

            // Check if the request or request context is null
            if (request?.RequestContext?.Http == null)
            {
                return Task.FromResult(new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonSerializer.Serialize(new { message = "Bad Request: Invalid Request Context" }),
                    Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }
                });
            }

            // Routing logic based on the request path and method
            if (request.RequestContext.Http.Path == "/weatherforecast" && request.RequestContext.Http.Method == "GET")
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                    })
                    .ToArray();

                return Task.FromResult(new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonSerializer.Serialize(forecast),
                    Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }
                });
            }

            // Default 404 response for unrecognized paths
            return Task.FromResult(new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 404,
                Body = JsonSerializer.Serialize(new { message = "Not Found" }),
                Headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        }
            });
        }
    }
    /// <summary>
    /// Represents a weather forecast.
    /// </summary>
    public class WeatherForecast
    {
        public DateOnly Date { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
