using System.Collections.Generic;

namespace DocFX.Repository.Sweeper.OpenPublishing
{
    public class Taxonomies
    {
        // https://review.docs.microsoft.com/en-us/new-hope/information-architecture/metadata/taxonomies?branch=master#dev-lang
        public static IReadOnlyList<Taxonomy> Languages { get; } =
            new List<Taxonomy>
            {
                new Taxonomy("aspx", "ASP.NET", null, null),
                new Taxonomy("aspx-csharp", "ASP.NET (C#)", null, "aspx-csharp"),
                new Taxonomy("aspx-vb", "ASP.NET (VB)", null, "aspx-vb"),
                new Taxonomy("azcopy", "AZCopy", null, "azcopy"),
                new Taxonomy("azurecli", "Azure CLI", "azure-cli", "azurecli"),
                new Taxonomy("azurepowershell", "Azure PowerShell", null, "azurepowershell"),
                new Taxonomy("brainscript", "BrainScript", "brainscript", null),
                new Taxonomy("c", "C", "c", null),
                new Taxonomy("cpp", "C++", "cpp", "cpp"),
                new Taxonomy("cppcx", "C++/CX", null, "cppcx"),
                new Taxonomy("cppwinrt", "C++/WIN RT", null, "cppwinrt"),
                new Taxonomy("csharp", "C#", "csharp", "csharp"),
                new Taxonomy("cshtml", "CSHTML", null, "cshtml"),
                new Taxonomy("dax", "DAX", null, "dax"),
                new Taxonomy("fsharp", "F#", "fsharp", "fsharp"),
                new Taxonomy("go", "Go", "go", "go"),
                new Taxonomy("html", "HTML", null, "html"),
                new Taxonomy("http", "HTTP", null, "http"),
                new Taxonomy("java", "Java", "java", "java"),
                new Taxonomy("javascript", "JavaScript", "javascript", "javascript"),
                new Taxonomy("json", "JSON", "json", "json"),
                new Taxonomy("kusto", "Kusto", null, "kusto"),
                new Taxonomy("md", "Markdown", null, "md"),
                new Taxonomy("mof", "Managed Object Format", "mof", null),
                new Taxonomy("nodejs", "Node.js", "nodejs", "nodejs"),
                new Taxonomy("objc", "Objective-C", "objective-c", "objc"),
                new Taxonomy("odata", "Odata", null, "odata"),
                new Taxonomy("php", "PHP", "php", "php"),
                new Taxonomy("powerappsfl", "PowerApps Formula", null, "powerappsfl"),
                new Taxonomy("powershell", "PowerShell", "powershell", "powershell"),
                new Taxonomy("python", "Python", "python", "python"),
                new Taxonomy("qsharp", "Q#", "qsharp", "qsharp"),
                new Taxonomy("r", "R", "r", null),
                new Taxonomy("rest", "REST API", "rest-api", "rest"),
                new Taxonomy("ruby", "Ruby", "ruby", "ruby"),
                new Taxonomy("sql", "SQL", null, "sql"),
                new Taxonomy("scala", "Scala", "spark-scala", null),
                new Taxonomy("solidity", "Solidity", null, null),
                new Taxonomy("swift", "Swift", null, "swift"),
                new Taxonomy("tsql", "Transact-SQL", null, null),
                new Taxonomy("typescript", "TypeScript", "typescript", "typescript"),
                new Taxonomy("usql", "U-SQL", null, "usql"),
                new Taxonomy("vb", "Visual Basic", "vb", "vb"),
                new Taxonomy("vba", "Visual Basic for Applications", null, null),
                new Taxonomy("vbs", "Visual Basic Script", "vbs", null),
                new Taxonomy("vstscli", "VSTS CLI", "vstscli", "vstscli"),
                new Taxonomy("XAML", "XAML", null, "XAML"),
                new Taxonomy("xml", "XML", null, "xml"),
                new Taxonomy("yaml", "YAML", null, "yaml")
            };
    }

    public struct Taxonomy
    {
        public string Slug;
        public string Label;
        public string MicrosoftLang;
        public string LangMapping;

        public Taxonomy(
            string slug,
            string label,
            string microsoftLang = null,
            string langMapping = null)
        {
            Slug = slug;
            Label = label;
            MicrosoftLang = microsoftLang;
            LangMapping = langMapping;
        }
    }
}