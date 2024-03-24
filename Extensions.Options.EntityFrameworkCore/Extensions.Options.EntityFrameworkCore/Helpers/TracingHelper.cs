using System.Diagnostics;
using System.Reflection;

namespace Extensions.Options.EntityFrameworkCore.Helpers;

internal static class TracingHelper
{
    private static readonly AssemblyName CurrentAssembly = typeof(TracingHelper).Assembly.GetName();

    private static readonly ActivitySource ActivitySource = new(CurrentAssembly.Name, CurrentAssembly.Version!.ToString());

    public static Activity? NewActivity(string name) => ActivitySource.StartActivity(name);
}
