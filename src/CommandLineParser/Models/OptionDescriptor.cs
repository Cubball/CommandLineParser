namespace CommandLineParser.Models;

internal class OptionDescriptor
{
    public OptionDescriptor(
        string fullName,
        string description,
        char? shortName = null,
        bool required = false,
        IReadOnlyList<IArgumentDescriptor>? arguments = null)
    {
        FullName = fullName;
        Description = description;
        ShortName = shortName;
        Required = required;
        Arguments = arguments ?? [];
    }

    public string FullName { get; }

    public string Description { get; }

    public char? ShortName { get; }

    public bool Required { get; }

    public IReadOnlyList<IArgumentDescriptor> Arguments { get; }
}