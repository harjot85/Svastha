using DbUp;
using Microsoft.Extensions.Configuration;

// Environment variables are added last so they override user secrets, keeping
// Fly, Render, and docker-compose working unchanged.
var configuration = new ConfigurationBuilder()
    .AddUserSecrets(typeof(Program).Assembly, optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString =
    configuration["SVASTHA_DB"]
    ?? "Host=localhost;Port=5432;Database=svastha;Username=svastha;Password=svastha";

EnsureDatabase.For.PostgresqlDatabase(connectionString);

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(typeof(Program).Assembly)
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.WriteLine(result.Error);
    return 1;
}

Console.WriteLine("Migrations complete.");
return 0;
