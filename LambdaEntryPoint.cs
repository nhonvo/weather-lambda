using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.AspNetCoreServer;
using Amazon.Lambda.Core;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

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

        private static readonly string SecretKey = "very_secret_key";

        /// <summary>
        /// Handles incoming API Gateway requests.
        /// </summary>
        /// <param name="request">The APIGatewayProxyRequest</param>
        /// <param name="context">The Lambda context</param>
        /// <returns>APIGatewayProxyResponse</returns>
        public override Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandlerAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogInformation("Processing request...");

            // Check if the Authorization header is present
            if (request?.Headers?.ContainsKey("Authorization") != true)
            {
                return Task.FromResult(new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401,
                    Body = JsonSerializer.Serialize(new { message = "Unauthorized: Missing Authorization Header" }),
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    }
                });
            }

            // Extract the token from the Authorization header
            var token = request.Headers["Authorization"].Split(" ").Last();

            // Validate the JWT Token
            // var validationResult = ValidateJwtToken(token);
            // if (!validationResult.IsValid)
            if (token != SecretKey)
            {
                return Task.FromResult(new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401,
                    Body = JsonSerializer.Serialize(new { message = "Unauthorized: Invalid or expired token" }),
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

        // Method to validate JWT token
        //     private static (bool IsValid, string UserName) ValidateJwtToken(string token)
        //     {
        //         try
        //         {
        //             var tokenHandler = new JwtSecurityTokenHandler();
        //             var key = System.Text.Encoding.ASCII.GetBytes(SecretKey);
        //             var validationParameters = new TokenValidationParameters
        //             {
        //                 ValidateIssuer = false,
        //                 ValidateAudience = false,
        //                 ValidateLifetime = true,
        //                 IssuerSigningKey = new SymmetricSecurityKey(key),
        //                 ClockSkew = TimeSpan.Zero // Optional: Control the allowed time skew
        //             };

        //             var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
        //             var userName = principal.FindFirst(ClaimTypes.Name)?.Value;
        //             return (true, userName);
        //         }
        //         catch (Exception)
        //         {
        //             return (false, null);
        //         }
        //     }
    }

    public class WeatherForecast
    {
        public DateOnly Date { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
