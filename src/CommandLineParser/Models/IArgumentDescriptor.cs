using System.Diagnostics.CodeAnalysis;

namespace CommandLineParser.Models;

internal interface IArgumentDescriptor
{
    string Name { get; }

    int Index { get; }

    bool Repeated { get; }

    bool TryConvert(string rawValue, [NotNullWhen(true)] out object? converted);
}