namespace WolverineCaseStudyElia.Contracts;

public record WriteToConsole : ICommand
{
    public required string Text { get; init; }
    public ConsoleColor? Color { get; init; }
}
