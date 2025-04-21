using Play.Common.Extensions;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var services = builder.Services;


builder.Services.AddMongo()
                .AddRepositories<InventoryItem>("inventory-items")
                .AddRepositories<CatalogItem>("catalog-items")
                .AddMassTransitWithRabbitMQ();

AddCatalogClientSynchronous(builder, services);



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.UseHttpsRedirection();

app.Run();

static void AddCatalogClientSynchronous(WebApplicationBuilder builder, IServiceCollection services)
{
    Random jitterer = new Random();
    builder.Services.AddHttpClient<CatalogClient>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:5001");
    })
    // implementing exponential retry timing
    .AddTransientHttpErrorPolicy(builder =>
        builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
            5,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                            + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
            onRetry: (outcome, timespan, retryAttempt) =>
            {
                var serviceProvider = services.BuildServiceProvider();
                serviceProvider.GetRequiredService<ILogger<CatalogClient>>()?
                    .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
            }))

    // implementing circuit breaker
    .AddTransientHttpErrorPolicy(builder =>
        builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
            3,
            TimeSpan.FromSeconds(15),
            onBreak: (outcome, timespan) =>
            {
                var serviceProvider = services.BuildServiceProvider();
                serviceProvider.GetRequiredService<ILogger<CatalogClient>>()?
                    .LogWarning($"Opening the circuit for {timespan} seconds");
            },
            onReset: () =>
            {
                var serviceProvider = services.BuildServiceProvider();
                serviceProvider.GetRequiredService<ILogger<CatalogClient>>()?
                    .LogWarning($"Closing the circuit");
            }
        )
    )

    // implementing timeout request
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
}