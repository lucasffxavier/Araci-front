namespace Araci.Services.Interaction
{
    public class ConnectionValidationResult
    {
        private ConnectionValidationResult(bool isValid, string? message)
        {
            IsValid = isValid;
            Message = message;
        }

        public bool IsValid { get; }

        public string? Message { get; }

        public static ConnectionValidationResult Valid()
        {
            return new ConnectionValidationResult(true, null);
        }

        public static ConnectionValidationResult Invalid(string message)
        {
            return new ConnectionValidationResult(false, message);
        }
    }
}
