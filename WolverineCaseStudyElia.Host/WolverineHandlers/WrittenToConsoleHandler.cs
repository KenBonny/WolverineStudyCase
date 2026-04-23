using WolverineCaseStudyElia.Contracts;

namespace WolverineCaseStudyElia.Host.WolverineHandlers;

public class WrittenToConsoleHandler
{
    public void Handle(WrittenToConsole message, ILogger<WrittenToConsoleHandler> logger)
    {
        logger.LogInformation("Received the message from NServiceBus: {Text}", message.Text);
    }
}
