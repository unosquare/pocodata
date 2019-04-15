namespace Unosquare.PocoData
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    public interface IPocoDb
    {
        IDbConnection Connection { get; }

        IPocoReader PocoReader { get; }

        IPocoDefinition Definition { get; }

        IPocoCommands Commands { get; }

        Task<IReadOnlyList<T>> RetrieveAsync<T>(string tableName, params string[] columnNames)
            where T : class, new();

        Task<IReadOnlyList<T>> RetrieveAsync<T>()
            where T : class, new();

        Task<IReadOnlyList<T>> RetrieveAsync<T>(string sqlQueryText, int timeoutSeconds = 600)
            where T : class, new();

        Task<int> InsertAsync(object obj, bool update);

        Task<int> UpdateAsync(object obj);

        Task<int> DeleteAsync(object obj);
    }
}
