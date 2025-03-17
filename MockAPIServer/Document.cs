namespace MockAPIServer
{
    public class Document
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public long PublishDate { get; set; }
        public long ExpiryDate { get; set; }
    }

}