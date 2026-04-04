using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace AracKiralamaPortali.API.Filters
{
    /// <summary>
    /// OperationFilter to handle IFormFile parameters for file uploads in Swagger
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if any parameter is IFormFile
            var formFileParams = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType.IsAssignableFrom(typeof(IFormFile)))
                .ToList();

            if (formFileParams.Count == 0)
                return;

            // Get all [FromForm] parameters
            var formParameters = context.MethodInfo.GetParameters()
                .Where(p => p.GetCustomAttribute<FromFormAttribute>() != null)
                .ToList();

            // Remove existing parameters that were added incorrectly by Swashbuckle
            operation.Parameters.Clear();

            // Build new request body with multipart/form-data
            var properties = new Dictionary<string, OpenApiSchema>();
            var required = new HashSet<string>();

            foreach (var parameter in formParameters)
            {
                if (string.IsNullOrWhiteSpace(parameter.Name))
                {
                    continue;
                }

                var parameterName = parameter.Name;

                if (parameter.ParameterType == typeof(IFormFile) || parameter.ParameterType.IsAssignableFrom(typeof(IFormFile)))
                {
                    properties[parameterName] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary",
                        Description = "File to upload"
                    };
                }
                else
                {
                    // For non-file form parameters, generate schema
                    try
                    {
                        var schema = context.SchemaGenerator.GenerateSchema(
                            parameter.ParameterType,
                            context.SchemaRepository);
                        properties[parameterName] = schema;
                    }
                    catch
                    {
                        // Fallback for complex types
                        properties[parameterName] = new OpenApiSchema { Type = "string" };
                    }
                }

                // Mark as required if not optional
                if (!parameter.IsOptional)
                {
                    required.Add(parameterName);
                }
            }

            // Set the request body
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = required.Count > 0,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties,
                            Required = required
                        }
                    }
                }
            };
        }
    }
}
