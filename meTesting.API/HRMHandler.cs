using MediatR;
using meTesting.Bus.SDK;
using meTesting.HRM.API.Event;
using meTesting.HRM.Services;
using meTesting.Sauron;
using Serilog.Context;
using System.Text.Json;

namespace meTesting.HRM.API;

public class HRMHandler(IServiceProvider services, ILogger<HRMHandler> logger) : IOnRecieveEvent
{
    static Func<Message, IBaseRequest?> Resolver = a =>
    {
        return a.Type switch
        {
            "LetterStateChangeEventArg" => ResolveForLetterStateChangeEventArg(a),

            _ => null
        };

        static LetterStateChangeArgs ResolveForLetterStateChangeEventArg(Message msg)
        {
            var data = JsonSerializer.Deserialize<LetterStateChangeArgs>(msg.Body, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });

            return new LetterStateChangeArgs()
            {
                NewState = data.NewState,
                TransactionId = data.TransactionId,
            };
        }
    };
    public async Task Do(Message message)
    {
        Console.WriteLine(JsonSerializer.Serialize(message));
        if (Resolver(message) is { } a)
        {
            logger.LogInformation("hrm get a new event from bus");

            using var scop = services.CreateScope();
            IMediator mediator = scop.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(a);
            logger.LogInformation("hrm served the new event");
        }
    }
}
