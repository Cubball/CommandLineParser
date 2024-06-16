using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace CommandLineParser.Models;

internal class ArgumentDescriptor<T> : IArgumentDescriptor
{
    private readonly Func<string, T>? _converter;

    public ArgumentDescriptor(
        string name,
        bool repeated = false,
        Func<string, T>? converter = null)
    {
        Name = name;
        Repeated = repeated;
        _converter = converter;
    }

    public string Name { get; }

    public bool Repeated { get; }

    public bool TryConvert(string rawValue, [NotNullWhen(true)] out object? converted)
    {
        try
        {
            var value = _converter is null
                ? ConvertUsingDefault(rawValue)
                : _converter(rawValue);
            converted = value;
            return converted is not null;
        }
        catch
        {
            converted = null;
            return false;
        }
    }

    private static object? ConvertUsingDefault(string rawValue)
    {
        return Convert.ChangeType(rawValue, typeof(T), CultureInfo.InvariantCulture);
    }
}