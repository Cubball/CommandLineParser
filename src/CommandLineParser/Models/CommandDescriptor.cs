namespace CommandLineParser.Models;

internal class CommandDescriptor
{
    public CommandDescriptor(
        string name,
        string description,
        IReadOnlyList<CommandDescriptor> subcommands,
        IReadOnlyList<IArgumentDescriptor> arguments,
        IReadOnlyList<OptionDescriptor> options,
        Action<ParsedCommand> handler)
    {
        Name = name;
        Description = description;
        Subcommands = subcommands;
        Arguments = arguments;
        Options = options;
        Handler = handler;
    }

    public string Name { get; }

    public string Description { get; }

    public IReadOnlyList<CommandDescriptor> Subcommands { get; }

    public IReadOnlyList<IArgumentDescriptor> Arguments { get; }

    public IReadOnlyList<OptionDescriptor> Options { get; }

    public Action<ParsedCommand> Handler { get; }
}