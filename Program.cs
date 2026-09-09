using System.Data;
using Npgsql;
using SimasTurbo.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("neondb")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IVeiculoInterface, VeiculoService>();
builder.Services.AddScoped<IClienteInterface, ClienteService>();
builder.Services.AddScoped<ILocacaoInterface, LocacaoService>();
builder.Services.AddScoped<IAuthInterface, AuthService>();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

Dapper.SqlMapper.AddTypeHandler(new SimasTurbo.Config.DateOnlyTypeHandler());

var jwtSecret = builder.Configuration.GetSection("JwtConfig").GetValue<string>("Secret");
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException(
        "A configuração 'JwtConfig:Secret' é obrigatória. Execute 'dotnet user-secrets set \"JwtConfig:Secret\" \"sua-chave-secreta\"' ou defina JwtConfig__Secret no ambiente.");
}

var key = Encoding.ASCII.GetBytes(jwtSecret);
var jwtSecurityKey = new SymmetricSecurityKey(key);
builder.Services.AddSingleton(jwtSecurityKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = jwtSecurityKey,
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole o seu token JWT aqui"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
