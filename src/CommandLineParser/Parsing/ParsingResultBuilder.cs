using System.Diagnostics.CodeAnalysis;

using CommandLineParser.Models;
using CommandLineParser.Parsing.Models;

namespace CommandLineParser.Parsing;

internal class ParsingResultBuilder : IParsingResultBuilder
{
    // PERF: should this be hashset?
    private readonly List<OptionDescriptor> _flags = [];
    private readonly Dictionary<OptionDescriptor, List<object>> _options = [];
    private readonly Dictionary<IArgumentDescriptor, List<object>> _positionalArguments = [];
    private readonly List<ParsingError> _errors = [];

    private CommandDescriptor? _currentCommand;

    public void AddError(ParsingError error)
    {
        _errors.Add(error);
    }

    public void AddFlag(OptionDescriptor option)
    {
        _flags.Add(option);
    }

    public void AddParsedArgumentValue(IArgumentDescriptor argument, object value)
    {
        ThrowIfCurrentCommandIsNull();
        var argumentsOption = _currentCommand.Options
            .FirstOrDefault(o => o.Argument == argument);
        if (argumentsOption is null)
        {
            AddValueToDictionary(_positionalArguments, argument, value);
        }
        else
        {
            AddValueToDictionary(_options, argumentsOption, value);
        }
    }

    public void SetCurrentCommand(CommandDescriptor command)
    {
        _currentCommand = command;
    }

    public ParsingResult Build()
    {
        ThrowIfCurrentCommandIsNull();
        CheckMissingPositionalArguments();
        CheckMissingRequiredOptions();
        if (_errors.Count > 0)
        {
            var failureContext = new ParsingFailureContext(_currentCommand, _errors);
            return new(failureContext);
        }

        var successContext = new ParsingSuccessContext(
            Command: _currentCommand,
            ParsedPositionalArguments: _positionalArguments,
            ParsedFlags: _flags,
            ParsedOptions: _options);
        return new(successContext);
    }

    private void CheckMissingPositionalArguments()
    {
        var missingPositionalArguments = _currentCommand!.Arguments
            .Except(_positionalArguments.Keys)
            .ToArray();
        foreach (var missingArgument in missingPositionalArguments)
        {
            AddError(new(ParsingErrorType.MissingPositionalArgument, missingArgument.Name));
        }
    }

    private void CheckMissingRequiredOptions()
    {
        var requiredOptionArguments = _currentCommand!.Options
            .Where(o => o.Required && o.Argument is not null);
        var missingOptionArguments = requiredOptionArguments
            .Except(_options.Keys)
            .ToArray();
        foreach (var missingOption in missingOptionArguments)
        {
            AddError(new(ParsingErrorType.MissingRequiredOption, missingOption.FullName));
        }
    }

    [MemberNotNull(nameof(_currentCommand))]
    private void ThrowIfCurrentCommandIsNull()
    {
        if (_currentCommand is null)
        {
            throw new InvalidOperationException("The current command needs to be set before performing this action");
        }
    }

    private static void AddValueToDictionary<T>(Dictionary<T, List<object>> dictionary, T key, object value)
        where T : notnull
    {
        if (!dictionary.TryGetValue(key, out var values))
        {
            values = [];
            dictionary[key] = values;
        }

        values.Add(value);
    }
}