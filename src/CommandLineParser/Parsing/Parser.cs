using CommandLineParser.Models;

namespace CommandLineParser.Parsing;

internal class Parser
{
    private CommandDescriptor _currentCommand;

    public Parser(CommandDescriptor rootCommand) => _currentCommand = rootCommand;

    public void Parse(string[] args)
    {
        var spanOfArgs = ParseSubcommands(args);
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
        // TODO:
        return args;
    }

    private Span<string> ParseShortNameOptions(Span<string> args)
    {
        // TODO:
        return args;
    }

    private Span<string> ParsePositionalArgument(Span<string> args)
    {
        // TODO:
        return args;
    }
}