using System.Diagnostics.CodeAnalysis;
using CommandLineParser.Models;

namespace CommandLineParser.Application.Builders;

internal interface ICommandNameBuilder
{
    ICommandDescriptionBuilder WithName(string name);
}

internal interface ICommandDescriptionBuilder
{
    ICommandComponentsBuilder WithDescription(string description);
}

internal interface ICommandComponentsBuilder
{
    ICommandComponentsBuilder HasSubcommand(Action<ICommandNameBuilder> configure);

    ICommandComponentsBuilder HasPositionalArgument(Action<IPositionalArgumentNameBuilder> configure);

    ICommandComponentsBuilder HasOption(Action<IOptionNameBuilder> configure);

    void IsHandledBy(Action<ParsedCommand> handler);
}

internal class CommandBuilder
    : ICommandNameBuilder,
      ICommandDescriptionBuilder,
      ICommandComponentsBuilder
{
    private readonly List<Action<ICommandNameBuilder>> _subcommandBuilders = [];
    private readonly List<Action<IPositionalArgumentNameBuilder>> _positionalArgumentBuilders = [];
    private readonly List<Action<IOptionNameBuilder>> _optionBuilders = [];

    private string? _name;
    private string? _description;
    private Action<ParsedCommand>? _handler;

    public ICommandDescriptionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ICommandComponentsBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ICommandComponentsBuilder HasSubcommand(Action<ICommandNameBuilder> configure)
    {
        _subcommandBuilders.Add(configure);
        return this;
    }

    public ICommandComponentsBuilder HasPositionalArgument(Action<IPositionalArgumentNameBuilder> configure)
    {
        _positionalArgumentBuilders.Add(configure);
        return this;
    }

    public ICommandComponentsBuilder HasOption(Action<IOptionNameBuilder> configure)
    {
        _optionBuilders.Add(configure);
        return this;
    }

    public void IsHandledBy(Action<ParsedCommand> handler)
    {
        _handler = handler;
    }

    public CommandDescriptor Build()
    {
        ThrowIfInvalid();
        var subcommands = BuildSubcommands();
        var positionalArguments = BuildPositionalArguments();
        var options = BuildOptions();
        return new(_name, _description, subcommands, positionalArguments, options, _handler);
    }

    [MemberNotNull(nameof(_name), nameof(_description), nameof(_handler))]
    private void ThrowIfInvalid()
    {
        if (_name is null || _description is null || _handler is null)
        {
            throw new InvalidOperationException("Name, description and handler need to be set before performing this action");
        }
    }

    private List<CommandDescriptor> BuildSubcommands()
    {
        return _subcommandBuilders.ConvertAll(configure =>
        {
            var builder = new CommandBuilder();
            configure(builder);
            return builder.Build();
        });
    }

    private List<IArgumentDescriptor> BuildPositionalArguments()
    {
        return _positionalArgumentBuilders.ConvertAll(configure =>
        {
            var builder = new PositionalArgumentBuilder();
            configure(builder);
            return builder.Build();
        });
    }

    private List<OptionDescriptor> BuildOptions()
    {
        return _optionBuilders.ConvertAll(configure =>
        {
            var builder = new OptionBuilder();
            configure(builder);
            return builder.Build();
        });
    }
}