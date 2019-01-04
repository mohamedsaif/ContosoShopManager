using Contoso.CognitivePipeline.BackgroundServices.Functions.Utilities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.BackgroundServices.Data
{
    public class CosmosDBRepository<T> : IDocumentDBRepository<T> where T : class
    {
        string collectionId;
        string databaseId;
        DocumentClient client;

        public string Origin { get; }

        public CosmosDBRepository()
        {
            //Initialize with db settings from settings
            Initialize(GlobalSettings.GetKeyValue("CosmosDbEndpoint"), GlobalSettings.GetKeyValue("CosmosDbKey"), GlobalSettings.GetKeyValue("CosmosDbName"));
            Origin = GlobalSettings.GetKeyValue("Origin");
        }

        public CosmosDBRepository(string endpoint, string key, string dbName, string origin)
        {
            //Initialize with db settings from settings
            Initialize(endpoint, key, dbName);
            Origin = origin;
        }

        public void Initialize(string endpoint, string key, string databaseId)
        {
            this.databaseId = databaseId;
            client = new DocumentClient(new Uri(endpoint), key, new ConnectionPolicy { EnableEndpointDiscovery = false });

            CreateDatabaseIfNotExistsAsync().Wait();

            //Collection name is dynamically selected based on the type of T :)
            CreateCollectionIfNotExistsAsync().Wait();
        }

        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public int GetItemsCount()
        {
            throw new NotImplementedException("Performance optimized implementation needed!");
            //try
            //{
            //    var document = client.CreateDocumentCollectionQuery(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), "SELECT c.id FROM c");
            //    return (int)(dynamic)document.Count();
            //}
            //catch (DocumentClientException e)
            //{
            //    if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            //    {
            //        return 0;
            //    }
            //    throw;
            //}
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            collectionId = GetCollectionName();

            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                new FeedOptions { MaxItemCount = -1 })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public async Task<Document> CreateItemAsync(T item)
        {
            collectionId = GetCollectionName();
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), item);
        }

        public async Task<Document> UpdateItemAsync(string id, T item)
        {
            collectionId = GetCollectionName();
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id), item);
        }

        public async Task DeleteItemAsync(string id)
        {
            collectionId = GetCollectionName();
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id));
        }

        async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));
                //Or using the built in check to create the database
                //await this.client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId });
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = databaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        async Task CreateCollectionIfNotExistsAsync()
        {
            collectionId = GetCollectionName();

            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseId),
                        new DocumentCollection { Id = collectionId },
                        new RequestOptions { OfferThroughput = 400 });
                }
                else
                {
                    throw;
                }
            }
        }

        string GetCollectionName()
        {
            //Dynamically selecting the name of the collection based on the type of T allows the re-usability of the repository for multiple collections
            var name = typeof(T).Name.ToLower();
            if (name.ToCharArray().Last().ToString() != "s")
                return $"{name}s";
            return name;
        }
    }
}
