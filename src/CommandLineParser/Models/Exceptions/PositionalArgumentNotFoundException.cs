namespace CommandLineParser.Models.Exceptions;

public class PositionalArgumentNotFoundException : Exception
{
    public PositionalArgumentNotFoundException() : base("The specified positional argument was not found.") { }
}