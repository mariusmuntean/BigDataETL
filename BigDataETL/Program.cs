using System.Text.Json.Serialization;
using BigDataETL.Data;
using BigDataETL.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.RegisterCustomServices();

builder.Services.AddDbContext<EtlDbContext>(optionsBuilder =>
{
    var dbConnectionString = builder.Configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>().DbConnectionString;
    optionsBuilder.UseNpgsql(dbConnectionString);
});

var app = builder.Build();

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