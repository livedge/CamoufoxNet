namespace CamoufoxNet.Exceptions;

public class CamoufoxNotInstalledException : CamoufoxException
{
    public CamoufoxNotInstalledException()
        : base("The 'camoufox' Python package is not installed. Run: pip install camoufox && camoufox fetch") { }

    public CamoufoxNotInstalledException(string message) : base(message) { }
}
