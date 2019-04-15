namespace Unosquare.PocoData
{
    using System.Data.Common;

    public interface IPocoReader
    {
        T ReadValue<T>(DbDataReader reader, string columnName);

        object ReadObject(DbDataReader reader, object result);

        T ReadObject<T>(DbDataReader reader, T result) where T : class;

        T ReadObject<T>(DbDataReader reader) where T : class, new();
    }
}
