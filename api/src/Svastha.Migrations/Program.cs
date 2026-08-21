using DbUp;

var connectionString =
    Environment.GetEnvironmentVariable("SVASTHA_DB")
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
