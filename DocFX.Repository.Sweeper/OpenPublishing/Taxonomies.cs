using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocFX.Repository.Sweeper.OpenPublishing
{
    public class Taxonomies
    {
        public static readonly Regex CodeFenceRegex =
            new Regex("```\b(?'slug'.+?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

        // https://review.docs.microsoft.com/en-us/new-hope/information-architecture/metadata/taxonomies?branch=master#dev-lang
        public static IDictionary<string, Taxonomy> Languages { get; } =
            new Dictionary<string, Taxonomy>(StringComparer.OrdinalIgnoreCase)
            {
                ["aspx"] = new Taxonomy("aspx", "ASP.NET"),
                ["aspx-csharp"] = new Taxonomy("aspx-csharp", "ASP.NET (C#)"),
                ["aspx-vb"] = new Taxonomy("aspx-vb", "ASP.NET (VB)"),
                ["azcopy"] = new Taxonomy("azcopy", "AZCopy"),
                ["azurecli"] = new Taxonomy("azurecli", "Azure CLI"),
                ["azurepowershell"] = new Taxonomy("azurepowershell", "Azure PowerShell"),
                ["brainscript"] = new Taxonomy("brainscript", "BrainScript"),
                ["c"] = new Taxonomy("c", "C"),
                ["cpp"] = new Taxonomy("cpp", "C++"),
                ["cppcx"] = new Taxonomy("cppcx", "C++/CX"),
                ["cppwinrt"] = new Taxonomy("cppwinrt", "C++/WIN RT"),
                ["csharp"] = new Taxonomy("csharp", "C#"),
                ["cshtml"] = new Taxonomy("cshtml", "CSHTML"),
                ["dax"] = new Taxonomy("dax", "DAX"),
                ["fsharp"] = new Taxonomy("fsharp", "F#"),
                ["go"] = new Taxonomy("go", "Go"),
                ["html"] = new Taxonomy("html", "HTML"),
                ["http"] = new Taxonomy("http", "HTTP"),
                ["java"] = new Taxonomy("java", "Java"),
                ["javascript"] = new Taxonomy("javascript", "JavaScript"),
                ["json"] = new Taxonomy("json", "JSON"),
                ["kusto"] = new Taxonomy("kusto", "Kusto"),
                ["md"] = new Taxonomy("md", "Markdown"),
                ["mof"] = new Taxonomy("mof", "Managed Object Format"),
                ["nodejs"] = new Taxonomy("nodejs", "Node.js"),
                ["objc"] = new Taxonomy("objc", "Objective-C"),
                ["odata"] = new Taxonomy("odata", "Odata"),
                ["php"] = new Taxonomy("php", "PHP"),
                ["powerappsfl"] = new Taxonomy("powerappsfl", "PowerApps Formula"),
                ["powershell"] = new Taxonomy("powershell", "PowerShell"),
                ["python"] = new Taxonomy("python", "Python"),
                ["qsharp"] = new Taxonomy("qsharp", "Q#"),
                ["r"] = new Taxonomy("r", "R"),
                ["rest"] = new Taxonomy("rest", "REST API"),
                ["ruby"] = new Taxonomy("ruby", "Ruby"),
                ["sql"] = new Taxonomy("sql", "SQL"),
                ["scala"] = new Taxonomy("scala", "Scala"),
                ["solidity"] = new Taxonomy("solidity", "Solidity"),
                ["swift"] = new Taxonomy("swift", "Swift"),
                ["tsql"] = new Taxonomy("tsql", "Transact-SQL"),
                ["typescript"] = new Taxonomy("typescript", "TypeScript"),
                ["usql"] = new Taxonomy("usql", "U-SQL"),
                ["vb"] = new Taxonomy("vb", "Visual Basic"),
                ["vba"] = new Taxonomy("vba", "Visual Basic for Applications"),
                ["vbs"] = new Taxonomy("vbs", "Visual Basic Script"),
                ["vstscli"] = new Taxonomy("vstscli", "VSTS CLI"),
                ["XAML"] = new Taxonomy("XAML", "XAML"),
                ["xml"] = new Taxonomy("xml", "XML"),
                ["yaml"] = new Taxonomy("yaml", "YAML")
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