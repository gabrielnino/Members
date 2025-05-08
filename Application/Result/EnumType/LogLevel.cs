namespace Application.Result.EnumType
{
    using Application.Result.EnumType.Extensions;

    public enum LogLevel
    {
        [EnumMetadata("Trace", "Used for the most detailed log outputs, including fine-grained information about the application's state.")]
        Trace,
        [EnumMetadata("Debug", "Used for interactive investigation during development, providing insights into the application behavior.")]
        Debug,
        [EnumMetadata("Information", "Used to track the general flow of the application, providing key insights and operational data.")]
        Information,
        [EnumMetadata("Warning", "Used for logs that highlight the abnormal or unexpected events in the application flow, which may need attention.")]
        Warning,
        [EnumMetadata("Error", "Used for logs that highlight when the current flow of execution is stopped due to a failure or significant issue.")]
        Error,
        [EnumMetadata("Fatal", "Used to log unhandled exceptions or critical errors that cause the program to crash or terminate.")]
        Fatal
    }
}
