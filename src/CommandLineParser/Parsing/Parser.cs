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
        _parsingResultBuilder.SetCurrentCommand(_currentCommand);
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
        if (shortOptions.IsEmpty)
        {
            _parsingResultBuilder.AddError(new(arg, $"Encountered an unknown symbol: {arg}"));
            return [];
        }

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

            _parsingResultBuilder.AddFlag(option);
        }

        var lastOptionName = shortOptions[^1];
        var lastOption = _currentCommand.Options.FirstOrDefault(o => o.ShortName == lastOptionName);
        if (lastOption is null)
        {
            _parsingResultBuilder.AddError(new(lastOptionName.ToString(), $"Encountered an unknown short option: {ShortNameOptionPrefix}{lastOptionName}"));
            return [];
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
        return ParseArgument(args, argument);
    }

    private Span<string> ParseOption(Span<string> args, OptionDescriptor option)
    {
        if (option.Arguments.Count == 0)
        {
            _parsingResultBuilder.AddFlag(option);
            return args;
        }

        return ParseOptionWithArguments(args, option);
    }

    private Span<string> ParseOptionWithArguments(Span<string> args, OptionDescriptor option)
    {
        foreach (var argument in option.Arguments)
        {
            if (args.IsEmpty)
            {
                _parsingResultBuilder.AddError(new(argument.Name, $"No args were provided for argument '{argument.Name}' for the '{option.FullName}' option"));
                return [];
            }

            args = ParseArgument(args, argument);
        }

        return args;
    }

    private Span<string> ParseArgument(
        Span<string> args,
        IArgumentDescriptor argument)
    {
        if (!argument.Repeated)
        {
            return ParseSingleArgumentValue(args, argument);
        }

        var noValuesParsed = true;
        while (!args.IsEmpty)
        {
            // NOTE: traditionally, '--' is used to terminate the list of options and to tell the program to treat the rest of the args as positional arguments.
            // This usage is still present here, however '--' can also be used to terminate a list of values of an argument that accepts an arbitrary number of values
            //
            // Extract it into settings?
            if (args[0] == FullNameOptionPrefix)
            {
                if (noValuesParsed)
                {
                    _parsingResultBuilder.AddError(new(argument.Name, $"Argument {argument.Name} requires at least 1 value, but {FullNameOptionPrefix} was encountered before any of the values"));
                    return [];
                }

                return args[1..];
            }

            args = ParseSingleArgumentValue(args, argument);
            noValuesParsed = false;
        }

        return [];
    }

    private Span<string> ParseSingleArgumentValue(
        Span<string> args,
        IArgumentDescriptor argument)
    {
        if (!argument.TryConvert(args[0], out var convertedValue))
        {
            _parsingResultBuilder.AddError(new(args[0], $"Failed to convert arg '{args[0]}' to target type"));
            return [];
        }

        _parsingResultBuilder.AddParsedArgumentValue(argument, convertedValue);
        return args[1..];
    }

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