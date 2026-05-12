using RobotServer.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddGrpc();


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); 
});

var app = builder.Build();

app.MapGrpcService<RobotServiceImpl>();
app.MapGet("/", () => "gRPC Robot Server работает!");

app.Run();