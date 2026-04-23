namespace WolverineCaseStudyElia.Host.Handlers;

public record WriteToConsole : ICommand
{
    public required string Text { get; init; }
    public ConsoleColor? Color { get; init; }
}

public class WriteToConsoleHandler : IHandleMessages<WriteToConsole>
{
    public Task Handle(WriteToConsole message, IMessageHandlerContext context)
    {
        var originalColor = Console.ForegroundColor;

        if (message.Color.HasValue)
            Console.ForegroundColor = message.Color.Value;

        Console.WriteLine("[NSB] " + message.Text);

        Console.ForegroundColor = originalColor;

        return Task.CompletedTask;
    }
}
