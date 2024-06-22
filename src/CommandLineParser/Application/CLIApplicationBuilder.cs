using CommandLineParser.Models;
using CommandLineParser.Parsing.Models;

namespace CommandLineParser.Application;

internal class CLIApplicationBuilder
{
    private readonly List<Action<ParsingResult, Action<ParsingResult>>> _middlewares = [];
    private CommandDescriptor? _rootCommand;

    // TODO: replace with factory method? would be useful to create non-empty builders
    internal CLIApplicationBuilder() { }

    // TODO: replace with command builder
    public CLIApplicationBuilder HasRootCommand(CommandDescriptor command)
    {
        _rootCommand = command;
        return this;
    }

    public CLIApplicationBuilder AddMiddleware(Action<ParsingResult, Action<ParsingResult>> middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    public CLIApplication Build()
    {
        var pipeline = BuildMiddlewarePipiline();
        return new(_rootCommand!, pipeline);
    }

    // TODO: methods that throw when invalid operation

    private Action<ParsingResult> BuildMiddlewarePipiline()
    {
        var count = _middlewares.Count;
        var pipeline = (ParsingResult parsingResult) => FinalMiddleware(parsingResult, null!);
        for (int i = count - 1; i >= 0; i--)
        {
            // TODO: refactor? I dont like these closures
            var current = _middlewares[i];
            var next = pipeline;
            pipeline = (ParsingResult parsingResult) => current(parsingResult, next);
        }

        return pipeline;
    }

    // TODO: move somewhere?
    private static void FinalMiddleware(ParsingResult parsingResult, Action<ParsingResult> next)
    {
        var command = parsingResult.Command;
        var handler = command.Handler;
        var parsedCommand = new ParsedCommand(
            command,
            parsingResult.ParsedPositionalArguments,
            parsingResult.ParsedFlags,
            parsingResult.ParsedOptions);
        handler(parsedCommand);
    }
}