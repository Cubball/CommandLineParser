using CommandLineParser.Models;
using CommandLineParser.Models.Exceptions;
using FluentAssertions;

namespace CommandLineParser.Tests.Models;

public class ParsedCommandTests
{
    private readonly CommandDescriptor _command;
    private readonly Dictionary<IArgumentDescriptor, List<object>> _parsedArguments;
    private readonly List<OptionDescriptor> _parsedFlags;
    private readonly Dictionary<OptionDescriptor, List<object>> _parsedOptions;
    private readonly ParsedCommand _sut;

    public ParsedCommandTests()
    {
        _command = new(
            "main",
            "description",
            [],
            [
                new ArgumentDescriptor<string>("arg1"),
                new ArgumentDescriptor<string>("arg2", repeated: true),
            ],
            [
                new OptionDescriptor(
                    "--opt1",
                    "description",
                    shortName: 'a',
                    argument: new ArgumentDescriptor<string>("opt1arg")
                ),
                new OptionDescriptor(
                    "--opt2",
                    "description",
                    shortName: 'b',
                    argument: new ArgumentDescriptor<string>("opt2arg", repeated: true)
                ),
                new OptionDescriptor(
                    "--opt3",
                    "description",
                    shortName: 'c',
                    argument: new ArgumentDescriptor<string>("opt3arg")
                ),
                new OptionDescriptor(
                    "--opt4",
                    "description",
                    shortName: 'd',
                    argument: new ArgumentDescriptor<string>("opt4arg", repeated: true)
                ),
                new OptionDescriptor(
                    "--flag1",
                    "description",
                    shortName: 'f'
                ),
                new OptionDescriptor(
                    "--flag2",
                    "description",
                    shortName: 'e'
                ),
            ],
            (_) => { }
        );

        _parsedArguments = [];
        _parsedArguments[_command.Arguments[0]] = ["argfoo"];
        _parsedArguments[_command.Arguments[1]] = ["argbar", "argbaz"];

        _parsedFlags = [_command.Options[^2]];

        _parsedOptions = [];
        _parsedOptions[_command.Options[0]] = ["optfoo"];
        _parsedOptions[_command.Options[1]] = ["optbar", "optbaz"];

        _sut = new(_command, _parsedArguments, _parsedFlags, _parsedOptions);
    }

    [Fact]
    public void GetRequiredOptionValueByFullName_ReturnsOptionValue_WhenOptionWasParsed()
    {
        // Arrange
        var fullName = "--opt1";

        // Act
        var value = _sut.GetRequiredOptionValue<string>(fullName);

        // Assert
        value.Should().Be("optfoo");
    }

    [Fact]
    public void GetRequiredOptionValueByFullName_ThrowsOptionNotFoundException_WhenOptionWasNotFound()
    {
        // Arrange
        var fullName = "--foo";
        var action = () => _sut.GetRequiredOptionValue<string>(fullName);

        // Act, Assert
        action.Should().Throw<OptionNotFoundException>();
    }

    [Fact]
    public void GetRequiredOptionValueByFullName_ThrowsParsedOptionNotFoundException_WhenOptionWasNotParsed()
    {
        // Arrange
        var fullName = "--opt3";
        var action = () => _sut.GetRequiredOptionValue<string>(fullName);

        // Act, Assert
        action.Should().Throw<ParsedOptionNotFoundException>();
    }

    [Fact]
    public void GetRequiredOptionValueByFullName_ThrowsTypeConversionFailedException_WhenTypeIsWrong()
    {
        // Arrange
        var fullName = "--opt1";
        var action = () => _sut.GetRequiredOptionValue<int>(fullName);

        // Act, Assert
        action.Should().Throw<TypeConversionFailedException>();
    }

    [Fact]
    public void GetRequiredOptionValueByShortName_ReturnsOptionValue_WhenOptionWasParsed()
    {
        // Arrange
        var shortName = 'a';

        // Act
        var value = _sut.GetRequiredOptionValue<string>(shortName);

        // Assert
        value.Should().Be("optfoo");
    }

    [Fact]
    public void GetRequiredOptionValueByShortName_ThrowsOptionNotFoundException_WhenOptionWasNotFound()
    {
        // Arrange
        var shortName = 'x';
        var action = () => _sut.GetRequiredOptionValue<string>(shortName);

        // Act, Assert
        action.Should().Throw<OptionNotFoundException>();
    }

