using SimpleResult.Errors;

namespace Common.Results;

public record ConflictError(string Code, string Message) : Error(Code, Message);

