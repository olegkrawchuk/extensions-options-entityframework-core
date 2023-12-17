using EFCoreConfigurationSample.Extensions;
using EFCoreConfigurationSample.Options;
using EFCoreConfigurationSample.Persistense;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPersistense(builder.Configuration);
builder.Configuration.AddEntityFrameworkCoreConfiguration(builder.Services);

builder.Services.ConfigureAndValidate<IdentificationOptions>(IdentificationOptions.ConfigName);
builder.Services.ConfigureAndValidate<SigningOptions>(SigningOptions.ConfigName);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().MigrateAsync();
}

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
