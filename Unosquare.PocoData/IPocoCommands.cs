namespace Unosquare.PocoData
{
    using System;
    using System.Data;

    public interface IPocoCommands
    {
        string SelectCommandText(Type T);

        string InsertCommandText(Type T);

        string InsertCommandText<T>();

        string UpdateCommandText(Type T);

        string UpdateCommandText<T>();

        string DeleteCommandText(Type T);

        string DeleteCommandText<T>();

        IDbCommand CreateInsertCommand(object obj);

        IDbCommand CreateSelectCommand(object obj);

        IDbCommand CreateUpdateCommand(object obj);

        IDbCommand CreateDeleteCommand(object obj);
    }
}
