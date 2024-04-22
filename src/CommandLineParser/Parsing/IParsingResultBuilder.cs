using CommandLineParser.Parsing.Models;

namespace CommandLineParser.Parsing;

internal interface IParsingResultBuilder
{
    void AddError(ParsingError error);

    void AddParsedValue(string key, object value);

    void AddFlag(string flagFullName);
}