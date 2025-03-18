namespace TakeHome.Models
{
    public class Document
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public long PublishDate { get; set; }
        public long ExpiryDate { get; set; }
        public byte[] FileData { get; set; } = [];
    }
}