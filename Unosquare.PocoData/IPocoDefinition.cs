namespace Unosquare.PocoData
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods to execute DDL commands on the database.
    /// </summary>
    public interface IPocoDefinition
    {
        /// <summary>
        /// Asynchronously creates a table of the corresponding table-mapped type.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <returns>The number of affected rows, typically -1.</returns>
        Task<int> CreateTableAsync(Type mappedType);

        /// <summary>
        /// Asynchronously creates a table of the corresponding table-mapped type.
        /// </summary>
        /// <typeparam name="T">The table-mapped type</typeparam>
        /// <returns>The number of affected rows, typically -1.</returns>
        Task<int> CreateTableAsync<T>();

        /// <summary>
        /// Determines if the table for the table-mapped type exists.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <returns>Whether or not the table exists.</returns>
        Task<bool> TableExistsAsync(Type mappedType);

        /// <summary>
        /// Determines if the table for the table-mapped type exists.
        /// </summary>
        /// <typeparam name="T">The table-mapped type</typeparam>
        /// <returns>Whether or not the table exists.</returns>
        Task<bool> TableExistsAsync<T>();

        /// <summary>
        /// Asynchronously deletes a table of the corresponding table-mapped type.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <returns>The awaitable task.</returns>
        Task DropTableAsync(Type mappedType);

        /// <summary>
        /// Asynchronously deletes a table of the corresponding table-mapped type.
        /// </summary>
        /// <typeparam name="T">The table-mapped type.</typeparam>
        /// <returns>The awaitable task.</returns>
        Task DropTableAsync<T>();
    }
}