    [Fact]
    public void GetRequiredOptionValueByShortName_ThrowsParsedOptionNotFoundException_WhenOptionWasNotParsed()
    {
        // Arrange
        var shortName = 'c';
        var action = () => _sut.GetRequiredOptionValue<string>(shortName);

        // Act, Assert
        action.Should().Throw<ParsedOptionNotFoundException>();
    }

    [Fact]
    public void GetRequiredOptionValueByShortName_ThrowsTypeConversionFailedException_WhenTypeIsWrong()
    {
        // Arrange
        var shortName = 'a';
        var action = () => _sut.GetRequiredOptionValue<int>(shortName);

        // Act, Assert
        action.Should().Throw<TypeConversionFailedException>();
    }

    [Fact]
    public void GetRequiredOptionValuesByFullName_ReturnsOptionValue_WhenOptionWasParsed()
    {
        // Arrange
        var fullName = "--opt2";

        // Act
        var values = _sut.GetRequiredOptionValues<string>(fullName);

        // Assert
        values.Should().Equal(["optbar", "optbaz"]);
    }

    [Fact]
    public void GetRequiredOptionValuesByFullName_ThrowsOptionNotFoundException_WhenOptionWasNotFound()
    {
        // Arrange
        var fullName = "--foo";
        var action = () => _sut.GetRequiredOptionValues<string>(fullName);

        // Act, Assert
        action.Should().Throw<OptionNotFoundException>();
    }

    [Fact]
    public void GetRequiredOptionValuesByFullName_ThrowsParsedOptionNotFoundException_WhenOptionWasNotParsed()
    {
        // Arrange
        var fullName = "--opt4";
        var action = () => _sut.GetRequiredOptionValues<string>(fullName);

        // Act, Assert
        action.Should().Throw<ParsedOptionNotFoundException>();
    }

    [Fact]
    public void GetRequiredOptionValuesByFullName_ThrowsTypeConversionFailedException_WhenTypeIsWrong()
    {
        // Arrange
        var fullName = "--opt2";
        var action = () => _sut.GetRequiredOptionValues<int>(fullName);

        // Act, Assert
        action.Should().Throw<TypeConversionFailedException>();
    }

    [Fact]
    public void GetRequiredOptionValuesByShortName_ReturnsOptionValue_WhenOptionWasParsed()
    {
        // Arrange
        var shortName = 'b';

        // Act
        var values = _sut.GetRequiredOptionValues<string>(shortName);

        // Assert
        values.Should().Equal(["optbar", "optbaz"]);
    }

    [Fact]
    public void GetRequiredOptionValuesByShortName_ThrowsOptionNotFoundException_WhenOptionWasNotFound()
    {
        // Arrange
        var shortName = 'x';
        var action = () => _sut.GetRequiredOptionValues<string>(shortName);

        // Act, Assert
        action.Should().Throw<OptionNotFoundException>();
    }

    [Fact]
    public void GetRequiredOptionValuesByShortName_ThrowsParsedOptionNotFoundException_WhenOptionWasNotParsed()
    {
        // Arrange
        var shortName = 'd';
        var action = () => _sut.GetRequiredOptionValues<string>(shortName);

        // Act, Assert
        action.Should().Throw<ParsedOptionNotFoundException>();
    }

    [Fact]
    public void GetRequiredOptionValuesByShortName_ThrowsTypeConversionFailedException_WhenTypeIsWrong()
    {
        // Arrange
        var shortName = 'b';
        var action = () => _sut.GetRequiredOptionValues<int>(shortName);

        // Act, Assert
        action.Should().Throw<TypeConversionFailedException>();
    }

    [Fact]
    public void TryGetOptionValueByFullName_ReturnsOptionValue_WhenOptionWasParsed()
    {
        // Arrange
        var fullName = "--opt1";

        // Act
        var retrieved = _sut.TryGetOptionValue<string>(fullName, out var value);

        // Assert
        retrieved.Should().BeTrue();
        value.Should().Be("optfoo");
    }

    [Fact]
    public void TryGetOptionValueByFullName_ThrowsOptionNotFoundException_WhenOptionWasNotFound()
    {
        // Arrange
        var fullName = "--foo";
        var action = () => _sut.TryGetOptionValue<string>(fullName, out _);

        // Act, Assert
        action.Should().Throw<OptionNotFoundException>();
    }

