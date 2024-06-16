namespace CommandLineParser.Models;

internal class OptionDescriptor
{
    public OptionDescriptor(
        string fullName,
        string description,
        IArgumentDescriptor? argument = null,
        char? shortName = null,
        bool required = false)
    {
        FullName = fullName;
        Description = description;
        Argument = argument;
        ShortName = shortName;
        Required = required;
    }

    public string FullName { get; }

    public string Description { get; }

    public char? ShortName { get; }

    public bool Required { get; }

    public IArgumentDescriptor? Argument { get; }
}