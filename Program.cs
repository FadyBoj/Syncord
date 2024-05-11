using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Syncord.Data;
using Syncord.Models;
using Syncord.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure Identity DB context

builder.Services.AddDbContext<IdentityContext>(options =>{
    var connectionString = builder.Configuration.GetConnectionString("Default");

    if(connectionString != null)
    options.UseMySQL(connectionString);
});

//user api endpoints
builder.Services.AddIdentityApiEndpoints<User>()
.AddDefaultTokenProviders()
.AddEntityFrameworkStores<IdentityContext>();

//Add scopped services
builder.Services.AddScoped<IUserRepository,UserRepository>();

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

app.Run();
