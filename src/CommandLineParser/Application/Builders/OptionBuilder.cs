using System.Diagnostics.CodeAnalysis;
using CommandLineParser.Models;

namespace CommandLineParser.Application.Builders;

internal interface IOptionNameBuilder
{
    IOptionDescriptionBuilder WithName(string name);
}

internal interface IOptionDescriptionBuilder
{
    IOptionShortNameBuilder WithDescription(string description);
}

internal interface IOptionShortNameBuilder : IOptionRequiredBuilder
{
    IOptionRequiredBuilder WithShortName(char shortName);
}

internal interface IOptionRequiredBuilder : IOptionArgumentBuilder
{
    IOptionArgumentBuilder IsRequired();
}

internal interface IOptionArgumentBuilder
{
    IOptionArgumentNameBuilder HasArgumentOfType<T>(Func<string, T>? converter = null);
}

internal interface IOptionArgumentNameBuilder : IOptionArgumentRepeatedBuilder
{
    IOptionArgumentRepeatedBuilder ThatHasName(string argumentName);
}

internal interface IOptionArgumentRepeatedBuilder
{
    void ThatIsRepeated();
}

internal class OptionBuilder
    : IOptionNameBuilder,
      IOptionDescriptionBuilder,
      IOptionShortNameBuilder,
      IOptionArgumentNameBuilder
{
    private string? _name;
    private string? _description;
    private char? _shortName;
    private bool _isRequired;
    private IArgumentCreator? _argumentCreator;
    private string? _argumentName;
    private bool _argumentIsRepeated;

    public IOptionDescriptionBuilder WithName(string name)
    {
        // TODO: enforce '--' here?
        _name = name;
        return this;
    }

    public IOptionShortNameBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public IOptionRequiredBuilder WithShortName(char shortName)
    {
        _shortName = shortName;
        return this;
    }

    public IOptionArgumentBuilder IsRequired()
    {
        _isRequired = true;
        return this;
    }

    public IOptionArgumentNameBuilder HasArgumentOfType<T>(Func<string, T>? converter = null)
    {
        _argumentCreator = new ArgumentCreator<T>(converter);
        return this;
    }

    public IOptionArgumentRepeatedBuilder ThatHasName(string argumentName)
    {
        _argumentName = argumentName;
        return this;
    }

    public void ThatIsRepeated()
    {
        _argumentIsRepeated = true;
    }

    public OptionDescriptor Build()
    {
        ThrowIfInvalid();
        IArgumentDescriptor? argument = null;
        if (_argumentCreator is not null)
        {
            _argumentName ??= GetArgumentNameFromOptionName(_name);
            argument = _argumentCreator.Create(
                name: _argumentName,
                description: null,
                isRepeated: _argumentIsRepeated);
        }

        return new(
            fullName: _name,
            description: _description,
            argument: argument,
            shortName: _shortName,
            required: _isRequired);
    }

    [MemberNotNull(nameof(_name), nameof(_description))]
    private void ThrowIfInvalid()
    {
        if (_name is null || _description is null)
        {
            throw new InvalidOperationException("You need to set name and description for the option");
        }
    }

    private static string GetArgumentNameFromOptionName(string optionName)
    {
        var name = optionName[OptionDescriptor.FullNameOptionPrefix.Length..];
        return $"<{name}>";
    }
}