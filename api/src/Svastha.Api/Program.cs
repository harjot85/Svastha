using System.Data;
using Dapper;
using Npgsql;
using Svastha.Api;

DefaultTypeMap.MatchNamesWithUnderscores = true;

// Npgsql surfaces timestamptz as DateTime, so Dapper cannot bind it to a
// DateTimeOffset constructor parameter without an explicit handler.
SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString =
    Environment.GetEnvironmentVariable("SVASTHA_DB")
    ?? "Host=localhost;Port=5432;Database=svastha;Username=svastha;Password=svastha";

builder.Services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());

builder.Services.AddCors(options =>
{
    options.AddPolicy("web", policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("web");

app.MapPost("/api/ping", async (CreatePingRequest request, NpgsqlDataSource dataSource) =>
{
    if (!PingValidation.IsValid(request))
    {
        return Results.BadRequest(new { error = "Message is required." });
    }

    await using var connection = await dataSource.OpenConnectionAsync();

    var ping = await connection.QuerySingleAsync<Ping>(
        """
        insert into ping (message) values (@Message)
        returning id, message, created_at
        """,
        new { request.Message });

    return Results.Created($"/api/ping/{ping.Id}", ping);
});

app.MapGet("/api/ping", async (NpgsqlDataSource dataSource) =>
{
    await using var connection = await dataSource.OpenConnectionAsync();

    var pings = await connection.QueryAsync<Ping>(
        """
        select id, message, created_at from ping
        order by created_at desc limit 50
        """);

    return Results.Ok(pings.ToArray());
});

app.Run();

sealed class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override DateTimeOffset Parse(object value) => value switch
    {
        DateTimeOffset offset => offset,
        DateTime utc => new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)),
        _ => throw new InvalidCastException($"Cannot convert {value?.GetType().Name} to DateTimeOffset.")
    };

    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value) =>
        parameter.Value = value.UtcDateTime;
}
