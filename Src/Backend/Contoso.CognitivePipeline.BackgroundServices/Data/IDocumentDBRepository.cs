using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.BackgroundServices.Data
{
    public interface IDocumentDBRepository<T> where T : class
    {
        Task<Document> CreateItemAsync(T item);
        Task DeleteItemAsync(string id);
        Task<T> GetItemAsync(string id);
        Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate);
        int GetItemsCount();
        void Initialize(string endpoint, string key, string databaseId);
        Task<Document> UpdateItemAsync(string id, T item);
        string Origin { get; }
    }
}
