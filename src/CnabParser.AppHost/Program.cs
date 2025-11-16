var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder
    .AddSqlServer("sql", port: 11433);

var database = sqlServer
    .AddDatabase("cnabdb");

var api = builder
    .AddProject<Projects.CnabParser_Api>("api")
    .WithExternalHttpEndpoints()
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.CnabParser_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);


builder.Build().Run();
