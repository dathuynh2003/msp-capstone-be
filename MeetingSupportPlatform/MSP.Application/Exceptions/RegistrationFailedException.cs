namespace MSP.Application.Exceptions
{
    public class RegistrationFailedException(IEnumerable<string> errorDescriptpions) : Exception($"Registration failed with following errors: {string.Join(Environment.NewLine, errorDescriptpions)}");
}
