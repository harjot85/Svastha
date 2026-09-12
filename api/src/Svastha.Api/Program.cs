using System.Data;
using System.Reflection;
using Dapper;
using Microsoft.AspNetCore.Authorization;
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
    builder.Configuration["SVASTHA_DB"]
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

var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

// Liveness: process is up. Deliberately touches nothing external, so it stays
// green while the database is down.
app.MapGet("/health", () => Results.Ok(new { status = "ok", version }))
    .AllowAnonymous();

// Readiness: the process can actually serve traffic, which means reaching Postgres.
app.MapGet("/ready", async (NpgsqlDataSource dataSource) =>
{
    try
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await connection.ExecuteScalarAsync<int>("select 1");

        return Results.Ok(new { status = "ready" });
    }
    catch (Exception ex)
    {
        return Results.Json(
            new { status = "not ready", error = ex.Message },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
})
    .AllowAnonymous();

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
