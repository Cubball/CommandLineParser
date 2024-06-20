using CommandLineParser.Models;

namespace CommandLineParser.Parsing.Models;

internal record ParsingFailureContext(
    CommandDescriptor Command,
    IReadOnlyList<ParsingError> Errors);