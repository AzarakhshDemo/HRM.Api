using MediatR;
using meTesting.Bus.SDK;
using meTesting.Chart;
using meTesting.Personnel;
using meTesting.LetterSrv;
using meTesting.TransactionIdGenerator;
using meTesting.HRM.API;
using meTesting.HRM.AtrributeService;
using meTesting.HRM.Evaluation;
using meTesting.GeneralHelpers;
using meTesting.Aether.SDK;
using meTesting.Sauron;
using Microsoft.AspNetCore.Authentication.OAuth;
using meTesting.Sandbaad.Sdk;
using meTesting.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

//using var loggerFactory = LoggerFactory.Create(builder =>
//{
//    builder.AddConsole();
//});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigFor<SauronConfig>(builder.Configuration, out var conf);

builder.Services.ConfigFor<EnvironmentConfig>(builder.Configuration, out var envConf);

builder.Services.ConfigFor<SandbaadConfig>(builder.Configuration, out var sandConf);

builder.Services.AddSauron(builder.Configuration, conf!);

builder.Services.AddSandbaadClient(sandConf!);

builder.Services.AddLogging(a =>
a.AddConsole());

builder.Services.AddMediatR(a => a.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

builder.Services.AddPersonelService();
builder.Services.AddLetterService();
builder.Services.AddChartService();
builder.Services.AddPersonelAttrributeService();
builder.Services.AddEvaluationService();

builder.Services.AddServiceBus(a =>
{
    a.BaseUrl = envConf.Url; // "http://localhost:5106";
    a.Key = envConf.AppKey;  // "HRM_API";
}, a => new HRMHandler(a, a.GetRequiredService<ILogger<HRMHandler>>()));

builder.Services.AddAetherNotifFromDiscovery();

//    (a =>
//{
//    a.BaseUrl = "https://localhost:7156";
//});



builder.Services.AddSingleton<TrGen>();

builder.Services.AddScoped<Mock>();

var app = builder.Build();

using var scop = app.Services.CreateScope();

var m = scop.ServiceProvider.GetRequiredService<Mock>();
m.inflate();

app.UseSandbaad(scop.ServiceProvider, envConf.AppKey, envConf.Url);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSauron();

app.UseAuthorization();

app.MapControllers();

app.Run();

