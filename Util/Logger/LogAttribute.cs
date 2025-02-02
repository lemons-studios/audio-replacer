using Metalama.Framework.Aspects;
using System;
using System.IO;

namespace AudioReplacer.Util.Logger;
public class LogAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        // Get method information
        string methodName = meta.Target.Method.ToDisplayString();

        File.AppendAllText(Generic.LogFile, $"\n{DateTime.Now:HH:mm:ss}: ENTERING {methodName}");
        try
        {
            // Execute original method
            dynamic? result = meta.Proceed();

            File.AppendAllText(Generic.LogFile ,$"\n{DateTime.Now:HH:mm:ss}: EXITING {methodName}");
            return result;
        }
        catch (Exception e)
        {
            // Log exception
            File.AppendAllText(Generic.LogFile, $"\n{DateTime.Now:HH:mm:ss}: EXCEPTION in {methodName}: {e.GetType().Name} - {e.Message}");
            throw;
        }
    }
}

