using TakeHome.Client;
using TakeHome.Models;

namespace TakeHome.ClientTester
{
    public class CMSClientTester
    {
        private static async Task Main(string[] args)
        {
            var baseAddress = "http://localhost:5000"; // Adjust the base address if needed
            var cmsClient = new CMSClient(baseAddress);

            try
            {
                // Authenticate
                await cmsClient.AuthenticateAsync("d4e5f6a7-8b9c-4d3a-9c3e-1f2b3a4d5e6f", "someone@someplace.net", "reind33r Fl0tilla");
                Console.WriteLine("Authentication successful.");

                // Get documents metadata
                List<Document> documents = await cmsClient.GetDocumentsMetadataAsync();
                Console.WriteLine("Documents retrieved successfully:");

                var properties = typeof(Document).GetProperties();
                foreach (var doc in documents)
                {
                    foreach (var prop in doc.GetType().GetProperties())
                    {
                        Console.WriteLine($"{prop.Name}: {prop.GetValue(doc)}");
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}