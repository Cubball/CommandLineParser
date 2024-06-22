using CommandLineParser.Models;

namespace CommandLineParser.Parsing.Models;

internal record ParsingFailureContext(
    CommandDescriptor Command,
    // NOTE: add parsed stuff here as well?
    IReadOnlyList<ParsingError> Errors);