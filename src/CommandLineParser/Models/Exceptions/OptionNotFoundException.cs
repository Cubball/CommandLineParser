namespace CommandLineParser.Models.Exceptions;

public class OptionNotFoundException : Exception
{
    public OptionNotFoundException()
        : base("The specified option was not found. Make sure you try to get an option that is available in this command.")
    { }
}