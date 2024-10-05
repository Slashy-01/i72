using System.Collections;

namespace I72_Backend.Exceptions;

public class AppSqlException : Exception
{
    public Object Data { get; set; }
    public AppSqlException(String message, IDictionary data) : base(message)
    {
        Data = data;
    }
}