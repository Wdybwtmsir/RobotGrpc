using Grpc.Core;
using Grpc.Net.Client;
using RobotGrpc;

namespace RobotClient;

class Program
{
    static async Task Main(string[] args)
    {
        
        var handler = new HttpClientHandler();

        
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        
        var channel = GrpcChannel.ForAddress("http://192.168.122.68:5000", new GrpcChannelOptions
        {
            HttpHandler = handler,
            MaxReceiveMessageSize = null, 
            MaxSendMessageSize = null    
        });

        var client = new RobotService.RobotServiceClient(channel);

        Console.WriteLine("=== УПРАВЛЕНИЕ РОБОТОМ ===");
        Console.WriteLine($"Подключаемся к серверу: http://192.168.122.68:5000");
        Console.WriteLine("====================================");

        try
        {
            
            using var call = client.Control();

            
            var readTask = Task.Run(async () => await ReadServerMessages(call));

            
            await Task.Delay(1000);

            
            await call.RequestStream.WriteAsync(new ClientCommand { Command = "speed", Speed = 2 });
            Console.WriteLine("✅ Автопилот запущен! Скорость = 2");
            Console.WriteLine("====================================");

            
            string[] commands = {
                "forward", "forward", "forward",
                "right",
                "forward", "forward", "forward",
                "right",
                "forward", "forward", "forward",
                "right",
                "forward", "forward", "forward",
                "right"
            };

            bool collisionDetected = false;

            for (int i = 0; i < commands.Length; i++)
            {
                
                if (collisionDetected)
                {
                    Console.WriteLine("\n⚠️ СТОЛКНОВЕНИЕ! Меняем маршрут...");
                    await call.RequestStream.WriteAsync(new ClientCommand { Command = "back" });
                    Console.WriteLine("  → Отправлено: back");
                    await Task.Delay(500);

                    await call.RequestStream.WriteAsync(new ClientCommand { Command = "right" });
                    Console.WriteLine("  → Отправлено: right");
                    await Task.Delay(500);

                    collisionDetected = false;
                    Console.WriteLine("  → Продолжаем движение по новому маршруту");
                    Console.WriteLine("====================================");
                }

                
                string cmd = commands[i];
                await call.RequestStream.WriteAsync(new ClientCommand { Command = cmd });
                Console.WriteLine($"→ Команда {i + 1}/{commands.Length}: {cmd}");

                await Task.Delay(1000);
            }

            Console.WriteLine("\n====================================");
            Console.WriteLine("✅ Автопилот завершил маршрут");
            Console.WriteLine("Нажмите Enter для выхода...");
            Console.ReadLine();

            readTask.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ ОШИБКА: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
            }
        }
    }

    static async Task ReadServerMessages(AsyncDuplexStreamingCall<ClientCommand, ServerStatus> call)
    {
        try
        {
            await foreach (var status in call.ResponseStream.ReadAllAsync())
            {
                if (status.Status == "error")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[СЕРВЕР] ❌ {status.Status.ToUpper()}: {status.Message}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n[СЕРВЕР] ✅ {status.Status}: {status.Message}");
                    Console.ResetColor();
                }

                Console.WriteLine($"[ПОЗИЦИЯ] 📍 X={status.Position.X}, Y={status.Position.Y}");
                Console.WriteLine("------------------------------------");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при чтении: {ex.Message}");
        }
    }
}