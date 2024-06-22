using CommandLineParser.Models;

namespace CommandLineParser.Parsing.Models;

// NOTE: should this type be in this namespace?
internal record ParsingResult(
    CommandDescriptor Command,
    IReadOnlyDictionary<IArgumentDescriptor, List<object>> ParsedPositionalArguments,
    IReadOnlyList<OptionDescriptor> ParsedFlags,
    IReadOnlyDictionary<OptionDescriptor, List<object>> ParsedOptions,
    IReadOnlyList<ParsingError> Errors)
{
    public bool IsSuccess => Errors.Count == 0;
}