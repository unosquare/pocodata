namespace Unosquare.PocoData
{
    using System;
    using System.Data;

    public interface IPocoCommands
    {
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
