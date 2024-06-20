namespace CommandLineParser.Parsing.Models;

internal record ParsingError(ParsingErrorType Type, string? TokenName = null, string? TokenValue = null);