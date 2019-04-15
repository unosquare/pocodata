namespace Unosquare.PocoData
{
    using System;
    using System.Threading.Tasks;

    public interface IPocoDefinition
    {
        Task<int> CreateTableAsync(Type T);

        Task<int> CreateTableAsync<T>();

        Task<bool> TableExistsAsync(Type T);

        Task<bool> TableExistsAsync<T>();

        Task DropTableAsync(Type T);

        Task DropTableAsync<T>();
    }
}
