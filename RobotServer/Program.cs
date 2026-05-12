using RobotServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();


app.MapGrpcService<RobotServiceImpl>();

app.MapGet("/", () => "gRPC Robot Server работает!");

app.Run();