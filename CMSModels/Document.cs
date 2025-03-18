namespace TakeHome.Models
{
    public class Document
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public long PublishDate { get; set; }
        public long ExpiryDate { get; set; }
        public byte[] FileData { get; set; } = [];

        public static byte[] GenerateRandomFileData()
        {
            var random = new Random();
            int size = random.Next(500_000, 1_500_000); // Random size between 500KB and 1.5MB
            var fileData = new byte[size];
            random.NextBytes(fileData);
            return fileData;
        }
    }
}