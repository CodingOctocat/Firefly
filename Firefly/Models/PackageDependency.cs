namespace Firefly.Models;

public class PackageDependency
{
    public string Name { get; set; }

    public string Version { get; set; }

    public PackageDependency(string name, string version)
    {
        Name = name;
        Version = version;
    }

    public override string ToString()
    {
        return $"{Name} - {Version}";
    }
}
