using SimpleResult.Errors;

namespace Common.Results;

public record UnauthorizedError(string Code, string Message) : Error(Code, Message);

