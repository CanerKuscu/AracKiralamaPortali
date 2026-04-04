using System.Globalization;
using System.Text;
using System.Globalization;
using AracKiralamaPortali.API.Data;
using AracKiralamaPortali.API.Models;
using AracKiralamaPortali.API.Repositories;
using AracKiralamaPortali.API.Localization;
using AracKiralamaPortali.API.Services;
using AracKiralamaPortali.API.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddErrorDescriber<CustomIdentityErrorDescriber>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPasswordHasherService>(sp => new PasswordHasherService(workFactor: 12));

builder.Services.AddLocalization();
builder.Services.AddControllers(options =>
{
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Bu alan zorunludur.");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(fieldName => $"'{fieldName}' alaný zorunludur.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => "Bir anahtar veya deðer eksik.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, fieldName) => $"'{fieldName}' alaný için '{value}' geçerli deðil.");
    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(fieldName => $"'{fieldName}' alaný için deðer geçersiz.");
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(value => $"'{value}' deðeri geçersiz.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(fieldName => $"'{fieldName}' alaný sayýsal bir deðer olmalýdýr.");
    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"'{value}' deðeri geçerli deðil.");
    options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Girilen deðer geçerli deðil.");
    options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(() => "Bu alan sayýsal bir deðer olmalýdýr.");
});

// Swagger yapýlandýrmasý
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Araç Kiralama Portal API",
        Version = "v1",
        Description = "Araç kiralama sistemi için RESTful API"
    });

    // JWT için Authorization header desteði
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Circular references çöz
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    
    // IFormFile parameters için OperationFilter ekle
    options.OperationFilter<FileUploadOperationFilter>();
    
    // Identity User ve derived types'larýn navigation properties'lerini ignore et
    options.SchemaFilter<IgnoreVirtualPropertiesSchemaFilter>();

    // XML dokümantasyonu ekle (isteðe baðlý)
    try
    {
        var xmlFile = Path.Combine(AppContext.BaseDirectory, "AracKiralamaPortali.API.xml");
        if (File.Exists(xmlFile))
        {
            options.IncludeXmlComments(xmlFile);
        }
    }
    catch { }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

var supportedCultures = new[] { new CultureInfo("tr-TR") };
app.UseRequestLocalization(new Microsoft.AspNetCore.Builder.RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Geliþtirme ortamýnda detaylý hata göster
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    try
    {
        await context.Database.MigrateAsync();
    }
    catch (System.InvalidOperationException ex) when (ex.Message.Contains("pending changes"))
    {
        // Ignore pending model changes warning
    }

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();

    string[] roles = { "Admin", "Employee", "User", "CarOwner" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            FullName = "Admin User",
            UserName = "admin",
            Email = "admin@arackiralama.com",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// Swagger UI'ý hem Development hem de Production'da aktif et
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Araç Kiralama API v1");
    options.RoutePrefix = string.Empty; // Swagger UI'ý root URL'de aç (https://localhost:xxxx/)
    options.DefaultModelsExpandDepth(1);
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

