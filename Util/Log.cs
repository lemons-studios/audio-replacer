using Metalama.Framework.Aspects;

namespace AudioReplacer.Util;
/// <summary>
/// Method Exception/Error logger built with PostSharp MetaLama
/// </summary>
public class Log : OverrideMethodAspect
{
    public override dynamic OverrideMethod()
    {
        // Get method information
        var methodName = meta.Target.Method.ToDisplayString();

        // File.AppendAllText(AppProperties.LogFile, $"\n{DateTime.Now:HH:mm:ss}: ENTERING {methodName}");
        try
        {
            // Execute original method
            var result = meta.Proceed();

            // File.AppendAllText(AppProperties.LogFile ,$"\n{DateTime.Now:HH:mm:ss}: EXITING {methodName}");
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
