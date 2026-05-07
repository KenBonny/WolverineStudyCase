namespace WolverineCaseStudy.Contracts;

public record WriteToConsole : ICommand
{
    public required string Text { get; init; }
    public ConsoleColor? Color { get; init; }
}

public record WrittenToConsole(string Text) : IEvent;