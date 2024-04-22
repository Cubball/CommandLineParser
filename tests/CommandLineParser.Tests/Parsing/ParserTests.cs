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
    public void Parse_ParsesArgumentsWithOneValue_WhenOnlyArgumentsArePresent()
    {
        // Arragne
        var (rootCommand, args) = GetCommandWith3ArgumentsOnly();
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[1]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[2].Name), args[2]);
    }

    [Fact]
    public void Parse_DoesNotParseAnythingBesidesArgumentes_WhenOnlyArgumentsArePresent()
    {
        // Arragne
        var (rootCommand, args) = GetCommandWith3ArgumentsOnly();
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddFlag(default!);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddParsedOptionValue(default!, default!);
    }

    private static (CommandDescriptor Command, string[] Args) GetCommandWith3ArgumentsOnly()
    {
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", index: 0),
            new ArgumentDescriptor<string>("arg2", index: 1),
            new ArgumentDescriptor<string>("arg3", index: 2),
        ], []);
        var args = new[] { "foo", "bar", "baz" };
        return (rootCommand, args);
    }
}