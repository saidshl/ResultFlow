using SimpleResult.Errors;

namespace Common.Results;

public record BadRequestError(string Code, string Message) : Error(Code, Message);

