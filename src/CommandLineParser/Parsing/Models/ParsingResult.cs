using System.Diagnostics.CodeAnalysis;

namespace CommandLineParser.Parsing.Models;

internal class ParsingResult
{
    public ParsingResult(ParsingSuccessContext successContext)
    {
        SuccessContext = successContext;
        IsSuccess = true;
    }

    public ParsingResult(ParsingFailureContext failureContext)
    {
        FailureContext = failureContext;
    }

    [MemberNotNullWhen(true, nameof(SuccessContext))]
    [MemberNotNullWhen(false, nameof(FailureContext))]
    public bool IsSuccess { get; }

    public ParsingSuccessContext? SuccessContext { get; }

    public ParsingFailureContext? FailureContext { get; }
}