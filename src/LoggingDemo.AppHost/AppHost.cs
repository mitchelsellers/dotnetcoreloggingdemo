var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BlazorLoggingDemo>("blazorloggingdemo");

builder.Build().Run();
