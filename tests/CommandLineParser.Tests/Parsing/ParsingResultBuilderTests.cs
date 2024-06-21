using CommandLineParser.Models;
using CommandLineParser.Parsing;
using CommandLineParser.Parsing.Models;

using FluentAssertions;

namespace CommandLineParser.Tests.Parsing;

public class ParsingResultBuilderTests
{
    private readonly ParsingResultBuilder _sut = new();

    [Fact]
    public void Build_ShouldReturnSuccessContext_WhenOnlyCommandHasBeenSet()
    {
        // Arrange
        var command = new CommandDescriptor("main", "description", [], [], []);
        _sut.SetCurrentCommand(command);

        // Act
        var result = _sut.Build();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var successContext = result.SuccessContext;
        successContext.Should().NotBeNull();
        successContext!.Command.Should().Be(command);
    }

    [Fact]
    public void Build_ShouldThrowInvalidOperationException_WhenCurrentCommandWasNotSet()
    {
        // Arrange
        var action = _sut.Build;

        // Act, Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("The current command needs to be set before performing this action");
    }

    [Fact]
    public void Build_ShouldReturnAddedFlags_WhenThereAreNoErrors()
    {
        // Arrange
        var command = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description"),
            new OptionDescriptor("--opt2", "description"),
        ]);
        _sut.SetCurrentCommand(command);

        // Act
        _sut.AddFlag(command.Options[0]);
        _sut.AddFlag(command.Options[1]);
        var result = _sut.Build();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var successContext = result.SuccessContext;
        successContext!.ParsedFlags.Should().HaveCount(2);
        successContext.ParsedFlags.Should().Contain(command.Options[0]);
        successContext.ParsedFlags.Should().Contain(command.Options[1]);
    }

    [Fact]
    public void Build_ShouldReturnAddedOptions_WhenThereAreNoErrors()
    {
        // Arrange
        var command = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<int>("opt1arg")),
            new OptionDescriptor("--opt2", "description", argument: new ArgumentDescriptor<string>("opt2arg")),
        ]);
        _sut.SetCurrentCommand(command);

        // Act
        _sut.AddParsedArgumentValue(command.Options[0].Argument!, 69);
        _sut.AddParsedArgumentValue(command.Options[1].Argument!, "foo");
        var result = _sut.Build();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var successContext = result.SuccessContext;
        successContext!.ParsedOptions.Should().Contain(kvp =>
            kvp.Key == command.Options[0]
            && kvp.Value.Count == 1
            && kvp.Value.Contains(69));
        successContext!.ParsedOptions.Should().Contain(kvp =>
            kvp.Key == command.Options[1]
            && kvp.Value.Count == 1
            && kvp.Value.Contains("foo"));
    }

    [Fact]
    public void Build_ShouldReturnAddedOption_WhenItHasMultipleValues()
    {
        // Arrange
        var command = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<int>("opt1arg", repeated: true))
        ]);
        _sut.SetCurrentCommand(command);

        // Act
        _sut.AddParsedArgumentValue(command.Options[0].Argument!, 69);
        _sut.AddParsedArgumentValue(command.Options[0].Argument!, 420);
        var result = _sut.Build();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var successContext = result.SuccessContext;
        successContext!.ParsedOptions.Should().Contain(kvp =>
            kvp.Key == command.Options[0]
            && kvp.Value.Count == 2
            && kvp.Value.Contains(69)
            && kvp.Value.Contains(420));
    }

    // NOTE: should this behavior be configurable?
    [Fact]
    public void Build_ShouldReturnAddedOptionsLastValue_WhenItHasOneValueButMultipleWereProvided()
    {
        // Arrange
        var command = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<int>("opt1arg"))
        ]);
        _sut.SetCurrentCommand(command);

        // Act
        _sut.AddParsedArgumentValue(command.Options[0].Argument!, 69);
        _sut.AddParsedArgumentValue(command.Options[0].Argument!, 420);
        var result = _sut.Build();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var successContext = result.SuccessContext;
        successContext!.ParsedOptions.Should().Contain(kvp =>
            kvp.Key == command.Options[0]
            && kvp.Value.Count == 1
            && kvp.Value.Contains(420));
    }

    [Fact]
    public void Build_ShouldReturnAddedPositionalArguments_WhenThereAreNoErrors()
    {
        // Arrange
        var command = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<int>("arg1"),
            new ArgumentDescriptor<string>("arg2"),
        ], []);
        _sut.SetCurrentCommand(command);

        // Act
        _sut.AddParsedArgumentValue(command.Arguments[0], 69);
        _sut.AddParsedArgumentValue(command.Arguments[1], "foo");
        var result = _sut.Build();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var successContext = result.SuccessContext;
        successContext!.ParsedPositionalArguments.Should().Contain(kvp =>
            kvp.Key == command.Arguments[0]
            && kvp.Value.Count == 1
            && kvp.Value.Contains(69));
        successContext!.ParsedPositionalArguments.Should().Contain(kvp =>
            kvp.Key == command.Arguments[1]
            && kvp.Value.Count == 1
            && kvp.Value.Contains("foo"));
    }

    [Fact]
    public void Build_ShouldReturnAddedPositionalArgument_WhenItHasMultipleValues()
    {
        // Arrange
        var command = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<int>("arg1", repeated: true),
        ], []);
        _sut.SetCurrentCommand(command);

        // Act
        _sut.AddParsedArgumentValue(command.Arguments[0], 69);
        _sut.AddParsedArgumentValue(command.Arguments[0], 420);
        var result = _sut.Build();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var successContext = result.SuccessContext;
        successContext!.ParsedPositionalArguments.Should().Contain(kvp =>
            kvp.Key == command.Arguments[0]
            && kvp.Value.Count == 2
            && kvp.Value.Contains(69)
            && kvp.Value.Contains(420));
    }

    // NOTE: should this behavior be configurable?
    [Fact]
    public void Build_ShouldReturnAddedPositionalArgumentsLastValue_WhenItHasOneValueButMultipleWereProvided()
    {
        // Arrange
        var command = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<int>("arg1"),
        ], []);
        _sut.SetCurrentCommand(command);

        // Act
        _sut.AddParsedArgumentValue(command.Arguments[0], 69);
        _sut.AddParsedArgumentValue(command.Arguments[0], 420);
        var result = _sut.Build();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var successContext = result.SuccessContext;
        successContext!.ParsedPositionalArguments.Should().Contain(kvp =>
            kvp.Key == command.Arguments[0]
            && kvp.Value.Count == 1
            && kvp.Value.Contains(420));
    }

    [Fact]
    public void Build_ShouldReturnFailureContext_WhenErrorsWereAdded()
    {
        // Arrange
        var command = new CommandDescriptor("main", "description", [], [], []);
        _sut.SetCurrentCommand(command);
        var error = new ParsingError(ParsingErrorType.UnknownToken);

        // Act
        _sut.AddError(error);
        var result = _sut.Build();

        // Assert
        result.IsSuccess.Should().BeFalse();
        var failureContext = result.FailureContext;
        failureContext.Should().NotBeNull();
        failureContext!.Command.Should().Be(command);
        failureContext.Errors.Should().HaveCount(1);
        failureContext.Errors[0].Should().Be(error);
    }

    [Fact]
    public void Build_ShouldReturnFailureContext_WhenPositionalArgumentsAreMissing()
    {
        // Arrange
        var command = new CommandDescriptor("main", "description", [],
        [
            new ArgumentDescriptor<int>("arg1"),
        ], []);
        _sut.SetCurrentCommand(command);

        // Act
        var result = _sut.Build();

        // Assert
        result.IsSuccess.Should().BeFalse();
        var failureContext = result.FailureContext;
        failureContext.Should().NotBeNull();
        failureContext!.Command.Should().Be(command);
        failureContext.Errors.Should().Satisfy(e =>
            e.Type == ParsingErrorType.MissingPositionalArgument && e.TokenName == command.Arguments[0].Name);
    }

    [Fact]
    public void Build_ShouldReturnFailureContext_WhenRequiredOptionsAreMissing()
    {
        // Arrange
        var command = new CommandDescriptor("main", "description", [], [],
        [
            new OptionDescriptor("--opt1", "description", argument: new ArgumentDescriptor<int>("arg1"), required: true)
        ]);
        _sut.SetCurrentCommand(command);

        // Act
        var result = _sut.Build();

        // Assert
        result.IsSuccess.Should().BeFalse();
        var failureContext = result.FailureContext;
        failureContext.Should().NotBeNull();
        failureContext!.Command.Should().Be(command);
        failureContext.Errors.Should().Satisfy(e =>
            e.Type == ParsingErrorType.MissingRequiredOption && e.TokenName == command.Options[0].FullName);
    }

    [Fact]
    public void AddParsedArgumentValue_ShouldThrowInvalidOperationException_WhenCurrentCommandWasNotSet()
    {
        // Arrange
        var argument = new ArgumentDescriptor<int>("arg1");
        var action = () => _sut.AddParsedArgumentValue(argument, 69);

        // Act, Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("The current command needs to be set before performing this action");
    }
}