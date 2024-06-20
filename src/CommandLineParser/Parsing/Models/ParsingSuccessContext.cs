using CommandLineParser.Models;

namespace CommandLineParser.Parsing.Models;

internal record ParsingSuccessContext(
    CommandDescriptor Command,
    IReadOnlyDictionary<IArgumentDescriptor, List<object>> ParsedPositionalArguments,
    IReadOnlyList<OptionDescriptor> ParsedFlags,
    IReadOnlyDictionary<OptionDescriptor, List<object>> ParsedOptions);