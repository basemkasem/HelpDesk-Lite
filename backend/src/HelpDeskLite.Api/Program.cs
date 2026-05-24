using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using HelpDeskLite.Api.Extensions;
using HelpDeskLite.Api.Middleware;
using HelpDeskLite.Application;
using HelpDeskLite.Infrastructure;
using HelpDeskLite.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    var maxBytes = builder.Configuration.GetValue<long>("FileStorage:MaxFileSizeBytes", 5 * 1024 * 1024);
    options.MultipartBodyLengthLimit = maxBytes * 6;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHelpDeskJwtAuthentication(builder.Configuration);
builder.Services.AddHelpDeskAuthorization();
builder.Services.AddHelpDeskSwagger();
builder.Services.AddHelpDeskApiVersioning();

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    if (app.Configuration.GetValue("SeedData:Enabled", app.Environment.IsDevelopment()))
    {
        await seeder.SeedAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
