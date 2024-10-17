using BinanceTradingBot.Services;
using BinanceTradingBot.Interfaces;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Регистрация сервисов
builder.Services.AddSingleton<ITradingBotService, TradingBotService>();
builder.Services.AddSingleton<ITradePredictionModel, SharedTradePredictionModel>();
builder.Services.AddSingleton<ITradeExecutionService, TradeExecutionService>();

// Добавление контроллеров
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Trading Bot API",
        Version = "v1"
    });
});

var app = builder.Build();

// Конфигурация HTTP-запросов
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trading Bot API v1");
});
app.MapControllers();

app.Run();