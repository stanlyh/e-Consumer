using PersonDataApp.Application.Ports;
using PersonDataApp.Application.UseCases;
using PersonDataApp.Infrastructure.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
    options.AddPolicy("FrontendDev", policy =>
        policy.WithOrigins("http://localhost:4321")
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IGetPersonUseCase, GetPersonUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("FrontendDev");
app.UseAuthorization();
app.MapControllers();

app.Run();
