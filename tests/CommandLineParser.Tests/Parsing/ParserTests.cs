using CommandLineParser.Models;
using CommandLineParser.Parsing;

using NSubstitute;

namespace CommandLineParser.Tests.Parsing;

public class ParserTests
{
    private readonly IParsingResultBuilder _mockParsingResultBuilder;

    public ParserTests()
    {
        _mockParsingResultBuilder = Substitute.For<IParsingResultBuilder>();
    }

    [Fact]
    public void Parse_ParsesArgumentsWithOneValue_WhenOneValuePerArgumentIsProvided()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1"),
            new ArgumentDescriptor<string>("arg2"),
            new ArgumentDescriptor<string>("arg3"),
        ], []);
        var args = new[] { "foo", "bar", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[1]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[2].Name), args[2]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddFlag(default!);
    }

    [Fact]
    public void Parse_ParsesArgumentWithUnlimitedValues_WhenOneValueIsProvided()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: true),
        ], []);
        var args = new[] { "foo" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddFlag(default!);
    }

    [Fact]
    public void Parse_ParsesArgumentWithUnlimitedValues_WhenMultipleValuesAreProvided()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: true),
        ], []);
        var args = new[] { "foo", "bar", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[1]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[2]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddFlag(default!);
    }

    [Fact]
    public void Parse_ParsesArgumentWithUnlimitedValues_WhenItIsLastAndMultipleValuesAreProvided()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: false),
            new ArgumentDescriptor<string>("arg2", repeated: true),
        ], []);
        var args = new[] { "foo", "bar", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[1]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[2]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddFlag(default!);
    }

    [Fact]
    public void Parse_IgnoresArgument_WhenItComesAfterAnotherArgumentWithUnlimitedValues()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: true),
            new ArgumentDescriptor<string>("arg2", repeated: false),
        ], []);
        var args = new[] { "foo", "bar", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[1]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[2]);
        _mockParsingResultBuilder.DidNotReceive().AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), Arg.Any<object>());
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddFlag(default!);
    }

    [Fact]
    public void Parse_ParsesAllArguments_WhenFullNameOptionsArePresentBetweenArguments()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: false),
            new ArgumentDescriptor<string>("arg2", repeated: false),
            new ArgumentDescriptor<string>("arg3", repeated: false),
        ], [
            new OptionDescriptor("--opt1", "description"),
            new OptionDescriptor("--opt2", "description", new ArgumentDescriptor<string>("opt2arg1")),
        ]);
        var args = new[] { "foo", "--opt1", "bar", "--opt2", "value", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[2]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[2].Name), args[5]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesAllArguments_WhenShortNameOptionsArePresentBetweenArguments()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: false),
            new ArgumentDescriptor<string>("arg2", repeated: false),
            new ArgumentDescriptor<string>("arg3", repeated: false),
        ], [
            new OptionDescriptor("--opt1", "description", shortName: 'a'),
            new OptionDescriptor("--opt2", "description", shortName: 'b'),
            new OptionDescriptor("--opt3", "description", new ArgumentDescriptor<string>("opt3arg1"), shortName: 'c'),
        ]);
        var args = new[] { "foo", "-a", "bar", "-bc", "value", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[2]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[2].Name), args[5]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_IgnoresSeparator_WhenSeparatorEncounteredForTheFirstTime()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: false),
            new ArgumentDescriptor<string>("arg2", repeated: false),
        ], []);
        var args = new[] { "foo", "--", "bar" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[2]);
        _mockParsingResultBuilder.DidNotReceive().AddParsedArgumentValue(Arg.Any<IArgumentDescriptor>(), args[1]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesSeparatorAsArgument_WhenSeparatorWasEncounteredAlready()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: false),
            new ArgumentDescriptor<string>("arg2", repeated: false),
            new ArgumentDescriptor<string>("arg3", repeated: false),
        ], []);
        var args = new[] { "foo", "--", "bar", "--" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[2]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[2].Name), args[3]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_StopsParsingArgumentWithUnlimitedValues_WhenEncountersFullNameOptionAndOptionsAreStillParsed()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: true),
        ], [
            new OptionDescriptor("--opt1", "description"),
        ]);
        var args = new[] { "foo", "bar", "baz", "--opt1" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[1]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[2]);
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[0].FullName));
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_StopsParsingArgumentWithUnlimitedValues_WhenEncountersShortNameOptionAndOptionsAreStillParsed()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: true),
        ], [
            new OptionDescriptor("--opt1", "description", shortName: 'a'),
        ]);
        var args = new[] { "foo", "bar", "baz", "-a" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[1]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[2]);
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[0].FullName));
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesShortAndFullNameOptionsAsValuesForArgumentsWithUnlimitedValues_WhenSeparatorIsEncounteredBefore()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: true),
        ], [
            new OptionDescriptor("--opt1", "description"),
            new OptionDescriptor("--opt2", "description", shortName: 'a'),
        ]);
        var args = new[] { "--", "foo", "bar", "--opt1", "baz", "-a" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[1]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[2]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[3]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[4]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[5]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }
}