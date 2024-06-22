using CommandLineParser.Models;

namespace CommandLineParser.Parsing.Models;

internal record ParsingResult(
    CommandDescriptor Command,
    IReadOnlyDictionary<IArgumentDescriptor, List<object>> ParsedPositionalArguments,
    IReadOnlyList<OptionDescriptor> ParsedFlags,
    IReadOnlyDictionary<OptionDescriptor, List<object>> ParsedOptions,
    IReadOnlyList<ParsingError> Errors)
{
    public bool IsSuccess => Errors.Count == 0;
}