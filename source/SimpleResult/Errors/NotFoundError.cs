using SimpleResult.Errors;

namespace Common.Results;

public record NotFoundError(string Code, string Message) : Error(Code, Message);

