using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace NorthWind.Tests;
[TestClass]
public sealed class ArchitectureTests
{
    [TestMethod]
    public void ProjectReferences_RespectDependencyRuleAndTargetNet10()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "NorthWind.sln"))) root = root.Parent;
        Assert.IsNotNull(root);
        var props = XDocument.Load(Path.Combine(root.FullName, "Directory.Build.props"));
        Assert.AreEqual("net10.0", props.Descendants("TargetFramework").Single().Value);
        foreach (var path in Directory.GetFiles(Path.Combine(root.FullName, "src"), "*.csproj", SearchOption.AllDirectories))
        {
            var file = new FileInfo(path);
            int layer = Layer(root.FullName, path);
            var project = XDocument.Load(path);
            foreach (var reference in project.Descendants("ProjectReference"))
            {
                var referenced = new FileInfo(Path.GetFullPath(Path.Combine(file.DirectoryName!, reference.Attribute("Include")!.Value)));
                Assert.IsTrue(referenced.Exists, $"Referencia inexistente: {referenced.FullName}");
                int targetLayer = Layer(root.FullName, referenced.FullName);
                Assert.IsLessThanOrEqualTo(layer, targetLayer, $"Dependencia hacia afuera: {file.Name} -> {referenced.Name}");
            }
            if (layer <= 2)
                Assert.IsEmpty(project.Descendants("FrameworkReference").Where(r => r.Attribute("Include")?.Value == "Microsoft.AspNetCore.App"),
                    "El dominio y los casos de uso deben ser independientes del host web.");
        }
    }

    private static int Layer(string root, string project) => int.Parse(
        Path.GetRelativePath(Path.Combine(root, "src"), project).Split(Path.DirectorySeparatorChar)[0][..1]);

    [TestMethod]
    public void Solution_Has31ProjectsAndTheRequestedFourLayers()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "NorthWind.sln"))) root = root.Parent;
        Assert.IsNotNull(root);
        var projects = Directory.GetFiles(Path.Combine(root.FullName, "src"), "*.csproj", SearchOption.AllDirectories);
        Assert.HasCount(31, projects);
        Assert.HasCount(11, projects.Where(p => Layer(root.FullName, p) == 1).ToArray());
        Assert.HasCount(4, projects.Where(p => Layer(root.FullName, p) == 2).ToArray());
        Assert.HasCount(8, projects.Where(p => Layer(root.FullName, p) == 3).ToArray());
        Assert.HasCount(8, projects.Where(p => Layer(root.FullName, p) == 4).ToArray());
        var solution = File.ReadAllText(Path.Combine(root.FullName, "NorthWind.sln"));
        foreach (var path in projects)
            StringAssert.Contains(solution, Path.GetRelativePath(root.FullName, path));
        var frontend = XDocument.Load(projects.Single(p => Path.GetFileNameWithoutExtension(p) == "NorthWind.Sales.Frontend.BusinessObjects"));
        Assert.IsFalse(frontend.Descendants("ProjectReference").Any(p => p.Attribute("Include")!.Value.Contains("3. Interface Adapters")),
            "El ViewModel usa el contrato del gateway; la implementación HTTP queda en adaptadores.");
    }
}
