using CommandLineParser.Models;

namespace CommandLineParser.Application.Builders;

// This interface and class is only there to store generic converter in a non-generic builder
// Perhaps there is a better way, but for now this hack will do
internal interface IArgumentCreator
{
    IArgumentDescriptor Create(string name, string? description, bool isRepeated);
}

internal class ArgumentCreator<T> : IArgumentCreator
{
    private readonly Func<string, T>? _converter;

    public ArgumentCreator(Func<string, T>? converter)
    {
        _converter = converter;
    }

    public IArgumentDescriptor Create(string name, string? description, bool isRepeated)
    {
        return new ArgumentDescriptor<T>(
            name: name,
            description: description,
            repeated: isRepeated,
            converter: _converter);
    }
}