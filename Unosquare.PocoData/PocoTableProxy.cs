namespace Unosquare.PocoData
{
    using Annotations;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    public class PocoTableProxy<T>
        where T : class, new()
    {
        public PocoTableProxy(IPocoDb pocoDb)
        {
            PocoDb = pocoDb;
            PocoSchema.Instance.Validate<T>();
        }

        public IPocoDb PocoDb { get; }

        public bool TableExists => PocoDb.Definition.TableExistsAsync<T>().GetAwaiter().GetResult();

        public void CreateTable() => PocoDb.Definition.CreateTableAsync<T>().GetAwaiter().GetResult();

        public void DropTable() => PocoDb.Definition.DropTableAsync<T>().GetAwaiter().GetResult();

        public TableAttribute Table => PocoSchema.Instance.Table<T>();

        public IReadOnlyList<ColumnMetadata> Columns => PocoSchema.Instance.Columns<T>();

        public IEnumerable<T> SelectAll() => PocoDb.SelectAll<T>();

        public async Task<IEnumerable<T>> SelectAllAsync() => await PocoDb.SelectAllAsync<T>();

        public IEnumerable<T> SelectMany(IDbCommand command) => PocoDb.SelectMany<T>(command);

        public async Task<IEnumerable<T>> SelectManyAsync(IDbCommand command) => await PocoDb.SelectManyAsync<T>(command);

        public bool SelectSingle(T target) => PocoDb.SelectSingle(target);

        public async Task<bool> SelectSingleAsync(T target) => await PocoDb.SelectSingleAsync(target);

        public async Task<int> InsertAsync(T item, bool update) => await PocoDb.InsertAsync(item, update);

        public int Insert(T item, bool update) => PocoDb.Insert(item, update);

        public async Task<int> InsertManyAsync(IEnumerable<T> items, bool update) => await PocoDb.InsertManyAsync(items, update);

        public int InsertMany(IEnumerable<T> items, bool update) => PocoDb.InsertMany(items, update);

        public async Task<int> UpdateAsync(T item) => await PocoDb.UpdateAsync(item);

        public int Update(T item) => PocoDb.Update(item);

        public async Task<int> UpdateManyAsync(IEnumerable<T> items) => await PocoDb.UpdateManyAsync(items);

        public int UpdateMany(IEnumerable<T> items) => PocoDb.UpdateMany(items);

        public async Task<int> DeleteAsync(T obj) => await PocoDb.DeleteAsync(obj);

        public int Delete(T obj) => PocoDb.Delete(obj);

        public int CountAll() => PocoDb.CountAll(typeof(T));

        public async Task<int> CountAllAsync() => await PocoDb.CountAllAsync(typeof(T));
    }
}
