using CommandLineParser.Models;
using CommandLineParser.Parsing.Models;

namespace CommandLineParser.Parsing;

internal interface IParsingResultBuilder
{
    void SetCurrentCommand(CommandDescriptor command);

    void AddParsedOptionValue(OptionDescriptor option, object value);

    void AddParsedArgumentValue(IArgumentDescriptor argument, object value);

    void AddFlag(OptionDescriptor option);

    void AddError(ParsingError error);
}