namespace CommandLineParser.Models;

internal record ParsedCommand(
    CommandDescriptor Command,
    IReadOnlyDictionary<IArgumentDescriptor, List<object>> ParsedPositionalArguments,
    IReadOnlyList<OptionDescriptor> ParsedFlags,
    IReadOnlyDictionary<OptionDescriptor, List<object>> ParsedOptions)
{
    // TODO: shortName as well
    public T GetRequiredOptionValue<T>(string fullName)
    {
        var option = Command.Options.FirstOrDefault(c => c.FullName == fullName);
        // throw if null
        // try/catch
        var value = ParsedOptions[option!][0];
        return (T)value;
    }

    public List<T> GetRequiredOptionValues<T>(string fullName)
    {
        var option = Command.Options.FirstOrDefault(c => c.FullName == fullName);
        // throw if null
        var values = ParsedOptions[option!];
        // try/catch
        return values.ConvertAll(v => (T)v);
    }

    public bool GetFlag(string fullName)
    {
        var option = Command.Options.FirstOrDefault(c => c.FullName == fullName);
        return option is not null;
    }
}