using RobotServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();


builder.WebHost.ConfigureKestrel(options =>
{
    
    options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

var app = builder.Build();


app.UseHttpsRedirection(); 

app.MapGrpcService<RobotServiceImpl>();


app.MapGet("/", () => "gRPC Robot Server работает на HTTP/2!");

app.Run();