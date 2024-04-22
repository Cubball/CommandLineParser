using CommandLineParser.Models;

namespace CommandLineParser.Parsing;

internal class Parser
{
    private const string FullNameOptionPrefix = "--";
    // TODO: extract these into config class?
    // also, should this be a string?
    private const string ShortNameOptionPrefix = "-";

    private CommandDescriptor _currentCommand;
    private bool _parseOptions = true;
    private int _currnetPositionalArgumentIndex;

    public Parser(CommandDescriptor rootCommand) => _currentCommand = rootCommand;

    public void Parse(string[] args)
    {
        var spanOfArgs = ParseSubcommands(args);
        while (!spanOfArgs.IsEmpty)
        {
            if (!_parseOptions)
            {
                spanOfArgs = ParsePositionalArgument(spanOfArgs);
                continue;
            }

            var arg = spanOfArgs[0];
            if (IsFullNameOption(arg))
            {
                spanOfArgs = ParseFullNameOption(spanOfArgs);
                continue;
            }

            if (IsShortNameOptions(arg))
            {
                spanOfArgs = ParseShortNameOptions(spanOfArgs);
                continue;
            }

            spanOfArgs = ParsePositionalArgument(spanOfArgs);
        }
    }

    private Span<string> ParseSubcommands(Span<string> args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var subcommand = _currentCommand.Subcommands.FirstOrDefault(c => c.Name == arg);
            if (subcommand is null)
            {
                return args[i..];
            }

            _currentCommand = subcommand;
        }

        return [];
    }

    private Span<string> ParseFullNameOption(Span<string> args)
    {
        var arg = args[0];
        if (arg == FullNameOptionPrefix)
        {
            _parseOptions = false;
            return args[1..];
        }

        var option = _currentCommand.Options.FirstOrDefault(o => o.FullName == arg);
        if (option is null)
        {
            // TODO: unknown option, error here
            throw new Exception();
        }

        var index = 1;
        foreach (var argument in option.Arguments)
        {
            if (index >= args.Length)
            {
                // TODO: not enough args provided, error here
                throw new Exception();
            }

            // TODO: consider case when argument.Repeated
            var converted = argument.TryConvert(args[index], out var convertedValue);
            if (!converted)
            {
                // TODO: failed to convert arg, error here
                throw new Exception();
            }

            // TODO: add convertedValue to some sort of collection of parsed args
            index++;
        }

        return args[index..];
    }

    private Span<string> ParseShortNameOptions(Span<string> args)
    {
        // TODO:
        return args;
    }

    private Span<string> ParsePositionalArgument(Span<string> args)
    {
        // TODO:
        return args;
    }

    // NOTE: should these be extracted?
    private static bool IsFullNameOption(string arg) => arg.StartsWith(FullNameOptionPrefix, StringComparison.InvariantCulture);

    private static bool IsShortNameOptions(string arg) => arg.StartsWith(ShortNameOptionPrefix, StringComparison.InvariantCulture);
}