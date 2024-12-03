# Setting Up an AWS Lambda Project with Authentication in .NET 8

## 1. **Required Libraries**

Install these libraries to set up the project with authentication and handle API Gateway requests:

- **AWS Lambda Core**: Required for creating AWS Lambda functions.

  ```bash
  dotnet add package Amazon.Lambda.Core
  ```

- **AWS Lambda ASP.NET Core Server**: Provides ASP.NET Core hosting for Lambda.

  ```bash
  dotnet add package Amazon.Lambda.AspNetCoreServer
  ```

- **JWT Bearer Authentication**: For validating JWT tokens.

  ```bash
  dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
  ```

- **System.Text.Json**: For JSON serialization/deserialization.

  ```bash
  dotnet add package System.Text.Json
  ```

- **Amazon.Lambda.Serialization.SystemTextJson**: For using `System.Text.Json` with Lambda.

  ```bash
  dotnet add package Amazon.Lambda.Serialization.SystemTextJson
  ```

------

## 2. **Project Structure**

- **`Startup.cs`**: Configure authentication, add JWT Bearer authentication, and define any required middleware.
- **`LambdaEntryPoint.cs`**: Entry point for the Lambda function.
- **`Program.cs`**: Default ASP.NET Core configuration.
- **`appsettings.json`**: Store JWT secrets and settings.

------

## 3. **Steps to Build the Project**

1. **Configure Authentication**:
   In `Startup.cs`:

2. **Lambda Handler**:
   Update `LambdaEntryPoint.cs` with your routing and authentication logic.

3. Update in csproj file

`<AssemblyName>Weather</AssemblyName>`

------

## 4. **Build and Deploy**

### Build and Publish the Project

1. **Publish the Project**:

    ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Create the Deployment Package**: Navigate to the `publish` directory and compress the files into a `.zip` file:

   ```bash
   cd ./publish
   zip -r lambda-package.zip *
   ```

------

### Deploy to AWS Lambda

1. **Deploy via AWS CLI**:

   - **Create a Lambda Function**:

```bash
    aws lambda create-function \
         --function-name WeatherLambda \
         --runtime dotnet8 \
         --role arn:aws:iam::account-id:role/your-lambda-role \
         --handler Weather::Weather.LambdaEntryPoint::FunctionHandlerAsync \
         --zip-file fileb://lambda-package.zip \
         --timeout 30 \
         --memory-size 256
```

- **Update the Function** (for subsequent deployments):

```bash
    aws lambda update-function-code \
         --function-name WeatherLambda \
         --zip-file fileb://lambda-package.zip
```

2. **Test the Function**: Use the event provided in earlier responses, simulating an API Gateway request with the `Authorization` header.

------

## 5. **Configuration for AWS API Gateway**

- **Create a REST API** in API Gateway and map it to your Lambda function.
- Set Authorization:
- Use the `AWS_IAM` or custom authorizers if needed.(Example Use Case: Lambda Accessing an S3 Bucket)

------

## 6. **Test Locally (Optional)**

Use the **AWS Lambda Mock Test Tool** for .NET:

1. Install the tool:

    ```bash
   dotnet tool install -g Amazon.Lambda.TestTool-8.0
   ```

2. Run the tool:

    ```bash
   dotnet lambda-test-tool-8.0
   ```

- Use this event in `weatherevent.json`
- Zip publish `zip -r ../publish.zip *`
- `dotnet publish -c Release -o ./publish`

3. Use the JSON event provided above for testing.

change handler in runtime

enhance 

setup apigateway, run schedule, step function, dynamodb

