using System.Collections.Generic;

namespace DocFX.Repository.Sweeper.OpenPublishing
{
    public class Taxonomies
    {
        // https://review.docs.microsoft.com/en-us/new-hope/information-architecture/metadata/taxonomies?branch=master#dev-lang
        public static IReadOnlyList<Taxonomy> Languages { get; } =
            new List<Taxonomy>
            {
                new Taxonomy("aspx", "ASP.NET"),
                new Taxonomy("aspx-csharp", "ASP.NET (C#)"),
                new Taxonomy("aspx-vb", "ASP.NET (VB)"),
                new Taxonomy("azcopy", "AZCopy"),
                new Taxonomy("azurecli", "Azure CLI"),
                new Taxonomy("azurepowershell", "Azure PowerShell"),
                new Taxonomy("brainscript", "BrainScript"),
                new Taxonomy("c", "C"),
                new Taxonomy("cpp", "C++"),
                new Taxonomy("cppcx", "C++/CX"),
                new Taxonomy("cppwinrt", "C++/WIN RT"),
                new Taxonomy("csharp", "C#"),
                new Taxonomy("cshtml", "CSHTML"),
                new Taxonomy("dax", "DAX"),
                new Taxonomy("fsharp", "F#"),
                new Taxonomy("go", "Go"),
                new Taxonomy("html", "HTML"),
                new Taxonomy("http", "HTTP"),
                new Taxonomy("java", "Java"),
                new Taxonomy("javascript", "JavaScript"),
                new Taxonomy("json", "JSON"),
                new Taxonomy("kusto", "Kusto"),
                new Taxonomy("md", "Markdown"),
                new Taxonomy("mof", "Managed Object Format"),
                new Taxonomy("nodejs", "Node.js"),
                new Taxonomy("objc", "Objective-C"),
                new Taxonomy("odata", "Odata"),
                new Taxonomy("php", "PHP"),
                new Taxonomy("powerappsfl", "PowerApps Formula"),
                new Taxonomy("powershell", "PowerShell"),
                new Taxonomy("python", "Python"),
                new Taxonomy("qsharp", "Q#"),
                new Taxonomy("r", "R"),
                new Taxonomy("rest", "REST API"),
                new Taxonomy("ruby", "Ruby"),
                new Taxonomy("sql", "SQL"),
                new Taxonomy("scala", "Scala"),
                new Taxonomy("solidity", "Solidity"),
                new Taxonomy("swift", "Swift"),
                new Taxonomy("tsql", "Transact-SQL"),
                new Taxonomy("typescript", "TypeScript"),
                new Taxonomy("usql", "U-SQL"),
                new Taxonomy("vb", "Visual Basic"),
                new Taxonomy("vba", "Visual Basic for Applications"),
                new Taxonomy("vbs", "Visual Basic Script"),
                new Taxonomy("vstscli", "VSTS CLI"),
                new Taxonomy("XAML", "XAML"),
                new Taxonomy("xml", "XML"),
                new Taxonomy("yaml", "YAML")
            };
    }

    public struct Taxonomy
    {
        public string Slug;
        public string Label;

        public Taxonomy(
            string slug,
            string label)
        {
            Slug = slug;
            Label = label;
        }
    }
}