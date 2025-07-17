namespace Infonetica_Task.Exceptions
{
    public class ValidationException : WorkflowException
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
