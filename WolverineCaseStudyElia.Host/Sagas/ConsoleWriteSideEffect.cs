using Wolverine;

namespace WolverineCaseStudyElia.Host.Sagas;

public sealed record ConsoleWriteSideEffect(string Text) : ISideEffect
{
    public void Execute()
    {
        Console.WriteLine(Text);
    }
}


