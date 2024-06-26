using CommandLineParser.Models;
using CommandLineParser.Parsing;
using CommandLineParser.Parsing.Models;

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
        ], [], _ => { });
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
        ], [], _ => { });
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
        ], [], _ => { });
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
        ], [], _ => { });
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
        ], [], _ => { });
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
            new OptionDescriptor("--opt2", "description", new ArgumentDescriptor<string>("opt2arg")),
        ], _ => { });
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
            new OptionDescriptor("--opt3", "description", new ArgumentDescriptor<string>("opt3arg"), shortName: 'c'),
        ], _ => { });
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
        ], [], _ => { });
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
        ], [], _ => { });
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
        ], _ => { });
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
        ], _ => { });
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

    // NOTE: not sure if this is the indented behavior
    [Fact]
    public void Parse_StopsParsingArgumentWithUnlimitedValues_WhenEncountersSeparatorAndOptionsAreStillParsed()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: true),
            new ArgumentDescriptor<string>("arg2", repeated: false),
        ], [], _ => { });
        var args = new[] { "foo", "bar", "--", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[1]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[3]);
        _mockParsingResultBuilder.DidNotReceive().AddParsedArgumentValue(Arg.Any<IArgumentDescriptor>(), args[2]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesSeparatorAsArgumentForArgumentWithUnlimitedValues_WhenSeparatorWasEncounteredAlready()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: false),
            new ArgumentDescriptor<string>("arg2", repeated: true),
        ], [], _ => { });
        var args = new[] { "foo", "--", "bar", "--" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[0]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[2]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[3]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesShortAndFullNameOptionsAsValuesForArguments_WhenSeparatorIsEncounteredBefore()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: false),
            new ArgumentDescriptor<string>("arg2", repeated: false),
            new ArgumentDescriptor<string>("arg3", repeated: false),
            new ArgumentDescriptor<string>("arg4", repeated: false),
            new ArgumentDescriptor<string>("arg5", repeated: false),
        ], [
            new OptionDescriptor("--opt1", "description"),
            new OptionDescriptor("--opt2", "description", shortName: 'a'),
        ], _ => { });
        var args = new[] { "--", "foo", "bar", "--opt1", "baz", "-a" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[1]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), args[2]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[2].Name), args[3]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[3].Name), args[4]);
        _mockParsingResultBuilder.Received(1).AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[4].Name), args[5]);
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
        ], _ => { });
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

    [Fact]
    public void Parse_ParsesFullNameOptionsAsFlags_WhenTheyDoNotHaveArguments()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description"),
            new OptionDescriptor("--opt2", "description"),
            new OptionDescriptor("--opt3", "description"),
        ], _ => { });
        var args = new[] { "--opt1", "--opt3", "--opt2" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[0].FullName));
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[1].FullName));
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[2].FullName));
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesFullNameOptions_WhenTheyHaveArguments()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<string>("opt1arg")),
            new OptionDescriptor("--opt2", "description", argument: new ArgumentDescriptor<string>("opt2arg")),
        ], _ => { });
        var args = new[] { "--opt1", "foo", "--opt2", "bar" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[1]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[1].Argument!.Name), args[3]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesFullNameOption_WhenItHasArgumentWithUnlimitedValues()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<string>("opt1arg", repeated: true)),
        ], _ => { });
        var args = new[] { "--opt1", "foo", "bar", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[1]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[2]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[3]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_StopsParsingOptionWithArgumentWithUnlimitedValues_WhenEncountersFullNameOptionAndOptionsAreStillParsed()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<string>("opt1arg", repeated: true)),
            new OptionDescriptor("--opt2", "description"),
        ], _ => { });
        var args = new[] { "--opt1", "foo", "bar", "--opt2" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[1]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[2]);
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[1].FullName));
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_StopsParsingOptionWithArgumentWithUnlimitedValues_WhenEncountersShortNameOptionAndOptionsAreStillParsed()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<string>("opt1arg", repeated: true)),
            new OptionDescriptor("--opt2", "description", shortName: 'a'),
        ], _ => { });
        var args = new[] { "--opt1", "foo", "bar", "-a" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[1]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[2]);
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[1].FullName));
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_StopsParsingOptionWithArgumentWithUnlimitedValues_WhenEncountersSeparatorAndOptionsAreStillParsed()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: true),
        ],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<string>("opt1arg", repeated: true)),
        ], _ => { });
        var args = new[] { "--opt1", "foo", "bar", "--", "--opt1" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[1]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[2]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[4]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesFullNameOption_WhenArgumentIsSeparated()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<string>("opt1arg")),
        ], _ => { });
        var args = new[] { "--opt1=foo" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), "foo");
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesOnlyFirstValueOfTheOption_WhenOptionHasArgumentWithUnlimitedOptionsAndIsSeparated()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1"),
            new ArgumentDescriptor<string>("arg2"),
        ],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<string>("opt1arg", repeated: true)),
        ], _ => { });
        var args = new[] { "--opt1=foo", "bar", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), "foo");
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), "bar");
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), "baz");
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesOptionWithArgumentWithUnlimitedValues_WhenArgumentIsSeparatedButAppearsMultipleTimes()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<string>("opt1arg", repeated: true)),
        ], _ => { });
        var args = new[] { "--opt1=foo", "--opt1=bar", "--opt1=baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), "foo");
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), "bar");
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), "baz");
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesShortNameOptionsAsFlags_WhenTheyDoNotHaveArguments()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a'),
            new OptionDescriptor("--opt2", "description", shortName: 'b'),
            new OptionDescriptor("--opt3", "description", shortName: 'c'),
        ], _ => { });
        var args = new[] { "-b", "-a", "-c" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[0].FullName));
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[1].FullName));
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[2].FullName));
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesShortNameOptions_WhenTheyAreGrouped()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a'),
            new OptionDescriptor("--opt2", "description", shortName: 'b'),
            new OptionDescriptor("--opt3", "description", shortName: 'c'),
        ], _ => { });
        var args = new[] { "-acb" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[0].FullName));
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[1].FullName));
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[2].FullName));
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesGroupedShortNameOptions_WhenTheLastOneHasArgument()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a'),
            new OptionDescriptor("--opt2", "description", shortName: 'b', argument: new ArgumentDescriptor<string>("opt2arg")),
            new OptionDescriptor("--opt3", "description", shortName: 'c'),
        ], _ => { });
        var args = new[] { "-acbfoo" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[0].FullName));
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[1].Argument!.Name), "foo");
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[2].FullName));
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_IgnoresShortOptions_WhenOneOfThemHasArgument()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a'),
            new OptionDescriptor("--opt2", "description", shortName: 'b', argument: new ArgumentDescriptor<string>("opt2arg")),
            new OptionDescriptor("--opt3", "description", shortName: 'c'),
        ], _ => { });
        var args = new[] { "-abcfoo" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[0].FullName));
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[1].Argument!.Name), "cfoo");
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesGroupedShortNameOptions_WhenArgumentIsASeperateArg()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a'),
            new OptionDescriptor("--opt2", "description", shortName: 'b', argument: new ArgumentDescriptor<string>("opt2arg")),
            new OptionDescriptor("--opt3", "description", shortName: 'c'),
        ], _ => { });
        var args = new[] { "-acb", "foo" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[0].FullName));
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[2].FullName));
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[1].Argument!.Name), args[1]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesShortNameOptions_WhenArgumentHasUnlimitedValues()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a', argument: new ArgumentDescriptor<string>("opt1arg", repeated: true)),
        ], _ => { });
        var args = new[] { "-a", "foo", "bar", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[1]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[2]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[3]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_StopsParsingShortNameOptionWithArgumentWithUnlimitedValues_WhenEncountersShortNameOptionAndOptionsAreStillParsed()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a', argument: new ArgumentDescriptor<string>("opt1arg", repeated: true)),
            new OptionDescriptor("--opt2", "description", shortName: 'b'),
        ], _ => { });
        var args = new[] { "-a", "foo", "bar", "-b" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[1]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[2]);
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[1].FullName));
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_StopsParsingShortNameOptionWithArgumentWithUnlimitedValues_WhenEncountersFullNameOptionAndOptionsAreStillParsed()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a', argument: new ArgumentDescriptor<string>("opt1arg", repeated: true)),
            new OptionDescriptor("--opt2", "description"),
        ], _ => { });
        var args = new[] { "-a", "foo", "bar", "--opt2" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[1]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[2]);
        _mockParsingResultBuilder.Received(1).AddFlag(Arg.Is<OptionDescriptor>(o => o.FullName == rootCommand.Options[1].FullName));
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_StopsParsingShortOptionWithArgumentWithUnlimitedValues_WhenEncountersSeparatorAndOptionsAreStillParsed()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1", repeated: true),
        ],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a', argument: new ArgumentDescriptor<string>("opt1arg", repeated: true)),
        ], _ => { });
        var args = new[] { "-a", "foo", "bar", "--", "--opt1" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[1]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[2]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), args[4]);
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesOnlyFirstValueOfTheShortOption_WhenOptionHasArgumentWithUnlimitedOptionsAndIsGrouped()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1"),
            new ArgumentDescriptor<string>("arg2"),
        ],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a', argument: new ArgumentDescriptor<string>("opt1arg", repeated: true)),
            new OptionDescriptor("--opt2", "description", shortName: 'b'),
        ], _ => { });
        var args = new[] { "-afoo", "bar", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), "foo");
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[0].Name), "bar");
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Arguments[1].Name), "baz");
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesShortOptionWithArgumentWithUnlimitedValues_WhenArgumentIsGroupedButAppearsMultipleTimes()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a', argument: new ArgumentDescriptor<string>("opt1arg", repeated: true)),
        ], _ => { });
        var args = new[] { "-afoo", "-abar", "-abaz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), "foo");
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), "bar");
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), "baz");
        _mockParsingResultBuilder.DidNotReceiveWithAnyArgs().AddError(default!);
    }

    [Fact]
    public void Parse_ParsesOptionValues_WhenTheyHaveOptionPrefix()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a', argument: new ArgumentDescriptor<string>("opt1arg")),
            new OptionDescriptor("--opt2", "description", shortName: 'b', argument: new ArgumentDescriptor<string>("opt2arg")),
        ], _ => { });
        var args = new[] { "--opt1", "--opt2", "-a", "-b", "--opt2", "-a", "-b", "--opt1" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[1]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[3]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[1].Argument!.Name), args[5]);
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[1].Argument!.Name), args[7]);
    }

    [Fact]
    public void Parse_ParsesRootCommand_WhenNoSubcommandsProvided()
    {
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1"),
        ], [], _ => { });
        var args = new[] { "arg1" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).SetCurrentCommand(Arg.Is<CommandDescriptor>(c => c.Name == rootCommand.Name));
    }

    [Fact]
    public void Parse_ParsesSubcommands_WhenTheyAreProvided()
    {
        var rootCommand = new CommandDescriptor("main", "description",
        [
            new CommandDescriptor("sub", "description",
            [
                new CommandDescriptor("subsub", "description", [], [], [], _ => { }),
            ], [], [], _ => { }),
        ], [], [], _ => { });
        var args = new[] { "sub", "subsub" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).SetCurrentCommand(Arg.Is<CommandDescriptor>(c => c.Name == rootCommand.Name));
        _mockParsingResultBuilder.Received(1)
            .SetCurrentCommand(Arg.Is<CommandDescriptor>(c => c.Name == rootCommand.Subcommands[0].Name));
        _mockParsingResultBuilder.Received(1)
            .SetCurrentCommand(Arg.Is<CommandDescriptor>(c => c.Name == rootCommand.Subcommands[0].Subcommands[0].Name));
    }

    [Fact]
    public void Parse_StopsParsingSubcommands_WhenEncounteredOptionsAndArguments()
    {
        var rootCommand = new CommandDescriptor("main", "description",
        [
            new CommandDescriptor("sub", "description", [], [], [], _ => { }),
        ], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<string>("opt1arg"))
        ], _ => { });
        var args = new[] { "--opt1", "sub" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1).SetCurrentCommand(Arg.Is<CommandDescriptor>(c => c.Name == rootCommand.Name));
        _mockParsingResultBuilder.Received(1)
            .AddParsedArgumentValue(Arg.Is<IArgumentDescriptor>(a => a.Name == rootCommand.Options[0].Argument!.Name), args[1]);
    }

    [Fact]
    public void Parse_AddsError_WhenEncountersUnexpectedArg()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<string>("arg1"),
            new ArgumentDescriptor<string>("arg2"),
        ], [], _ => { });
        var args = new[] { "foo", "bar", "baz" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddError(Arg.Is<ParsingError>(e => e.TokenValue == args[2] && e.Type == ParsingErrorType.UnknownToken));
    }

    [Fact]
    public void Parse_AddsError_WhenEncountersUnexpectedFullNameOption()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [], [], _ => { });
        var args = new[] { "--foo" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddError(Arg.Is<ParsingError>(e => e.TokenValue == args[0] && e.Type == ParsingErrorType.UnknownOption));
    }

    [Fact]
    public void Parse_AddsError_WhenEncountersUnexpectedFullNameOptionWithValue()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [], [], _ => { });
        var args = new[] { "--foo=bar" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddError(Arg.Is<ParsingError>(e => e.TokenValue == args[0] && e.Type == ParsingErrorType.UnknownOption));
    }

    [Fact]
    public void Parse_AddsError_WhenEncountersUnexpectedShortNameOption()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [], [], _ => { });
        var args = new[] { "-a" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddError(Arg.Is<ParsingError>(e => e.TokenValue == "a" && e.Type == ParsingErrorType.UnknownOption));
    }

    [Fact]
    public void Parse_AddsError_WhenEncountersUnexpectedShortNameOptionInGroup()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", shortName: 'a'),
        ], _ => { });
        var args = new[] { "-ab" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddError(Arg.Is<ParsingError>(e => e.TokenValue == "b" && e.Type == ParsingErrorType.UnknownOption));
    }

    [Fact]
    public void Parse_AddsError_WhenValueForFullNameOptionIsNotProvided()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<string>("opt1arg")),
        ], _ => { });
        var args = new[] { "--opt1" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddError(Arg.Is<ParsingError>(e => e.TokenName == rootCommand.Options[0].Argument!.Name && e.Type == ParsingErrorType.MissingArgumentValue));
    }

    [Fact]
    public void Parse_AddsError_WhenValueForShortNameOptionIsNotProvided()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<string>("opt1arg"), shortName: 'a'),
        ], _ => { });
        var args = new[] { "-a" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddError(Arg.Is<ParsingError>(e => e.TokenName == rootCommand.Options[0].Argument!.Name && e.Type == ParsingErrorType.MissingArgumentValue));
    }

    [Fact]
    public void Parse_AddsError_WhenFullNameOptionDoesNotHaveArgumentButItIsProvided()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description"),
        ], _ => { });
        var args = new[] { "--opt1=foo" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddError(Arg.Is<ParsingError>(e => e.TokenName == rootCommand.Options[0].FullName && e.Type == ParsingErrorType.ArgumentValueForFlag));
    }

    [Fact]
    public void Parse_AddsError_WhenArgumentIsNotConverted()
    {
        // Arragne
        var rootCommand = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<int>("arg1")
        ], [], _ => { });
        var args = new[] { "foo" };
        var sut = new Parser(_mockParsingResultBuilder, rootCommand);

        // Act
        sut.Parse(args);

        // Assert
        _mockParsingResultBuilder.Received(1)
            .AddError(Arg.Is<ParsingError>(e => e.TokenName == rootCommand.Arguments[0].Name && e.Type == ParsingErrorType.ConversionFailed));
    }
}