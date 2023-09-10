using System.Reflection;

namespace ProjectLawfulEbook;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
sealed class GitHashAttribute : System.Attribute
{
    public string Hash { get; }
    public GitHashAttribute(string hsh)
    {
        this.Hash = hsh;
    }

    private static string? _hash = null;
    public static string Get() => _hash ??= Assembly.GetEntryAssembly()!.GetCustomAttribute<GitHashAttribute>()!.Hash;
}
