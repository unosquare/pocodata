namespace Unosquare.PocoData
{
    using System;
    using System.Data;

    /// <summary>
    /// Represents an interface with methods to generate standard commands.
    /// </summary>
    public interface IPocoCommands
    {
        /// <summary>
        /// Gets the command text to select all records.
        /// </summary>
        /// <param name="T">The type that is mapped to the database.</param>
        /// <returns>The generated command text.</returns>
        string SelectAllCommandText(Type T);

        /// <summary>
        /// Gets the command text to select a single record by its key column values.
        /// </summary>
        /// <param name="T">The type that is mapped to the database.</param>
        /// <returns>The generated command text.</returns>
        string SelectSingleCommandText(Type T);

        /// <summary>
        /// Gets the command text to inset a single record of the given type.
        /// </summary>
        /// <param name="T">The type that is mapped to the database.</param>
        /// <returns>The generated command text.</returns>
        string InsertCommandText(Type T);

        /// <summary>
        /// Gets the command text to update a single record of the given type.
        /// </summary>
        /// <param name="T">The type that is mapped to the database.</param>
        /// <returns>The generated command text.</returns>
        string UpdateCommandText(Type T);

        /// <summary>
        /// Gets the command text to delete a single record of the given type.
        /// </summary>
        /// <param name="T">The type that is mapped to the database.</param>
        /// <returns>The generated command text.</returns>
        string DeleteCommandText(Type T);

        /// <summary>
        /// Gets the command text to count all records of the given type.
        /// </summary>
        /// <param name="T">The type that is mapped to the database.</param>
        /// <returns>The generated command text.</returns>
        string CountAllCommandText(Type T);

        /// <summary>
        /// Creates a command to select all records of the given type.
        /// </summary>
        /// <param name="T">The type that is mapped to the database.</param>
        /// <returns>The generated command.</returns>
        IDbCommand CreateSelectAllCommand(Type T);


        /// <summary>
        /// Creates a command to select a single record matching the key columns of the given object.
        /// </summary>
        /// <param name="mappedObject">The mapped object.</param>
        /// <returns>The generated command.</returns>
        IDbCommand CreateSelectSingleCommand(object mappedObject);

        /// <summary>
        /// Creates a command to insert a single record of the given object.
        /// </summary>
        /// <param name="mappedObject">The mapped object.</param>
        /// <returns>The generated command.</returns>
        IDbCommand CreateInsertCommand(object mappedObject);

        /// <summary>
        /// Creates a command to update a single record of the given object.
        /// </summary>
        /// <param name="mappedObject">The mapped object.</param>
        /// <returns>The generated command.</returns>
        IDbCommand CreateUpdateCommand(object mappedObject);

        /// <summary>
        /// Creates a command to delete a single record of the given object.
        /// </summary>
        /// <param name="mappedObject">The mapped object.</param>
        /// <returns>The generated command.</returns>
        IDbCommand CreateDeleteCommand(object mappedObject);

        /// <summary>
        /// Creates a command to count all records of the given type.
        /// </summary>
        /// <param name="T">The type that is mapped to the database.</param>
        /// <returns>The generated command text.</returns>
        IDbCommand CreateCountAllCommand(Type T);
    }
}
