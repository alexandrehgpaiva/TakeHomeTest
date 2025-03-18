using System.Text;
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
                Console.WriteLine("Retrieving documents metadata...");
                List<Document> documents = await cmsClient.GetDocumentsMetadataAsync();
                Console.WriteLine("Documents retrieved successfully:");
                var properties = typeof(Document).GetProperties();
                foreach (var doc in documents)
                {
                    PrintDocumentMetadata(doc);
                    Console.WriteLine();
                }
                Console.WriteLine();

                // Get a document
                Console.WriteLine("Retrieving a document...");
                var document = await cmsClient.GetDocumentAsync("b3e1a6f4-8c2b-4d3a-9c3e-1f2b3a4d5e6f");
                Console.WriteLine("Document retrieved successfully:");
                PrintDocumentMetadata(document);
                Console.WriteLine();

                // Create a document
                Console.WriteLine("Creating a document...");
                var newDocumentId = Guid.NewGuid().ToString();
                var newDocument = new Document
                {
                    Id = newDocumentId,
                    Title = "New Document",
                    PublishDate = DateTime.Now.Ticks,
                    ExpiryDate = DateTime.Now.AddDays(30).Ticks,
                    Content = Document.GenerateRandomFileData()
                };
                await cmsClient.CreateDocumentAsync(newDocument);
                var newlyCreatedDocument = await cmsClient.GetDocumentAsync(newDocumentId);
                if (newlyCreatedDocument.Id == newDocumentId)
                {
                    Console.WriteLine("Document created successfully.");
                    PrintDocumentMetadata(newlyCreatedDocument);
                }
                else
                {
                    throw new Exception("Document not created.");
                }
                Console.WriteLine();

                // Update a document
                Console.WriteLine("Updating a document...");
                newDocument.Title = "Updated Document";
                await cmsClient.UpdateDocumentAsync(newDocument);
                var updatedDocument = await cmsClient.GetDocumentAsync(newDocumentId);
                if (updatedDocument.Title == "Updated Document")
                {
                    Console.WriteLine("Document updated successfully.");
                    PrintDocumentMetadata(updatedDocument);
                }
                else
                {
                    throw new Exception("Document not updated.");
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void PrintDocumentMetadata(Document doc)
        {
            foreach (var prop in doc.GetType().GetProperties())
            {
                Console.WriteLine($"{prop.Name}: {prop.GetValue(doc)}");
            }
        }
    }
}