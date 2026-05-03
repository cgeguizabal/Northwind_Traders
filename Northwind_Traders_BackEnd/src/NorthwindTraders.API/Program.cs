using System.Text;                                              // C# built in
using Microsoft.AspNetCore.Authentication.JwtBearer;           // JWT package
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;                          // JWT package
using NorthwindTraders.Domain.Interfaces;
using NorthwindTraders.Infrastructure.Persistence;
using NorthwindTraders.Infrastructure.Repositories;
using NorthwindTraders.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ── DATABASE ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ── REPOSITORIES ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>(); 
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>(); 
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>(); 

// ── SERVICES ──────────────────────────────────────────────────────────────────
// AddScoped — JwtService needs IConfiguration which is a singleton, scoped is fine here
builder.Services.AddHttpClient<GeocodingService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<DashboardService>();



// ── CORS ──────────────────────────────────────────────────────────────────────
// AddCors — C# built in ASP.NET Core Method
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Program.cs — reads origins from config
         var allowedOrigins = builder.Configuration
             .GetSection("AllowedOrigins")
             .Get<string[]>() ?? [];
                policy.WithOrigins(allowedOrigins)


            // AllowAnyHeader — allow any HTTP header (Authorization, Content-Type, etc.)
            .AllowAnyHeader()
            // AllowAnyMethod — allow GET, POST, PUT, DELETE, OPTIONS, etc.
            .AllowAnyMethod()
            // AllowCredentials — allow cookies and Authorization headers
            // Required so the browser sends the JWT token
            .AllowCredentials();
    });
});

// ── JWT AUTHENTICATION ────────────────────────────────────────────────────────
// AddAuthentication — C# built in ASP.NET Core Method
// Tells ASP.NET Core: "use JWT bearer tokens as the auth scheme"
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // TokenValidationParameters — JWT package — defines what to check on every token
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,   // check the Issuer claim matches
            ValidateAudience         = true,   // check the Audience claim matches
            ValidateLifetime         = true,   // reject expired tokens
            ValidateIssuerSigningKey = true,   // verify the signature

            ValidIssuer      = builder.Configuration["Jwt:Issuer"],
            ValidAudience    = builder.Configuration["Jwt:Audience"],

            // SymmetricSecurityKey — JWT package — recreates the key to verify signature
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// ── API BASICS ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to accept JWT tokens
// This adds the Authorize button in Swagger UI
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: eyJhbGci..."
    });

    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

// ── MIDDLEWARE PIPELINE ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend"); 
app.UseAuthentication();    // MUST come before UseAuthorization
app.UseAuthorization();
app.MapControllers();

// ── SEEDER ────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await EmployeeSeeder.SeedAsync(context);
}

app.Run();