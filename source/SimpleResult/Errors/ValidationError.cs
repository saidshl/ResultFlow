using SimpleResult.Errors;

namespace Common.Results;

public record ValidationError(string Code, string Message) : Error(Code, Message);

