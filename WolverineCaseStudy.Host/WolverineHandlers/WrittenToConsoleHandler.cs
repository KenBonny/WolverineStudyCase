using WolverineCaseStudy.Contracts;

namespace WolverineCaseStudy.Host.WolverineHandlers;

public class WrittenToConsoleHandler(ILogger<WrittenToConsoleHandler> _logger)
{
    public void Handle(WrittenToConsole message)
    {
        _logger.LogInformation("Received the message from NServiceBus: {Text}", message.Text);
    }
}
