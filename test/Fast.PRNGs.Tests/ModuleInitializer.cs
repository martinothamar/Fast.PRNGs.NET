using System.Runtime.CompilerServices;

namespace Fast.PRNGs.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Run()
    {
        Plotly.NET.Defaults.DefaultWidth = 1100;
        Plotly.NET.Defaults.DefaultHeight = 1100;
    }
}
