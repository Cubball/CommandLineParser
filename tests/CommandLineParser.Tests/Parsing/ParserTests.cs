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
            new ArgumentDescriptor<string>("arg1", index: 0),
            new ArgumentDescriptor<string>("arg2", index: 1),
            new ArgumentDescriptor<string>("arg3", index: 2),
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
            new ArgumentDescriptor<string>("arg1", index: 0, repeated: true),
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
            new ArgumentDescriptor<string>("arg1", index: 0, repeated: true),
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
            new ArgumentDescriptor<string>("arg1", index: 0, repeated: false),
            new ArgumentDescriptor<string>("arg2", index: 1, repeated: true),
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
            new ArgumentDescriptor<string>("arg1", index: 0, repeated: true),
            new ArgumentDescriptor<string>("arg2", index: 1, repeated: false),
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
    public void Parse_StopsParsingArgumentWithUnlimitedValues_WhenSeparatorIsPresent()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", index: 0, repeated: true),
            new ArgumentDescriptor<string>("arg2", index: 1, repeated: false),
        ], []);
        // TODO: extract '--' and such into a settings class
        var args = new[] { "foo", "bar", "--", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[1]);
        _mockParsingResultBuilder.DidNotReceive().AddParsedArgumentValue(Arg.Any<IArgumentDescriptor>(), Arg.Is<object>(args[2]));
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[3]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddFlag(default!);
    }

    [Fact]
    public void Parse_ParsesAllArguments_WhenFullNameOptionsArePresentBetweenArguments()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", index: 0, repeated: false),
            new ArgumentDescriptor<string>("arg2", index: 1, repeated: false),
            new ArgumentDescriptor<string>("arg3", index: 2, repeated: false),
        ], [
            new OptionDescriptor("--opt1", "description", []),
            new OptionDescriptor("--opt2", "description", [new ArgumentDescriptor<string>("opt2arg1")]),
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
}