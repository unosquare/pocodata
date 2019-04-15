namespace Unosquare.PocoData
{
    using System.Data;

    public interface IPocoReader
    {
        object ReadObject(IDataReader reader, object result);

        T ReadObject<T>(IDataReader reader, T result) where T : class;

        T ReadObject<T>(IDataReader reader) where T : class, new();
    }
}
