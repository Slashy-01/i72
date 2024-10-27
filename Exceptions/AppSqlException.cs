using System.Collections;

namespace I72_Backend.Exceptions;

/*
 * A customized Exception to handle any SQL relevant exceptions in the app
 */
public class AppSqlException : Exception
{
    public Object Data { get; set; }
    public AppSqlException(String message, IDictionary data) : base(message)
    {
        Data = data;
    }
}