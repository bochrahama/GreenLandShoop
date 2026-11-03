
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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {// Configure JSON serialization options to handle enum values as strings
        //it use for serializing and deserializing enum values as their string representations in JSON, rather than their underlying numeric values.
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
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
        //it use for serializing and deserializing enum values as their string
        //representations in JSON, rather than their underlying numeric values.
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>)) ;
// Register the ProductServices and CategoryService with the DI container
builder.Services.AddScoped<IProductService, ProductServices>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddMemoryCache();
//FrontendPolicy is a constant string that defines the name of the CORS policy being added to the application.
const string FrontendPolicy = "AllowFrontEnd";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendPolicy, policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();
app.UseMiddleware<EcommerceBackend.Middleware.ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Enable CORS for the specified frontend origins
app.UseCors(FrontendPolicy);
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
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // [CONFIRM: DbContext name]
    {
        Category EnsureCategory(string name, string description)
        {
            var existing = db.Categories.FirstOrDefault(c => c.Name == name);
            if (existing is not null) return existing;
            var created = new Category { Name = name, Description = description };
            db.Categories.Add(created);
            db.SaveChanges();
            return created;
        }
        // Names must match the CATEGORY_NAMES map in js/products.js (case-insensitive).
        var fashion = EnsureCategory("Fashion & Style", "Clothing, accessories, and everyday essentials.");
        var skincare = EnsureCategory("Skincare", "Natural routines for healthy, glowing skin.");
        var home = EnsureCategory("Home Decor", "Pieces to make your space feel calm and inviting.");
        void SeedProducts(Category cat, params (string name, string desc, decimal price, int stock, string image)[]
        items)
        {
            if (db.Products.Any(p => p.ProductCategoryId == cat.Id)) return; // do not duplicate
            foreach (var i in items)
            {
                var product = new Product
                {
                    ProductName = i.name,
                    ProductDescription = i.desc,
                    ProductPrice = i.price,
                    ProductQuantity = i.stock,
                    ProductCategoryId = cat.Id
                };
                db.Products.Add(product);
            }
            db.SaveChanges();
        }
        SeedProducts(fashion,
        ("Organic Cotton Hoodie", "Fleece-lined hoodie in certified organic cotton.", 89.00m, 25,
        "images/product-1.jpg"),
    ("Linen Wrap Dress", "Breathable linen dress with a relaxed wrap fit.", 129.00m, 12, "images/product-2.jpg"),
        ("Canvas Tote Bag", "Sturdy everyday tote with a natural finish.", 39.00m, 40, "images/product-3.jpg"),
        ("Merino Wool Scarf", "Soft, warm and made to last.", 59.00m, 3, "images/product-4.jpg"));
        SeedProducts(skincare,
        ("Glowfit Gel 100ml", "Light hydrating gel for daily use.", 189.00m, 30, "images/product-2.jpg"),
        ("Botanical Renewal Serum", "Squalane, wild nettle and rosemary extract.", 74.00m, 18,
        "images/product-1.jpg"),
    ("Rosemary Face Oil", "Cold-pressed botanical oil blend.", 48.00m, 22, "images/product-3.jpg"),
        ("Nettle Clay Mask", "Weekly purifying clay mask.", 32.00m, 0, "images/product-4.jpg"));
        SeedProducts(home,
        ("Velvet Lounge Sofa", "Velvet upholstery with solid oak legs.", 899.00m, 5, "images/product-1.jpg"),
        ("Oak Floor Lamp", "Warm light and a solid oak base.", 149.00m, 14, "images/product-2.jpg"),
        ("Handwoven Jute Rug", "Natural jute, handwoven by artisans.", 219.00m, 9, "images/product-3.jpg"),
        ("Ceramic Vase", "Hand-glazed stoneware vase.", 45.00m, 31, "images/product-4.jpg"));
    }

    app.Run();
}