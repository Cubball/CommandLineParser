using CommandLineParser.Models;
using CommandLineParser.Parsing.Models;

namespace CommandLineParser.Parsing;

internal class Parser
{
    private const char FullNameOptionValueSeparator = '=';

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
        if (arg == OptionDescriptor.FullNameOptionPrefix)
        {
            _parseOptions = false;
            return args[1..];
        }

        var separatorIndex = arg.IndexOf(FullNameOptionValueSeparator);
        if (separatorIndex != -1)
        {
            return ParseFullNameOptionWithSeparator(args, arg, separatorIndex);
        }

        var option = _currentCommand.Options.FirstOrDefault(o => o.FullName == arg);
        if (option is null)
        {
            _parsingResultBuilder.AddError(new(ParsingErrorType.UnknownOption, null, arg));
            return [];
        }

        return ParseOption(args[1..], option);
    }

    private Span<string> ParseFullNameOptionWithSeparator(Span<string> args, string arg, int separatorIndex)
    {
        var optionName = arg[..separatorIndex];
        var optionValue = arg[(separatorIndex + 1)..];
        var option = _currentCommand.Options.FirstOrDefault(o => o.FullName == optionName);
        if (option is null)
        {
            _parsingResultBuilder.AddError(new(ParsingErrorType.UnknownOption, null, arg));
            return [];
        }

        if (option.Argument is null)
        {
            _parsingResultBuilder.AddError(new(ParsingErrorType.ArgumentValueForFlag, option.FullName, optionValue));
            return [];
        }

        args[0] = optionValue;
        return ParseSingleArgumentValue(args, option.Argument);
    }

    private Span<string> ParseShortNameOptions(Span<string> args)
    {
        var arg = args[0];
        var shortOptions = arg.AsSpan(OptionDescriptor.ShortNameOptionPrefix.Length);
        for (var i = 0; i < shortOptions.Length; i++)
        {
            var shortName = shortOptions[i];
            var option = _currentCommand.Options.FirstOrDefault(o => o.ShortName == shortName);
            if (option is null)
            {
                _parsingResultBuilder.AddError(new(ParsingErrorType.UnknownOption, null, shortName.ToString()));
                return [];
            }

            if (option.Argument is not null)
            {
                if (i + 1 == shortOptions.Length)
                {
                    return ParseOptionWithArguments(args[1..], option);
                }

                args[0] = shortOptions[(i + 1)..].ToString();
                return ParseSingleArgumentValue(args, option.Argument);
            }

            _parsingResultBuilder.AddFlag(option);
        }

        return args[1..];
    }

    private Span<string> ParsePositionalArgument(Span<string> args)
    {
        if (_currentPositionalArgumentIndex >= _currentCommand.Arguments.Count)
        {
            _parsingResultBuilder.AddError(new(ParsingErrorType.UnknownToken, null, args[0]));
            return [];
        }

        var argument = _currentCommand.Arguments[_currentPositionalArgumentIndex];
        _currentPositionalArgumentIndex++;
        return ParseArgument(args, argument);
    }

    private Span<string> ParseOption(Span<string> args, OptionDescriptor option)
    {
        if (option.Argument is null)
        {
            _parsingResultBuilder.AddFlag(option);
            return args;
        }

        return ParseOptionWithArguments(args, option);
    }

    private Span<string> ParseOptionWithArguments(Span<string> args, OptionDescriptor option)
    {
        var argument = option.Argument!;
        if (args.IsEmpty)
        {
            _parsingResultBuilder.AddError(new(ParsingErrorType.MissingArgumentValue, argument.Name));
            return [];
        }

        return ParseArgument(args, argument);
    }

    private Span<string> ParseArgument(
        Span<string> args,
        IArgumentDescriptor argument)
    {
        args = ParseSingleArgumentValue(args, argument);
        if (!argument.Repeated)
        {
            return args;
        }

        while (!args.IsEmpty)
        {
            // TODO: make this configurable? either stop parsing repeated arguments when encountered '--...' or no
            // also, check if this option actually exists?
            if (_parseOptions && (IsFullNameOption(args[0]) || IsShortNameOptions(args[0])))
            {
                return args;
            }

            args = ParseSingleArgumentValue(args, argument);
        }

        return [];
    }

    private Span<string> ParseSingleArgumentValue(
        Span<string> args,
        IArgumentDescriptor argument)
    {
        if (!argument.TryConvert(args[0], out var convertedValue))
        {
            _parsingResultBuilder.AddError(new(ParsingErrorType.ConversionFailed, argument.Name, args[0]));
            return [];
        }

        _parsingResultBuilder.AddParsedArgumentValue(argument, convertedValue);
        return args[1..];
    }

    private static bool IsFullNameOption(string arg) => arg.StartsWith(OptionDescriptor.FullNameOptionPrefix, StringComparison.InvariantCulture);

    private static bool IsShortNameOptions(string arg) => arg.StartsWith(OptionDescriptor.ShortNameOptionPrefix, StringComparison.InvariantCulture)
        && arg.Length > OptionDescriptor.ShortNameOptionPrefix.Length;
}