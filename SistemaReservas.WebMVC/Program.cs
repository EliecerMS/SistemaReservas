using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SistemaReservas.WebMVC.Services;
using SistemaReservas.WebMVC.Services.Handlers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. Configure HttpContextAccessor for Cookie Proxy pattern
builder.Services.AddHttpContextAccessor();

// 2. Register Custom Handlers
builder.Services.AddTransient<TokenRefreshHandler>();

// Register base API url from config or hardcode
var baseApiUrl = builder.Configuration["ApiUrl"] ?? "https://localhost:7148/api/"; // Adjust this based on api url

// 3. Register a special client for Refreshing without the interceptor to avoid infinite loop
builder.Services.AddHttpClient("AuthRefreshClient", client =>
{
    client.BaseAddress = new Uri(baseApiUrl);
});

// 4. Register Typed Clients with the TokenRefreshHandler
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri(baseApiUrl);
})
.AddHttpMessageHandler<TokenRefreshHandler>();

// Register other services
builder.Services.AddHttpClient<IZoneService, ZoneService>(client =>
{
    client.BaseAddress = new Uri(baseApiUrl);
})
.AddHttpMessageHandler<TokenRefreshHandler>();

builder.Services.AddHttpClient<IReservationService, ReservationService>(client =>
{
    client.BaseAddress = new Uri(baseApiUrl);
})
.AddHttpMessageHandler<TokenRefreshHandler>();

// 5. Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["auth_token"];
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Usually API returns 401, but MVC should redirect to Login
            context.HandleResponse();
            context.Response.Redirect("/Auth/Login");
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// MapStaticAssets is strictly enforced for .NET 9 performance optimization
app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
