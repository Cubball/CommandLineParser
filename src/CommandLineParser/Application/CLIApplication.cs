using CommandLineParser.Models;
using CommandLineParser.Parsing;
using CommandLineParser.Parsing.Models;

namespace CommandLineParser.Application;

internal class CLIApplication
{
    private readonly Parser _parser;
    private readonly ParsingResultBuilder _parsingResultBuilder;
    private readonly Action<ParsingSuccessContext> _successPipeline;
    private readonly Action<ParsingFailureContext> _failurePipeline;

    public CLIApplication(
        CommandDescriptor rootCommand,
        Action<ParsingSuccessContext> successPipeline,
        Action<ParsingFailureContext> failurePipeline)
    {
        _parsingResultBuilder = new();
        _parser = new(_parsingResultBuilder, rootCommand);
        _successPipeline = successPipeline;
        _failurePipeline = failurePipeline;
    }

    public void Run(string[] args)
    {
        _parser.Parse(args);
        // NOTE: I do not like it that much
        var result = _parsingResultBuilder.Build();
        if (result.IsSuccess)
        {
            _successPipeline(result.SuccessContext);
        }
        else
        {
            _failurePipeline(result.FailureContext);
        }
    }
}