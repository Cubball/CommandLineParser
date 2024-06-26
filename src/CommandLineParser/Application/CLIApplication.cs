using CommandLineParser.Application.Builders;
using CommandLineParser.Models;
using CommandLineParser.Parsing;
using CommandLineParser.Parsing.Models;

namespace CommandLineParser.Application;

internal class CLIApplication
{
    private readonly Parser _parser;
    private readonly ParsingResultBuilder _parsingResultBuilder;
    private readonly Action<ParsingResult> _pipeline;

    public CLIApplication(
        CommandDescriptor rootCommand,
        Action<ParsingResult> pipeline)
    {
        _parsingResultBuilder = new();
        _parser = new(_parsingResultBuilder, rootCommand);
        _pipeline = pipeline;
    }

    public void Run(string[] args)
    {
        _parser.Parse(args);
        // NOTE: I do not like it that much
        var result = _parsingResultBuilder.Build();
        _pipeline(result);
    }

    public static ICLIApplicationRootCommandBuilder CreateEmptyBuilder()
    {
        return CLIApplicationBuilder.CreateEmpty();
    }
}