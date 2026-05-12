using Grpc.Core;
using RobotGrpc;

namespace RobotServer.Services;

public class RobotServiceImpl : RobotService.RobotServiceBase
{
    
    private const int MinX = 0;
    private const int MaxX = 9;
    private const int MinY = 0;
    private const int MaxY = 9;

  
    private int _x = 5;        
    private int _y = 5;        
    private string _direction = "stop";  
    private int _speed = 1;    

    private Timer? _timer;     

    
    public override async Task Control(
        IAsyncStreamReader<ClientCommand> requestStream,    
        IServerStreamWriter<ServerStatus> responseStream,  
        ServerCallContext context)
    {
        
        _timer = new Timer(async _ => await MoveRobot(responseStream),
            null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

        try
        {
            
            await foreach (var command in requestStream.ReadAllAsync())
            {
                
                switch (command.Command.ToLower())
                {
                    case "forward":
                        _direction = "forward";
                        break;
                    case "back":
                        _direction = "back";
                        break;
                    case "left":
                        _direction = "left";
                        break;
                    case "right":
                        _direction = "right";
                        break;
                    case "stop":
                        _direction = "stop";
                        break;
                    case "speed":
                        if (command.Speed >= 1 && command.Speed <= 5)
                            _speed = command.Speed;
                        break;
                }

                
                await responseStream.WriteAsync(new ServerStatus
                {
                    Status = "ok",
                    Message = $"Команда {command.Command} выполнена",
                    Position = GetPosition()
                });
            }
        }
        finally
        {
            await _timer.DisposeAsync();
        }
    }

    
    private async Task MoveRobot(IServerStreamWriter<ServerStatus> responseStream)
    {
        int newX = _x;
        int newY = _y;

        
        switch (_direction)
        {
            case "forward": newY += _speed; break;
            case "back": newY -= _speed; break;
            case "left": newX -= _speed; break;
            case "right": newX += _speed; break;
            case "stop": return;
        }

       
        if (newX < MinX || newX > MaxX || newY < MinY || newY > MaxY)
        {
            
            await responseStream.WriteAsync(new ServerStatus
            {
                Status = "error",
                Message = "СТОЛКНОВЕНИЕ! Робот врезался в стену",
                Position = GetPosition()
            });
        }
        else
        {
            
            _x = newX;
            _y = newY;
        }
    }

    
    private Position GetPosition()
    {
        return new Position { X = _x, Y = _y };
    }
}