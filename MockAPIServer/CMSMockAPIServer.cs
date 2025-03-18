using Newtonsoft.Json;
using TakeHome.Models;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Util;

namespace TakeHome.MockAPIServer
{
    public class CMSMockAPIServer
    {
        private static void Main(string[] args)
        {
            var server = WireMockServer.Start(5000);

            SetupAuth(server);
            SetupDocuments(server);

            Console.WriteLine($"Mock server running at {server.Url}");
            Console.WriteLine("Press any key to stop the server");

            Console.ReadLine();

            server.Stop();
        }

        private static void SetupAuth(WireMockServer server)
        {
            // /auth
            server
                .Given(Request.Create().WithPath("/auth").UsingPost())
                .RespondWith(Response.Create()
                    .WithCallback(request =>
                    {
                        var body = JsonConvert.DeserializeObject<dynamic>(request.Body);
                        if ((body?.username) != "someone@someplace.net" || (body?.password) != "reind33r Fl0tilla")
                        {
                            return new ResponseMessage { StatusCode = 403 };
                        }

                        return new ResponseMessage
                        {
                            StatusCode = 200,
                            BodyData = new BodyData { BodyAsJson = generateNewToken(), DetectedBodyType = WireMock.Types.BodyType.Json }
                        };
                    }));

            // /auth/refresh
            server
                .Given(Request.Create().WithPath("/auth/refresh").UsingGet())
                .RespondWith(Response.Create()
                    .WithCallback(request =>
                    {
                        if (!IsAuthorized(request, BearerToken))
                        {
                            return new ResponseMessage { StatusCode = 401 };
                        }

                        return new ResponseMessage
                        {
                            StatusCode = 200,
                            BodyData = new BodyData { BodyAsJson = generateNewToken(), DetectedBodyType = WireMock.Types.BodyType.Json }
                        };
                    }));
        }

        private static void SetupDocuments(WireMockServer server)
        {
            var documents = new List<Document>{
                new Document
                {
                    Id = "b3e1a6f4-8c2b-4d3a-9c3e-1f2b3a4d5e6f",
                    Title = "A Sample Document",
                    PublishDate = 1725148800,
                    ExpiryDate = 1735603199
                },
                new Document
                {
                    Id = "b3e1a6f5-8c2b-4d3a-9c3e-1f2b3a4d5e6f",
                    Title = "Another Sample Document",
                    PublishDate = 1725148800,
                    ExpiryDate = 1735603199
                }
            };

            server
                .Given(Request.Create().WithPath("/documents").UsingGet())
                .RespondWith(Response.Create()
                    .WithCallback(request =>
                    {
                        if (!IsAuthorized(request, BearerToken))
                        {
                            return new ResponseMessage { StatusCode = 401 };
                        }

                        return new ResponseMessage
                        {
                            StatusCode = 200,
                            BodyData = new BodyData { BodyAsJson = documents, DetectedBodyType = WireMock.Types.BodyType.Json }
                        };
                    }));

            server
                .Given(Request.Create().WithPath("/document/*").UsingGet())
                .RespondWith(Response.Create()
                    .WithCallback(request =>
                    {
                        if (!IsAuthorized(request, BearerToken))
                        {
                            return new ResponseMessage { StatusCode = 401 };
                        }

                        var id = request.PathSegments[1];
                        var document = documents.FirstOrDefault(d => d.Id == id);

                        if (document == null)
                        {
                            return new ResponseMessage { StatusCode = 404 };
                        }

                        document.FileData = GenerateRandomFileData();

                        return new ResponseMessage
                        {
                            StatusCode = 200,
                            BodyData = new BodyData { BodyAsJson = document, DetectedBodyType = WireMock.Types.BodyType.Json }
                        };
                    }));

            server
                .Given(Request.Create().WithPath("/document").UsingPost())
                .RespondWith(Response.Create()
                    .WithCallback(request =>
                    {
                        if (!IsAuthorized(request, BearerToken))
                        {
                            return new ResponseMessage { StatusCode = 401 };
                        }

                        if (string.IsNullOrEmpty(request.Body))
                        {
                            return new ResponseMessage { StatusCode = 400 };
                        }

                        var document = JsonConvert.DeserializeObject<Document>(request.Body);
                        if (document == null || string.IsNullOrEmpty(document.Id) || string.IsNullOrEmpty(document.Title))
                        {
                            return new ResponseMessage { StatusCode = 400 };
                        }

                        var existingDocument = documents.FirstOrDefault(d => d.Id == document.Id);
                        if (existingDocument != null)
                        {
                            documents.Remove(existingDocument);
                            documents.Add(document);
                            return new ResponseMessage { StatusCode = 200 };
                        }

                        documents.Add(document);
                        return new ResponseMessage { StatusCode = 201 };
                    }));
        }

        private static byte[] GenerateRandomFileData()
        {
            var random = new Random();
            int size = random.Next(500_000, 1_500_000); // Random size between 500KB and 1.5MB
            var fileData = new byte[size];
            random.NextBytes(fileData);
            return fileData;
        }


        private static string BearerToken = "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d"; // Setting up default value to test endpoints without having to authenticate
        private static long ExpiryDate = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds();

        private static object generateNewToken()
        {
            BearerToken = new Guid().ToString();
            ExpiryDate = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds();

            return new
            {
                bearerToken = BearerToken,
                expiryDate = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
            };
        }

        private static bool IsAuthorized(IRequestMessage request, string expectedToken)
        {
            if (request.Headers == null || !request.Headers.TryGetValue("Authorization", out var authorizationValues))
            {
                return false;
            }

            string? token = authorizationValues.FirstOrDefault();
            return !string.IsNullOrEmpty(token) && 
                token == ($"Bearer {expectedToken}") && 
                !IsTokenExpired();
        }

        private static bool IsTokenExpired()
        {
            var currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return currentUnixTime > ExpiryDate;
        }
    }
}