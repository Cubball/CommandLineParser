namespace CommandLineParser.Parsing.Models;

internal enum ParsingErrorType
{
    UnknownOption,
    ArgumentValueForFlag,
    UnknownToken,
    MissingArgumentValue,
    ConversionFailed,
    MissingRequiredOption,
    MissingPositionalArgument,
}