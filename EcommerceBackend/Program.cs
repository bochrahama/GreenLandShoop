
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using EcommerceBackend.Data;
using EcommerceBackend.Entities;
using EcommerceBackend.Repositories;
using EcommerceBackend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

    

var builder = WebApplication.CreateBuilder(args);
// Add services to the container. 
builder.Services.AddControllers();
IServiceCollection serviceCollection = builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); 

builder.Services.AddIdentity<AppUser,IdentityRole>(options =>
{
    // Configure password requirements for Identity
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var Key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtSettings["Issuer"],
         ValidAudience = jwtSettings["Audience"],
         IssuerSigningKey = new SymmetricSecurityKey(Key)
     });
builder.Services.AddAuthorization();



builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter Bearer {your_token}",
        Type = SecuritySchemeType.Http,
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>)) ;
// Register the ProductServices and CategoryService with the DI container
builder.Services.AddScoped<IProductService, ProductServices>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddMemoryCache();
var app = builder.Build();
app.UseMiddleware<EcommerceBackend.Middleware.ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Add authentication and authorization middleware
// authentication is about confirming who you are, while authorization is about determining what you can do.
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
var roleManager = app.Services.CreateScope().ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
// Create roles if they don't exist
// the porpes is to ensure that the necessary roles are available in the system for user management and authorization purposes.
foreach (var roleName in new[] { "Admin", "User" })
{
    if (!await roleManager.RoleExistsAsync(roleName))
    {
        await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Categories.Any())
    {
        var electronics = new Category { Name = "Electronics" };
        var books = new Category { Name = "Books" };
        db.Categories.AddRange(electronics, books);
        db.SaveChanges();
        db.Products.AddRange(
        new Product { ProductName = "Wireless Mouse", ProductPrice = 19.99m, ProductQuantity = 50, ProductCategoryId = electronics.Id },
        new Product { ProductName = "Mechanical Keyboard", ProductPrice = 79.99m, ProductQuantity = 20, ProductCategoryId = electronics.Id },
        new Product { ProductName = "Clean Code", ProductPrice = 34.99m, ProductQuantity = 15, ProductCategoryId = books.Id }
        );
        db.SaveChanges();
    }
}

app.Run();