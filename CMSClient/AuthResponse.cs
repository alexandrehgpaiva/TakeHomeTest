namespace TakeHomeLibrary
{
    public class AuthResponse
    {
        public string BearerToken { get; set; } = string.Empty;
        public ulong ExpiryDate { get; set; }
    }
}
