namespace CommandLineParser.Models;

internal class OptionDescriptor
{
    public OptionDescriptor(
        string fullName,
        string description,
        IReadOnlyList<IArgumentDescriptor> arguments,
        char? shortName = null,
        bool required = false)
    {
        // NOTE: if required and arguments - empty -> does not make sense
        // maybe check for it when building the option?
        FullName = fullName;
        Description = description;
        // NOTE: only the last argument can be repeated, need to enforce it probably
        Arguments = arguments;
        ShortName = shortName;
        Required = required;
    }

    public string FullName { get; }

    public string Description { get; }

    public char? ShortName { get; }

    public bool Required { get; }

    public IReadOnlyList<IArgumentDescriptor> Arguments { get; }
}