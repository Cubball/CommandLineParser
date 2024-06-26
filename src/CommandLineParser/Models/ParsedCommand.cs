using System.Diagnostics.CodeAnalysis;
using CommandLineParser.Models.Exceptions;

namespace CommandLineParser.Models;

internal record ParsedCommand(
    CommandDescriptor Command,
    IReadOnlyDictionary<IArgumentDescriptor, List<object>> ParsedPositionalArguments,
    IReadOnlyList<OptionDescriptor> ParsedFlags,
    IReadOnlyDictionary<OptionDescriptor, List<object>> ParsedOptions)
{
    // TODO: docs
    public T GetRequiredOptionValue<T>(string fullName)
    {
        var option = GetOptionOrThrow(fullName);
        return GetParsedOptionValueOrThrow<T>(option);
    }

    public T GetRequiredOptionValue<T>(char shortName)
    {
        var option = GetOptionOrThrow(shortName);
        return GetParsedOptionValueOrThrow<T>(option);
    }

    public List<T> GetRequiredOptionValues<T>(string fullName)
    {
        var option = GetOptionOrThrow(fullName);
        return GetParsedOptionValuesOrThrow<T>(option);
    }

    public List<T> GetRequiredOptionValues<T>(char shortName)
    {
        var option = GetOptionOrThrow(shortName);
        return GetParsedOptionValuesOrThrow<T>(option);
    }

    public bool TryGetOptionValue<T>(string fullName, [NotNullWhen(true)] out T? value)
    {
        var option = GetOptionOrThrow(fullName);
        return TryGetOptionValue(option, out value);
    }

    public bool TryGetOptionValue<T>(char shortName, [NotNullWhen(true)] out T? value)
    {
        var option = GetOptionOrThrow(shortName);
        return TryGetOptionValue(option, out value);
    }

    public bool TryGetOptionValues<T>(string fullName, [NotNullWhen(true)] out List<T>? values)
    {
        var option = GetOptionOrThrow(fullName);
        return TryGetOptionValues(option, out values);
    }

    public bool TryGetOptionValues<T>(char shortName, [NotNullWhen(true)] out List<T>? values)
    {
        var option = GetOptionOrThrow(shortName);
        return TryGetOptionValues(option, out values);
    }

    public bool GetFlag(string fullName)
    {
        var option = Command.Options.FirstOrDefault(c => c.FullName == fullName);
        return option is not null;
    }

    public bool GetFlag(char shortName)
    {
        var option = Command.Options.FirstOrDefault(c => c.ShortName == shortName);
        return option is not null;
    }

    public T GetPositionalArgumentValue<T>(int index)
    {
        if (index >= Command.Arguments.Count)
        {
            throw new PositionalArgumentNotFoundException();
        }

        var argument = Command.Arguments[index];
        // If ParsedCommand is constructed, then all the Command's Arguments should be parsed
        var values = ParsedPositionalArguments[argument];
        try
        {
            return (T)values[0];
        }
        catch
        {
            throw new TypeConversionFailedException();
        }
    }

    public List<T> GetPositionalArgumentValues<T>(int index)
    {
        if (index >= Command.Arguments.Count)
        {
            throw new PositionalArgumentNotFoundException();
        }

        var argument = Command.Arguments[index];
        // If ParsedCommand is constructed, then all the Command's Arguments should be parsed
        var values = ParsedPositionalArguments[argument];
        try
        {
            return values.ConvertAll(v => (T)v);
        }
        catch
        {
            throw new TypeConversionFailedException();
        }
    }

    private OptionDescriptor GetOptionOrThrow(string fullName)
    {
        return Command.Options.FirstOrDefault(c => c.FullName == fullName)
            ?? throw new OptionNotFoundException();
    }

    private OptionDescriptor GetOptionOrThrow(char shortName)
    {
        return Command.Options.FirstOrDefault(c => c.ShortName == shortName)
            ?? throw new OptionNotFoundException();
    }

    private T GetParsedOptionValueOrThrow<T>(OptionDescriptor option)
    {
        if (!ParsedOptions.TryGetValue(option, out var values))
        {
            throw new ParsedOptionNotFoundException();
        }

        var value = values[0];
        try
        {
            return (T)value;
        }
        catch
        {
            throw new TypeConversionFailedException();
        }
    }

    private List<T> GetParsedOptionValuesOrThrow<T>(OptionDescriptor option)
    {
        if (!ParsedOptions.TryGetValue(option, out var values))
        {
            throw new ParsedOptionNotFoundException();
        }

        try
        {
            return values.ConvertAll(v => (T)v);
        }
        catch
        {
            throw new TypeConversionFailedException();
        }
    }

    private bool TryGetOptionValue<T>(OptionDescriptor option, [NotNullWhen(true)] out T? value)
    {
        if (!ParsedOptions.TryGetValue(option, out var values))
        {
            value = default;
            return false;
        }

        try
        {
            value = (T)values[0];
            return true;
        }
        catch
        {
            throw new TypeConversionFailedException();
        }
    }

    private bool TryGetOptionValues<T>(OptionDescriptor option, [NotNullWhen(true)] out List<T>? values)
    {
        if (!ParsedOptions.TryGetValue(option, out var rawValues))
        {
            values = default;
            return false;
        }

        try
        {
            values = rawValues.ConvertAll(v => (T)v);
            return true;
        }
        catch
        {
            throw new TypeConversionFailedException();
        }
    }
}