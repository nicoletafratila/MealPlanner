using System.Reflection;

namespace Common.Data.DataContext
{
    public class TableConfigurationAssemblies(IEnumerable<Assembly> assemblies)
    {
        public IReadOnlyList<Assembly> Assemblies { get; } = assemblies.ToList();
    }
}
