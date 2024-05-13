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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure Identity DB context

builder.Services.AddDbContext<IdentityContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");

    if (connectionString != null)
        options.UseMySQL(connectionString);
});

//user api endpoints
builder.Services.AddIdentityApiEndpoints<User>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
})
.AddDefaultTokenProviders()
.AddEntityFrameworkStores<IdentityContext>();

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


//Configure real time 
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider,EmailBasedUserIdProvider>();

var app = builder.Build();

//Configure db context


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<MsgHub>("/chat");

app.Run();
