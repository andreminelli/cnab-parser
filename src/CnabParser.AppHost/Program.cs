var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder
    .AddSqlServer("sql", port: 11433);

var database = sqlServer
    .AddDatabase("cnabdb");

builder
    .AddProject<Projects.CnabParser_Api>("api")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
