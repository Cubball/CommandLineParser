using System.Diagnostics.CodeAnalysis;
using CommandLineParser.Models;
using CommandLineParser.Parsing.Models;

namespace CommandLineParser.Application.Builders;

internal interface ICLIApplicationRootCommandBuilder
{
    ICLIApplicationMiddlewareBuilder HasRootCommand(Action<ICommandNameBuilder> configure);
}

internal interface ICLIApplicationMiddlewareBuilder
{
    ICLIApplicationMiddlewareBuilder UseMiddleware(Action<ParsingResult, Action<ParsingResult>> middleware);

    CLIApplication Build();
}

// TODO: do not restrict the methods by interfaces for this builder?
internal class CLIApplicationBuilder : ICLIApplicationRootCommandBuilder, ICLIApplicationMiddlewareBuilder
{
    private readonly List<Action<ParsingResult, Action<ParsingResult>>> _middlewares = [];
    private Action<ICommandNameBuilder>? _configureRootCommand;

    private CLIApplicationBuilder() { }

    public ICLIApplicationMiddlewareBuilder HasRootCommand(Action<ICommandNameBuilder> configure)
    {
        _configureRootCommand = configure;
        return this;
    }

    public ICLIApplicationMiddlewareBuilder UseMiddleware(Action<ParsingResult, Action<ParsingResult>> middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    public CLIApplication Build()
    {
        ThrowIfRootCommandBuilderIsNull();
        var builder = new CommandBuilder();
        _configureRootCommand(builder);
        var rootCommand = builder.Build();
        var pipeline = BuildMiddlewarePipeline();
        return new(rootCommand, pipeline);
    }

    public static ICLIApplicationRootCommandBuilder CreateEmpty()
    {
        return new CLIApplicationBuilder();
    }

    private Action<ParsingResult> BuildMiddlewarePipeline()
    {
        var count = _middlewares.Count;
        var pipeline = FinalMiddleware;
        for (int i = count - 1; i >= 0; i--)
        {
            var current = _middlewares[i];
            var next = pipeline;
            pipeline = (parsingResult) => current(parsingResult, next);
        }

        return pipeline;
    }

    [MemberNotNull(nameof(_configureRootCommand))]
    private void ThrowIfRootCommandBuilderIsNull()
    {
        if (_configureRootCommand is null)
        {
            throw new InvalidOperationException("The current command needs to be set before performing this action");
        }
    }

    // TODO: move somewhere?
    // like a class of built-in middlewares
    private static void FinalMiddleware(ParsingResult parsingResult)
    {
        if (!parsingResult.IsSuccess)
        {
            return;
        }

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