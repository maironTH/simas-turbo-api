using System.Data;
using Npgsql;
using SimasTurbo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("neondb")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IVeiculoInterface, VeiculoService>();
builder.Services.AddScoped<IClienteInterface, ClienteService>();
builder.Services.AddScoped<ILocacaoInterface, LocacaoService>();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

Dapper.SqlMapper.AddTypeHandler(new SimasTurbo.Config.DateOnlyTypeHandler());

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
