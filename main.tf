provider "aws" {
  region = "us-east-1" # Replace with your preferred region
}

# Create IAM Role for Lambda
resource "aws_iam_role" "lambda_role" {
  name               = "lambda_execution_role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect    = "Allow"
        Principal = { Service = "lambda.amazonaws.com" }
        Action    = "sts:AssumeRole"
      }
    ]
  })
}

# Attach Policies to IAM Role
resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  role       = aws_iam_role.lambda_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

# Upload Lambda Function
resource "aws_lambda_function" "my_lambda_function" {
  function_name    = "MyDotNetLambda"
  filename         = "weather1.zip" # Ensure this file exists in your working directory
  handler          = "Weather::Weather.LambdaEntryPoint::FunctionHandlerAsync"
  runtime          = "dotnet8"
  role             = aws_iam_role.lambda_role.arn
  source_code_hash = filebase64sha256("weather.zip") # Detect changes in code for deployment

  environment {
    variables = {
      ENVIRONMENT = "production" # Example of environment variables
    }
  }
}

# (Optional) Create API Gateway to Invoke Lambda
resource "aws_apigatewayv2_api" "http_api" {
  name          = "MyDotNetLambdaAPI"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_integration" "lambda_integration" {
  api_id           = aws_apigatewayv2_api.http_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = aws_lambda_function.my_lambda_function.arn
}

resource "aws_apigatewayv2_route" "lambda_route" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "GET /weatherforecast"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

resource "aws_apigatewayv2_stage" "default_stage" {
  api_id      = aws_apigatewayv2_api.http_api.id
  name        = "$default"
  auto_deploy = true
}

# Allow API Gateway to Invoke Lambda
resource "aws_lambda_permission" "allow_api_gateway" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.my_lambda_function.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.http_api.execution_arn}/*/*"
}