    [Fact]
    public void TryGetOptionValueByFullName_ReturnsFalse_WhenOptionWasNotParsed()
    {
        // Arrange
        var fullName = "--opt3";

        // Act
        var retrieved = _sut.TryGetOptionValue<string>(fullName, out _);

        // Assert
        retrieved.Should().BeFalse();
    }

    [Fact]
    public void TryGetOptionValueByFullName_ThrowsTypeConversionFailedException_WhenTypeIsWrong()
    {
        // Arrange
        var fullName = "--opt1";
        var action = () => _sut.TryGetOptionValue<int>(fullName, out _);

        // Act, Assert
        action.Should().Throw<TypeConversionFailedException>();
    }

    [Fact]
    public void TryGetOptionValueByShortName_ReturnsOptionValue_WhenOptionWasParsed()
    {
        // Arrange
        var shortName = 'a';

        // Act
        var retrieved = _sut.TryGetOptionValue<string>(shortName, out var value);

        // Assert
        retrieved.Should().BeTrue();
        value.Should().Be("optfoo");
    }

    [Fact]
    public void TryGetOptionValueByShortName_ThrowsOptionNotFoundException_WhenOptionWasNotFound()
    {
        // Arrange
        var shortName = 'x';
        var action = () => _sut.TryGetOptionValue<string>(shortName, out _);

        // Act, Assert
        action.Should().Throw<OptionNotFoundException>();
    }

    [Fact]
    public void TryGetOptionValueByShortName_ReturnsFalse_WhenOptionWasNotParsed()
    {
        // Arrange
        var shortName = 'c';

        // Act
        var retrieved = _sut.TryGetOptionValue<string>(shortName, out _);

        // Assert
        retrieved.Should().BeFalse();
    }

    [Fact]
    public void TryGetOptionValueByShortName_ThrowsTypeConversionFailedException_WhenTypeIsWrong()
    {
        // Arrange
        var shortName = 'a';
        var action = () => _sut.TryGetOptionValue<int>(shortName, out _);

        // Act, Assert
        action.Should().Throw<TypeConversionFailedException>();
    }

    [Fact]
    public void TryGetOptionValuesByFullName_ReturnsOptionValue_WhenOptionWasParsed()
    {
        // Arrange
        var fullName = "--opt2";

        // Act
        var retrieved = _sut.TryGetOptionValues<string>(fullName, out var values);

        // Assert
        retrieved.Should().BeTrue();
        values.Should().Equal(["optbar", "optbaz"]);
    }

    [Fact]
    public void TryGetOptionValuesByFullName_ThrowsOptionNotFoundException_WhenOptionWasNotFound()
    {
        // Arrange
        var fullName = "--foo";
        var action = () => _sut.TryGetOptionValues<string>(fullName, out _);

        // Act, Assert
        action.Should().Throw<OptionNotFoundException>();
    }

    [Fact]
    public void TryGetOptionValuesByFullName_ReturnsFalse_WhenOptionWasNotParsed()
    {
        // Arrange
        var fullName = "--opt4";

        // Act
        var retrieved = _sut.TryGetOptionValues<string>(fullName, out _);

        // Assert
        retrieved.Should().BeFalse();
    }

    [Fact]
    public void TryGetOptionValuesByFullName_ThrowsTypeConversionFailedException_WhenTypeIsWrong()
    {
        // Arrange
        var fullName = "--opt2";
        var action = () => _sut.TryGetOptionValues<int>(fullName, out _);

        // Act, Assert
        action.Should().Throw<TypeConversionFailedException>();
    }

    [Fact]
    public void TryGetOptionValuesByShortName_ReturnsOptionValue_WhenOptionWasParsed()
    {
        // Arrange
        var shortName = 'b';

        // Act
        var retrieved = _sut.TryGetOptionValues<string>(shortName, out var values);

        // Assert
        retrieved.Should().BeTrue();
        values.Should().Equal(["optbar", "optbaz"]);
    }

    [Fact]
    public void TryGetOptionValuesByShortName_ThrowsOptionNotFoundException_WhenOptionWasNotFound()
    {
        // Arrange
        var shortName = 'x';
        var action = () => _sut.TryGetOptionValues<string>(shortName, out _);

        // Act, Assert
        action.Should().Throw<OptionNotFoundException>();
    }

