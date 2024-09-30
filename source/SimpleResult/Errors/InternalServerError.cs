using SimpleResult.Errors;

namespace Common.Results;

public record InternalServerError(string Code, string Message) : Error(Code, Message);

