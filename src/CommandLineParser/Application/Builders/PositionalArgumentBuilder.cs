using System.Diagnostics.CodeAnalysis;
using CommandLineParser.Models;

namespace CommandLineParser.Application.Builders;

internal interface IPositionalArgumentNameBuilder
{
    IPositionalArgumentDescriptionBuilder WithName(string name);
}

internal interface IPositionalArgumentDescriptionBuilder
{
    IPositionalArgumentTypeBuilder WithDescription(string description);
}

internal interface IPositionalArgumentTypeBuilder
{
    IPositionalArgumentRepeatedBuilder IsOfType<T>(Func<string, T>? converter = null);
}

internal interface IPositionalArgumentRepeatedBuilder
{
    void IsRepeated();
}

internal class PositionalArgumentBuilder
    : IPositionalArgumentNameBuilder,
      IPositionalArgumentDescriptionBuilder,
      IPositionalArgumentTypeBuilder,
      IPositionalArgumentRepeatedBuilder
{
    private string? _name;
    private string? _description;
    private bool _isRepeated;
    private IArgumentCreator? _argumentCreator;

    public IPositionalArgumentDescriptionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public IPositionalArgumentTypeBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public IPositionalArgumentRepeatedBuilder IsOfType<T>(Func<string, T>? converter = null)
    {
        _argumentCreator = new ArgumentCreator<T>(converter);
        return this;
    }

    public void IsRepeated()
    {
        _isRepeated = true;
    }

    public IArgumentDescriptor Build()
    {
        ThrowIfInvalid();
        return _argumentCreator.Create(_name, _description, _isRepeated);
    }

    [MemberNotNull(nameof(_name), nameof(_description), nameof(_argumentCreator))]
    private void ThrowIfInvalid()
    {
        if (_name is null || _description is null || _argumentCreator is null)
        {
            throw new InvalidOperationException("Name, description and type need to be set before performing this action");
        }
    }
}