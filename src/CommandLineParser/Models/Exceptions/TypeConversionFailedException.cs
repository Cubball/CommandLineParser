namespace CommandLineParser.Models.Exceptions;

public class TypeConversionFailedException : Exception
{
    public TypeConversionFailedException()
        : base("Failed to convert a value to specified type. Did you use the correct type?")
    { }
}