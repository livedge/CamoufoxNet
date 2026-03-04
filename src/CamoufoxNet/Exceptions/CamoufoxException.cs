namespace CamoufoxNet.Exceptions;

public class CamoufoxException : Exception
{
    public CamoufoxException(string message) : base(message) { }
    public CamoufoxException(string message, Exception innerException) : base(message, innerException) { }
}
