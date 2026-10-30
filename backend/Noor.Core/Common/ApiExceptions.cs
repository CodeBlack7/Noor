namespace Noor.Core.Common;

public abstract class ApiException : Exception
{
    public abstract int StatusCode { get; }
    protected ApiException(string message) : base(message) { }
}

public sealed class NotFoundException : ApiException
{
    public override int StatusCode => 404;
    public NotFoundException(string message) : base(message) { }
}

public sealed class ValidationFailedException : ApiException
{
    public override int StatusCode => 400;
    public ValidationFailedException(string message) : base(message) { }
}

public sealed class ConflictException : ApiException
{
    public override int StatusCode => 409;
    public ConflictException(string message) : base(message) { }
}

public sealed class ForbiddenException : ApiException
{
    public override int StatusCode => 403;
    public ForbiddenException(string message) : base(message) { }
}

public sealed class AuthenticationException : ApiException
{
    public override int StatusCode => 401;
    public AuthenticationException(string message) : base(message) { }
}
