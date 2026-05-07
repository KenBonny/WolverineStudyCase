using Wolverine;

namespace WolverineCaseStudy.Host.Sagas;

public sealed record ConsoleWriteSideEffect(string Text) : ISideEffect
{
    public void Execute()
    {
        Console.WriteLine(Text);
    }
}


