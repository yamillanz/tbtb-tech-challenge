using Microsoft.EntityFrameworkCore;
using TbtbChallenge.Api.Data;
using TbtbChallenge.Api.Handlers;
using TbtbChallenge.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<TbtbChallengeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TbtbDatabase")));

builder.Services.AddScoped<ContactService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<ContactAmendmentService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();
