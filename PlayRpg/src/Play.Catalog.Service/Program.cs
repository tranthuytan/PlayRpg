using System.Reflection;
using MassTransit;
using Play.Catalog.Service.Entities;
using Play.Common.Extensions;
using Play.Common.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// builder.Services.Configure<ServiceSettings>(builder.Configuration.GetSection(nameof(ServiceSettings)));
// builder.Services.AddTransient(sp => sp.GetRequiredService<IOptions<ServiceSettings>>().Value);

// builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection(nameof(RabbitMQSettings)));
// builder.Services.AddTransient(sp => sp.GetRequiredService<IOptions<RabbitMQSettings>>().Value);

builder.Services.AddMongo()
                .AddRepositories<Item>("items")
                .AddMassTransitWithRabbitMQ();

// builder.Services.AddMassTransitHostedService();

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

app.UseHttpsRedirection();

app.MapControllers();
app.Run();
