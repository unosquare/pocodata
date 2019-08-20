namespace Unosquare.PocoData
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines standard database fields and methods to store, retrieve, update and delete
    /// data from a backing store.
    /// </summary>
    public interface IPocoDb
    {
        /// <summary>
        /// Gets the database connection object.
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Provides a helper object containing methods to read row data into object properties.
        /// </summary>
        PocoReader ObjectReader { get; }

        /// <summary>
        /// Provides a helper object containing methods to create and delete tables.
        /// </summary>
        IPocoDefinition Definition { get; }

        /// <summary>
        /// Provides a helper object containing methods to generate commands for standard operations.
        /// </summary>
        IPocoCommands Commands { get; }

        /// <summary>
        /// Gets a dynamically generated table proxy containing methods to perform CRUD operations on the given table-mapped type.
        /// </summary>
        /// <typeparam name="T">The type mapped to a database table.</typeparam>
        /// <returns>The table proxy.</returns>
        PocoTableProxy<T> TableProxy<T>()
            where T : class, new();

        /// <summary>
        /// Selects all records of the given table-mapped type.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <returns>An enumerable collection of the records that were retrieved.</returns>
        IEnumerable SelectAll(Type mappedType);

        /// <summary>
        /// Asynchronously selects all records of the given table-mapped type.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <returns>An enumerable collection of the records that were retrieved</returns>
        Task<IEnumerable> SelectAllAsync(Type mappedType);

        /// <summary>
        /// Selects all records of the given table-mapped type.
        /// </summary>
        /// <typeparam name="T">The table-mapped type.</typeparam>
        /// <returns>
        /// An enumerable collection of the records that were retrieved
        /// </returns>
        IEnumerable<T> SelectAll<T>()
            where T : class, new();

        /// <summary>
        /// Asynchronously selects all records of the given table-mapped type.
        /// </summary>
        /// <typeparam name="T">The table-mapped type.</typeparam>
        /// <returns>
        /// An enumerable collection of the records that were retrieved
        /// </returns>
        Task<IEnumerable<T>> SelectAllAsync<T>()
            where T : class, new();

        /// <summary>
        /// Executes the specified command and reads the results as records of the given table-mapped type.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <param name="command">The command.</param>
        /// <returns>
        /// An enumerable collection of the records that were retrieved
        /// </returns>
        IEnumerable SelectMany(Type mappedType, IDbCommand command);

        /// <summary>
        /// Asynchronously executes the specified command and reads the results as records of the given table-mapped type.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <param name="command">The command.</param>
        /// <returns>
        /// An enumerable collection of the records that were retrieved
        /// </returns>
        Task<IEnumerable> SelectManyAsync(Type mappedType, IDbCommand command);

        /// <summary>
        /// Executes the specified command and reads the results as records of the given table-mapped type.
        /// </summary>
        /// <typeparam name="T">The table-mapped type.</typeparam>
        /// <param name="command">The command.</param>
        /// <returns>
        /// An enumerable collection of the records that were retrieved
        /// </returns>
        IEnumerable<T> SelectMany<T>(IDbCommand command)
            where T : class, new();

        /// <summary>
        /// Asynchronously executes the specified command and reads the results as records of the given table-mapped type.
        /// </summary>
        /// <typeparam name="T">The table-mapped type.</typeparam>
        /// <param name="command">The command.</param>
        /// <returns>
        /// An enumerable collection of the records that were retrieved
        /// </returns>
        Task<IEnumerable<T>> SelectManyAsync<T>(IDbCommand command)
            where T : class, new();

        /// <summary>
        /// Selects a single record with matching keys of the given object and updates it with record data.
        /// </summary>
        /// <param name="target">The target object to update.</param>
        /// <returns>Whether the object was found and updated.</returns>
        bool SelectSingle(object target);

        /// <summary>
        /// Asynchronously selects a single record with matching keys of the given object and updates it with record data.
        /// </summary>
        /// <param name="target">The target object to update.</param>
        /// <returns>Whether the object was found and updated.</returns>
        Task<bool> SelectSingleAsync(object target);

        /// <summary>
        /// Asynchronously inserts an item to its corresponding mapped table.
        /// If the object has an autogenerated key, the object will have its value updated.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="update">if set to <c>true</c> it updates all data values from the store after the insert (in addition to autogenerated key values).</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> InsertAsync(object item, bool update);

        /// <summary>
        /// Inserts an item to its corresponding mapped table.
        /// If the object has an autogenerated key, the object will have its value updated.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="update">if set to <c>true</c> it updates all data values from the store after the insert (in addition to autogenerated key values).</param>
        /// <returns>The number of affected rows.</returns>
        int Insert(object item, bool update);

        /// <summary>
        /// Asynchronously inserts a collection of items into their corresponding table-mapped type.
        /// The collection must exclusively be of objects of the same type.
        /// </summary>
        /// <param name="items">The items to insert.</param>
        /// <param name="update">if set to <c>true</c> it will update data from inserted rows in addition to generated key values.</param>
        /// <returns>The number of affected rows</returns>
        Task<int> InsertManyAsync(IEnumerable items, bool update);

        /// <summary>
        /// Inserts a collection of items into their corresponding table-mapped type.
        /// The collection must exclusively be of objects of the same type.
        /// </summary>
        /// <param name="items">The items to insert.</param>
        /// <param name="update">if set to <c>true</c> it will update data from inserted rows in addition to generated key values.</param>
        /// <returns>The number of affected rows</returns>
        int InsertMany(IEnumerable items, bool update);

        /// <summary>
        /// Updates an item in its corresponding mapped table.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> UpdateAsync(object item);

        /// <summary>
        /// Updates an item in its corresponding mapped table.
        /// </summary>
        /// <param name="item">The item to update.</param>
        /// <returns>The number of affected rows.</returns>
        int Update(object item);

        /// <summary>
        /// Asynchronously updates a collection of items in their corresponding table-mapped type.
        /// The collection must exclusively be of objects of the same type.
        /// </summary>
        /// <param name="items">The items to update.</param>
        /// <returns>The number of affected rows</returns>
        Task<int> UpdateManyAsync(IEnumerable items);

        /// <summary>
        /// Updates a collection of items in their corresponding table-mapped type.
        /// The collection must exclusively be of objects of the same type.
        /// </summary>
        /// <param name="items">The items to update.</param>
        /// <returns>The number of affected rows</returns>
        int UpdateMany(IEnumerable items);

        /// <summary>
        /// Asynchronously deletes the row with matching object key values.
        /// </summary>
        /// <param name="item">The object to delete from the table.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> DeleteAsync(object item);

        /// <summary>
        /// Deletes the row with matching object key values.
        /// </summary>
        /// <param name="item">The object to delete from the table.</param>
        /// <returns>The number of affected rows.</returns>
        int Delete(object item);

        /// <summary>
        /// Counts all rows for the given table-mapped type.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <returns>The number of rows in the table.</returns>
        int CountAll(Type mappedType);

        /// <summary>
        /// Asynchronously counts all rows for the given table-mapped type.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <returns>The number of rows in the table.</returns>
        Task<int> CountAllAsync(Type mappedType);
    }
}
