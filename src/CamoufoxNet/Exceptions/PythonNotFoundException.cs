namespace CamoufoxNet.Exceptions;

public class PythonNotFoundException : CamoufoxException
{
    public PythonNotFoundException()
        : base("Python executable not found. Ensure Python 3.10+ is installed and on PATH, or set PythonPath in CamoufoxOptions.") { }

    public PythonNotFoundException(string message) : base(message) { }
}
