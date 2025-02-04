using AudioReplacer.Generic;
using Metalama.Framework.Aspects;
using System;
using System.IO;

namespace AudioReplacer.Util;
public class Log : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        // Get method information
        string methodName = meta.Target.Method.ToDisplayString();

        File.AppendAllText(AppProperties.LogFile, $"\n{DateTime.Now:HH:mm:ss}: ENTERING {methodName}");
        try
        {
            // Execute original method
            dynamic? result = meta.Proceed();

            File.AppendAllText(AppProperties.LogFile ,$"\n{DateTime.Now:HH:mm:ss}: EXITING {methodName}");
            return result;
        }
        catch (Exception e)
        {
            // Log exception
            File.AppendAllText(AppProperties.LogFile, $"\n{DateTime.Now:HH:mm:ss}: EXCEPTION in {methodName}: {e.GetType().Name} - {e.Message}");
            throw;
        }
    }
}

