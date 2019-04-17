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
        /// Selects all command text.
        /// </summary>
        /// <param name="T">The t.</param>
        /// <returns></returns>
        string SelectAllCommandText(Type T);

        string SelectSingleCommandText(Type T);

        string InsertCommandText(Type T);

        string UpdateCommandText(Type T);

        string DeleteCommandText(Type T);

        string CountAllCommandText(Type T);

        IDbCommand CreateSelectAllCommand(Type T);

        IDbCommand CreateSelectSingleCommand(object obj);

        IDbCommand CreateInsertCommand(object obj);

        IDbCommand CreateUpdateCommand(object obj);

        IDbCommand CreateDeleteCommand(object obj);

        IDbCommand CreateCountAllCommand(Type T);
    }
}
