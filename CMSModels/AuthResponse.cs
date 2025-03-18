namespace TakeHome.Models
{
    public class AuthResponse
    {
        public string BearerToken { get; set; } = string.Empty;
        public ulong ExpiryDate { get; set; }
    }
}
