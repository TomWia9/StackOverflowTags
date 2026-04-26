using Api.Endpoints;
using Application;
using Application.Tags.Commands;
using Infrastructure;
using Infrastructure.Persistence;
using MediatR;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Run EF migrations and ensure tags loaded
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();

    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    await mediator.Send(new EnsureTagsLoadedCommand());
}

app.UseCors();

app.MapOpenApi();

// Scalar UI (/scalar)
app.MapScalarApiReference(opt =>
{
    opt.Title = "Stack Overflow Tags API";
    opt.Theme = ScalarTheme.DeepSpace;
});

app.MapTagsEndpoints();

app.Run();

// Expose for integration tests
public partial class Program;