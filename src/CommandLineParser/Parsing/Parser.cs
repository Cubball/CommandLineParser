using CommandLineParser.Models;

namespace CommandLineParser.Parsing;

internal class Parser
{
    private const string FullNameOptionPrefix = "--";
    // TODO: extract these into config class?
    // also, should this be a string?
    private const string ShortNameOptionPrefix = "-";
    private static readonly char[] Separators = ['='];

    private readonly IParsingResultBuilder _parsingResultBuilder;

    private CommandDescriptor _currentCommand;
    private bool _parseOptions = true;
    private int _currentPositionalArgumentIndex;

    public Parser(IParsingResultBuilder parsingResultBuilder, CommandDescriptor rootCommand)
    {
        _parsingResultBuilder = parsingResultBuilder;
        _currentCommand = rootCommand;
    }

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
            _parsingResultBuilder.SetCurrentCommand(_currentCommand);
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

        args = SplitFirstArgOnSeparators(args, out arg);
        var option = _currentCommand.Options.FirstOrDefault(o => o.FullName == arg);
        if (option is null)
        {
            _parsingResultBuilder.AddError(new(arg, $"Encountered an unknown option: {arg}"));
            return [];
        }

        return ParseOption(args, option);
    }

    private Span<string> ParseShortNameOptions(Span<string> args)
    {
        args = SplitFirstArgOnSeparators(args, out var arg);
        var shortOptions = arg.AsSpan(ShortNameOptionPrefix.Length);
        for (var i = 0; i < shortOptions.Length - 1; i++)
        {
            var shortName = shortOptions[i];
            var option = _currentCommand.Options.FirstOrDefault(o => o.ShortName == shortName);
            if (option is null)
            {
                _parsingResultBuilder.AddError(new(shortName.ToString(), $"Encountered an unknown short option: {ShortNameOptionPrefix}{shortName}"));
                return [];
            }

            if (option.Arguments.Count > 0)
            {
                _parsingResultBuilder.AddError(new(option.FullName, $"Option '{option.FullName}' requires 1 or more arguments, but is not the last one in the gruop of short options"));
                return [];
            }

            _parsingResultBuilder.AddFlag(option.FullName);
        }

        var lastOptionName = shortOptions[^1];
        var lastOption = _currentCommand.Options.FirstOrDefault(o => o.ShortName == lastOptionName);
        if (lastOption is null)
        {
            _parsingResultBuilder.AddError(new(lastOptionName.ToString(), $"Encountered an unknown short option: {ShortNameOptionPrefix}{lastOptionName}"));
            return [];
        }

        if (lastOption.Arguments.Count == 0)
        {
            _parsingResultBuilder.AddFlag(lastOption.FullName);
        }

        return ParseOption(args, lastOption);
    }

    private Span<string> ParsePositionalArgument(Span<string> args)
    {
        if (_currentPositionalArgumentIndex >= _currentCommand.Arguments.Count)
        {
            _parsingResultBuilder.AddError(new(args[0], $"Encountered an unknown token '{args[0]}' that is not the argument of any command or option"));
            return [];
        }

        var argument = _currentCommand.Arguments[_currentPositionalArgumentIndex];
        _currentPositionalArgumentIndex++;
        return ParseArgument(args, argument, argument, _parsingResultBuilder.AddParsedArgumentValue);
    }

    private Span<string> ParseOption(Span<string> args, OptionDescriptor option)
    {
        foreach (var argument in option.Arguments)
        {
            if (!_parseOptions)
            {
                _parsingResultBuilder.AddError(new(argument.Name, $"Expected to see argument '{argument.Name}' for the '{option.FullName}' option, but encountered '{FullNameOptionPrefix}'"));
                return [];
            }

            if (args.IsEmpty)
            {
                _parsingResultBuilder.AddError(new(argument.Name, $"No args were provided for argument '{argument.Name}' for the '{option.FullName}' option"));
                return [];
            }

            args = ParseArgument(args, argument, option, _parsingResultBuilder.AddParsedOptionValue);
        }

        return args;
    }

    private Span<string> ParseArgument<T>(
        Span<string> args,
        IArgumentDescriptor argument,
        T key,
        Action<T, object> addValue)
    {
        if (args[0] == FullNameOptionPrefix)
        {
            _parseOptions = false;
            return args[1..];
        }

        if (!argument.Repeated)
        {
            return ParseSingleArgumentValue(args, argument, key, addValue);
        }

        while (!args.IsEmpty)
        {
            // WARN: this means if we have a command with multiple arguments that accept arbitrary number of values,
            // like arg1 and arg2, then we can do this:
            // command arg1_1 arg1_2 arg1_3 -- arg2_1 arg2_2
            // and it would be interpreted as arg1 having 3 values and arg2 having 2 values
            // this might be something that we do or do not want
            // if we'd want to remove this 'feature',
            // we'd need to check whether the arg being parsed is positional, or is related to some option
            if (args[0] == FullNameOptionPrefix)
            {
                _parseOptions = false;
                return args[1..];
            }

            args = ParseSingleArgumentValue(args, argument, key, addValue);
        }

        return [];
    }

    private Span<string> ParseSingleArgumentValue<T>(
        Span<string> args,
        IArgumentDescriptor argument,
        T key,
        Action<T, object> addValue)
    {
        if (!argument.TryConvert(args[0], out var convertedValue))
        {
            _parsingResultBuilder.AddError(new(args[0], $"Failed to convert arg '{args[0]}' to target type"));
            return [];
        }

        addValue(key, convertedValue);
        return args[1..];
    }

    // NOTE: should these be extracted?
    private static bool IsFullNameOption(string arg) => arg.StartsWith(FullNameOptionPrefix, StringComparison.InvariantCulture);

    private static bool IsShortNameOptions(string arg) => arg.StartsWith(ShortNameOptionPrefix, StringComparison.InvariantCulture);

    private static Span<string> SplitFirstArgOnSeparators(Span<string> args, out string newFirstArg)
    {
        var firstArg = args[0];
        var separatorIndex = firstArg.IndexOfAny(Separators);
        if (separatorIndex == -1)
        {
            newFirstArg = firstArg;
            return args[1..];
        }

        newFirstArg = firstArg[..separatorIndex];
        args[0] = firstArg[(separatorIndex + 1)..];
        return args;
    }
}