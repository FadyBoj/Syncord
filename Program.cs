using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Syncord.Data;
using Syncord.Models;
using Syncord.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Syncord.Hubs;
using Syncord.providers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CloudinaryDotNet;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure Identity DB context

builder.Services.AddDbContext<SyncordContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");

    if (connectionString != null)
        options.UseNpgsql(connectionString);

    options.ConfigureWarnings(warnings =>
    warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored,CoreEventId.NavigationBaseIncluded));
});


//user api endpoints
builder.Services.AddIdentityApiEndpoints<User>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
})
.AddDefaultTokenProviders()
.AddEntityFrameworkStores<SyncordContext>();

//Configure JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtIssuer,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))

    };
});


//Add scopped services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFriendShipRepository,FriendShipRepository>();
builder.Services.AddScoped<IChatRepository,ChatRepository>();

//Configure cloudinary 
var cloudinaryKey = builder.Configuration["Cloudinary:Key"];
var cloudinarySecret = builder.Configuration["Cloudinary:Secret"];
Cloudinary cloudinary = new Cloudinary($"cloudinary://{cloudinaryKey}:{cloudinarySecret}@ddivi7f83");
builder.Services.AddSingleton(cloudinary);

//Configure real time 
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, EmailBasedUserIdProvider>();

var app = builder.Build();

//Configure db context


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<MsgHub>("/chat");

app.Run();
