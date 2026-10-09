using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;
using System.Text.RegularExpressions;

string root = args.Length == 1 ? Path.GetFullPath(args[0]) : Directory.GetCurrentDirectory();
string assets = Path.Combine(root, "Assets");
if (!Directory.Exists(assets)) throw new DirectoryNotFoundException("Pass the Unity project root.");
int failures = 0;
string[] sources = Directory.GetFiles(assets, "*.cs", SearchOption.AllDirectories);
// Unity asmdef dependencies are direct: Runtime referencing Core does not expose
// Core to Editor. Check project namespace uses as well as syntax, without claiming
// to compile against the Unity APIs. Shared/ambiguous root namespaces are skipped.
var assemblies = Directory.GetFiles(assets, "*.asmdef", SearchOption.AllDirectories)
    .Select(file =>
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(file));
        var definition = doc.RootElement;
        return new {
            Directory = Path.GetDirectoryName(file)!,
            Name = definition.GetProperty("name").GetString()!,
            Namespace = definition.TryGetProperty("rootNamespace", out var ns) ? ns.GetString() ?? "" : "",
            References = definition.GetProperty("references").EnumerateArray().Select(r => r.GetString()!).ToHashSet(),
            Guid = Regex.Match(File.ReadAllText(file + ".meta"), @"(?m)^guid: ([a-f0-9]{32})$").Groups[1].Value
        };
    }).ToArray();
var namespaceOwners = assemblies.Where(a => a.Namespace.Length > 0)
    .GroupBy(a => a.Namespace).Where(group => group.Count() == 1)
    .Select(group => group.Single()).OrderByDescending(a => a.Namespace.Length).ToArray();
var missingDependencies = new HashSet<string>();
var profiles = new[] {
    new CSharpParseOptions(LanguageVersion.CSharp9),
    new CSharpParseOptions(LanguageVersion.CSharp9, preprocessorSymbols: new[] { "UNITY_EDITOR", "UNITY_2022_2_OR_NEWER", "UNITY_INCLUDE_TESTS" }),
    new CSharpParseOptions(LanguageVersion.CSharp9, preprocessorSymbols: new[] { "UNITY_ANDROID", "UNITY_2022_2_OR_NEWER", "UNITY_INCLUDE_TESTS" })
};
foreach (string file in sources)
{
    var owner = assemblies.Where(a => file.StartsWith(a.Directory + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        .OrderByDescending(a => a.Directory.Length).FirstOrDefault();
    foreach (var profile in profiles)
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), profile, file);
        foreach (var diagnostic in tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
        {
            Console.Error.WriteLine(diagnostic);
            failures++;
        }
        if (owner == null) continue;
        foreach (var node in tree.GetRoot().DescendantNodes().Where(n =>
            n is UsingDirectiveSyntax || n is QualifiedNameSyntax || n is MemberAccessExpressionSyntax))
        {
            string name = node is UsingDirectiveSyntax directive ? directive.Name?.ToString() ?? "" : node.ToString();
            name = name.Replace("global::", "");
            var dependency = namespaceOwners.FirstOrDefault(a => name == a.Namespace || name.StartsWith(a.Namespace + ".", StringComparison.Ordinal));
            if (dependency == null || dependency.Name == owner.Name || owner.References.Contains(dependency.Name) ||
                owner.References.Contains("GUID:" + dependency.Guid)) continue;
            string error = $"{file}: {owner.Name} uses {dependency.Namespace} without a direct reference to {dependency.Name}.";
            if (missingDependencies.Add(error)) { Console.Error.WriteLine(error); failures++; }
        }
    }
}
var guids = new HashSet<string>();
foreach (string file in Directory.GetFiles(assets, "*", SearchOption.AllDirectories).Where(p => !p.EndsWith(".meta")))
{
    if (!File.Exists(file + ".meta")) { Console.Error.WriteLine("Missing .meta: " + file); failures++; }
    if (file.EndsWith(".asmdef")) using (JsonDocument.Parse(File.ReadAllText(file))) { }
}
foreach (string meta in Directory.GetFiles(assets, "*.meta", SearchOption.AllDirectories))
{
    string guid = Regex.Match(File.ReadAllText(meta), @"(?m)^guid: ([a-f0-9]{32})$").Groups[1].Value;
    if (guid.Length == 0 || !guids.Add(guid)) { Console.Error.WriteLine("Invalid or duplicate GUID: " + meta); failures++; }
}
foreach (string file in Directory.GetFiles(assets, "*", SearchOption.AllDirectories).Where(p => p.EndsWith(".unity") || p.EndsWith(".mat") || p.EndsWith(".pocklemesh.meta")))
{
    foreach (Match match in Regex.Matches(File.ReadAllText(file), @"guid: ([a-f0-9]{32})"))
        if (!guids.Contains(match.Groups[1].Value)) { Console.Error.WriteLine("Unresolved asset GUID in " + file); failures++; }
}
using (JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "Packages", "manifest.json")))) { }
Console.WriteLine($"C# syntax checked: {sources.Length} files across {profiles.Length} symbol profiles (default, Editor, Android). Assembly definitions checked: {assemblies.Length}. Asset GUIDs checked: {guids.Count}. Failures: {failures}.");
Console.WriteLine("This validates syntax, project namespace dependencies, and asset references, not Unity API compilation, shader rendering, scene import, or device behavior.");
return failures == 0 ? 0 : 1;
