namespace CommandLineParser.Models.Exceptions;

public class ParsedOptionNotFoundException : Exception
{
    public ParsedOptionNotFoundException() : base("The specified option was not parsed.") { }
}