    [Fact]
    public void TryGetOptionValuesByShortName_ReturnsFalse_WhenOptionWasNotParsed()
    {
        // Arrange
        var shortName = 'd';

        // Act
        var retrieved = _sut.TryGetOptionValues<string>(shortName, out _);

        // Assert
        retrieved.Should().BeFalse();
    }

    [Fact]
    public void TryGetOptionValuesByShortName_ThrowsTypeConversionFailedException_WhenTypeIsWrong()
    {
        // Arrange
        var shortName = 'b';
        var action = () => _sut.TryGetOptionValues<int>(shortName, out _);

        // Act, Assert
        action.Should().Throw<TypeConversionFailedException>();
    }

    [Fact]
    public void GetFlagByFullName_ReturnsTrue_WhenFlagWasParsed()
    {
        // Arrange
        var fullName = "--flag1";

        // Act
        var flag = _sut.GetFlag(fullName);

        // Assert
        flag.Should().BeTrue();
    }

    [Fact]
    public void GetFlagByFullName_ReturnsFalse_WhenFlagWasNotParsed()
    {
        // Arrange
        var fullName = "--flag2";

        // Act
        var flag = _sut.GetFlag(fullName);

        // Assert
        flag.Should().BeFalse();
    }

    [Fact]
    public void GetFlagByFullName_ThrowsOptionNotFoundException_WhenFlagWasNotFound()
    {
        // Arrange
        var fullName = "--foo";
        var action = () => _sut.GetFlag(fullName);

        // Act, Assert
        action.Should().Throw<OptionNotFoundException>();
    }

    [Fact]
    public void GetFlagByShortName_ReturnsTrue_WhenFlagWasParsed()
    {
        // Arrange
        var shortName = 'f';

        // Act
        var flag = _sut.GetFlag(shortName);

        // Assert
        flag.Should().BeTrue();
    }

    [Fact]
    public void GetFlagByShortName_ReturnsFalse_WhenFlagWasNotParsed()
    {
        // Arrange
        var shortName = 'e';

        // Act
        var flag = _sut.GetFlag(shortName);

        // Assert
        flag.Should().BeFalse();
    }

    [Fact]
    public void GetFlagByShortName_ThrowsOptionNotFoundException_WhenFlagWasNotFound()
    {
        // Arrange
        var shortName = 'x';
        var action = () => _sut.GetFlag(shortName);

        // Act, Assert
        action.Should().Throw<OptionNotFoundException>();
    }

    [Fact]
    public void GetPositionalArgumentValue_ReturnsArgumentValue_WhenArgumentWasParsed()
    {
        // Arrange
        var index = 0;

        // Act
        var value = _sut.GetPositionalArgumentValue<string>(index);

        // Assert
        value.Should().Be("argfoo");
    }

    [Fact]
    public void GetPositionalArgumentValue_ThrowsPositionalArgumentNotFoundException_WhenIndexIsOutOfRange()
    {
        // Arrange
        var index = 2;
        var action = () => _sut.GetPositionalArgumentValue<string>(index);

        // Act, Assert
        action.Should().Throw<PositionalArgumentNotFoundException>();
    }

    [Fact]
    public void GetPositionalArgumentValue_ThrowsTypeConversionFailedException_WhenTypeIsWrong()
    {
        // Arrange
        var index = 0;
        var action = () => _sut.GetPositionalArgumentValue<int>(index);

        // Act, Assert
        action.Should().Throw<TypeConversionFailedException>();
    }

    [Fact]
    public void GetPositionalArgumentValues_ReturnsArgumentValue_WhenArgumentWasParsed()
    {
        // Arrange
        var index = 1;

        // Act
        var value = _sut.GetPositionalArgumentValues<string>(index);

        // Assert
        value.Should().Equal(["argbar", "argbaz"]);
    }

    [Fact]
    public void GetPositionalArgumentValues_ThrowsPositionalArgumentNotFoundException_WhenIndexIsOutOfRange()
    {
        // Arrange
        var index = 2;
        var action = () => _sut.GetPositionalArgumentValues<string>(index);

        // Act, Assert
        action.Should().Throw<PositionalArgumentNotFoundException>();
    }

    [Fact]
    public void GetPositionalArgumentValues_ThrowsTypeConversionFailedException_WhenTypeIsWrong()
    {
        // Arrange
        var index = 1;
        var action = () => _sut.GetPositionalArgumentValues<int>(index);

        // Act, Assert
        action.Should().Throw<TypeConversionFailedException>();
    }
}