using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AracKiralamaPortali.API.Filters
{
    /// <summary>
    /// SchemaFilter to ignore complex navigation properties that cause circular references in Swagger
    /// </summary>
    public class IgnoreVirtualPropertiesSchemaFilter : ISchemaFilter
    {
        private static readonly Dictionary<string, string[]> NavigationPropertiesToIgnore = new()
        {
            { "AppUser", new[] { "Reservations", "Reviews", "Vehicles", "Claims", "Logins", "Tokens", "UserRoles" } },
            { "IdentityUser", new[] { "Claims", "Logins", "Tokens", "UserRoles" } },
            { "Vehicle", new[] { "Brand", "Owner", "Reservations", "Maintenances", "Reviews", "Images" } },
            { "Reservation", new[] { "Vehicle", "User", "AdditionalServices" } },
            { "Maintenance", new[] { "Vehicle" } },
            { "Review", new[] { "Vehicle", "User" } },
            { "VehicleImage", new[] { "Vehicle" } },
            { "Payment", new[] { "Reservation" } },
            { "Brand", new[] { "Vehicles" } },
            { "AdditionalService", new[] { "Reservations" } }
        };

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null)
                return;

            var typeName = context.Type.Name;

            if (NavigationPropertiesToIgnore.TryGetValue(typeName, out var propsToRemove))
            {
                foreach (var prop in propsToRemove)
                {
                    var keyToRemove = schema.Properties.Keys.FirstOrDefault(k => string.Equals(k, prop, StringComparison.OrdinalIgnoreCase));
                    if (keyToRemove != null)
                    {
                        schema.Properties.Remove(keyToRemove);
                    }
                }
            }
        }
    }
}
