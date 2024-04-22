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
    private int _currentPositionalArgumentIndex;

    public Parser(CommandDescriptor rootCommand) => _currentCommand = rootCommand;

    public void Parse(string[] args)
    {
        // TODO: split args in form '--foo=bar' and alike into '--foo' and 'bar' before actually parsing
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

        return ParseOption(args[1..], option);
    }

    private Span<string> ParseShortNameOptions(Span<string> args)
    {
        var shortOptions = args[0].AsSpan(ShortNameOptionPrefix.Length);
        for (var i = 0; i < shortOptions.Length - 1; i++)
        {
            var shortName = shortOptions[i];
            var option = _currentCommand.Options.FirstOrDefault(o => o.ShortName == shortName);
            if (option is null)
            {
                // TODO: unknown option, error here
                throw new Exception();
            }

            if (option.Arguments.Count > 0)
            {
                // TODO: option with values can only be last in a group of shortOptions, error here
                throw new Exception();
            }

            // TODO: add option into collection of parsed args as a flag (i.e. option with no arguments)
        }

        var lastOptionName = shortOptions[^1];
        var lastOption = _currentCommand.Options.FirstOrDefault(o => o.ShortName == lastOptionName);
        if (lastOption is null)
        {
            // TODO: unknown option, error here
            throw new Exception();
        }

        if (lastOption.Arguments.Count == 0)
        {
            // TODO: add option into collection of parsed args as a flag (i.e. option with no arguments)
            return args[1..];
        }

        return ParseOption(args[1..], lastOption);
    }

    private Span<string> ParsePositionalArgument(Span<string> args)
    {
        if (_currentPositionalArgumentIndex >= _currentCommand.Arguments.Count)
        {
            // TODO: ran out of pos args, unknown token, error here
            throw new Exception();
        }

        var argument = _currentCommand.Arguments[_currentPositionalArgumentIndex];
        _currentPositionalArgumentIndex++;
        // TODO: put actual function instead of lambda
        return ParseArgument(args, argument, argument.Name, (key, value) => { });
    }

    private Span<string> ParseOption(Span<string> args, OptionDescriptor option)
    {
        foreach (var argument in option.Arguments)
        {
            if (args.IsEmpty)
            {
                // TODO: not enough args provided, error here
                throw new Exception();
            }

            args = ParseArgument(args, argument, option.FullName, (key, value) => { });
        }

        return args;
    }

    private Span<string> ParseArgument(
        Span<string> args,
        IArgumentDescriptor argument,
        string key,
        Action<string, object> addParsedValue)
    {
        if (args[0] == FullNameOptionPrefix)
        {
            _parseOptions = false;
            return args[1..];
        }

        if (!argument.Repeated)
        {
            return ParseSingleArgumentValue(args, argument, key, addParsedValue);
        }

        while (!args.IsEmpty)
        {
            if (args[0] == FullNameOptionPrefix)
            {
                _parseOptions = false;
                return args[1..];
            }

            args = ParseSingleArgumentValue(args, argument, key, addParsedValue);
        }

        return [];
    }

    private static Span<string> ParseSingleArgumentValue(
        Span<string> args,
        IArgumentDescriptor argument,
        string key,
        Action<string, object> addParsedValue)
    {
        if (!argument.TryConvert(args[0], out var convertedValue))
        {
            // TODO: failed to convert arg, error here
            throw new Exception();
        }

        addParsedValue(key, convertedValue);
        return args[1..];
    }

    // NOTE: should these be extracted?
    private static bool IsFullNameOption(string arg) => arg.StartsWith(FullNameOptionPrefix, StringComparison.InvariantCulture);

    private static bool IsShortNameOptions(string arg) => arg.StartsWith(ShortNameOptionPrefix, StringComparison.InvariantCulture);
}