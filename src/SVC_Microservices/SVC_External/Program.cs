using SVC_External.DataCollectors;
using SVC_External.DataCollectors.Interfaces;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Register containers for dependency injection
        builder.Services.AddHttpClient("BinanceClient", client =>
        {
            client.BaseAddress = new Uri("https://api.binance.com");
        });
        builder.Services.AddHttpClient("BybitClient", client =>
        {
            client.BaseAddress = new Uri("https://api.bybit.com");
        });
        builder.Services.AddHttpClient("MexcClient", client =>
        {
            client.BaseAddress = new Uri("https://api.mexc.com");
        });
        builder.Services.AddScoped<IExchangesDataCollector, ExchangesDataCollector>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}