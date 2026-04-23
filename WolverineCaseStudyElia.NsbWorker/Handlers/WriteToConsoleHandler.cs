using WolverineCaseStudyElia.Contracts;

namespace WolverineCaseStudyElia.NsbWorker.Handlers;

public class WriteToConsoleHandler : IHandleMessages<WriteToConsole>
{
    public async Task Handle(WriteToConsole message, IMessageHandlerContext context)
    {
        var originalColor = Console.ForegroundColor;

        if (message.Color.HasValue)
            Console.ForegroundColor = message.Color.Value;

        Console.WriteLine("[NSB] " + message.Text);

        Console.ForegroundColor = originalColor;
        
        await context.Publish(new WrittenToConsole(message.Text));
    }
}
