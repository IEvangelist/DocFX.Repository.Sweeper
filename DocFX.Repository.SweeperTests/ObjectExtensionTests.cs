using DocFX.Repository.Sweeper;
using DocFX.Repository.Sweeper.OpenPublishing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DocFX.Repository.SweeperTests
{
    public class ObjectExtensionTests
    {
        readonly ITestOutputHelper _output;

        public ObjectExtensionTests(ITestOutputHelper output) => _output = output;
        
        [Fact]
        public async Task ReadDocFxConfigTest()
        {
            var json = await File.ReadAllTextAsync("docs-repo/docfx.json");
            var docfx = json.FromJson<DocFxConfig>();

            Assert.NotNull(docfx);
            Assert.Equal("azure", docfx.Build.Dest);
            Assert.Equal("articles", docfx.Build.Content[0].Src);
        }

        [Fact]
        public void FromYamlToTocTest()
        {
            var yaml =
@"- name: Academic Knowledge Documentation
- name: Overview
  items:
  - name: Learn about the Academic Knowledge API
    href: Home.md
- name: How to Guides
  items:
  - name: Entity attributes
    href: EntityAttributes.md";

            var tocs = yaml.FromYaml<List<TableOfContents>>();

            Assert.NotNull(tocs);
            Assert.Equal(3, tocs.Count);
            Assert.Contains(tocs.SelectMany(toc => toc.items ?? Enumerable.Empty<Reference>()), item => item?.href == "Home.md");
            Assert.Equal("Home.md", tocs.FindOverviewLink());
        }

        [Fact]
        public void FromYamlToIndexTest()
        {
            var yaml =
                @"### YamlMime:YamlDocument
documentType: LandingData
title: Azure Security Center Documentation
metadata:
  title: Azure Security Center Documentation - Tutorials, API Reference | Microsoft Doc
  meta.description: Azure Security Center provides unified security management and advanced threat protection across hybrid cloud workloads. Learn how to get started with Security Center, apply security policies across your workloads, limit your exposure to threats, and detect and respond to attacks with our quickstarts and tutorials..
  services: security-center
  author: cmcclister
  manager: carolz
  ms.service: security-center
  ms.tgt_pltfrm: na
  ms.devlang: na
  ms.topic: landing-page
  ms.date: 07/26/18
  ms.author: cmcclister
abstract:
  description: Azure Security Center provides unified security management and advanced threat protection across hybrid cloud workloads. Learn how to get started with Security Center, apply security policies across your workloads, limit your exposure to threats, and detect and respond to attacks with our quickstarts and tutorials.
sections:";

            var index = yaml.FromYaml<Index>();

            Assert.NotNull(index);
            Assert.True(index.IsLandingPage);
        }

        #region Run test, copy output to OpenPublishing/Taxonomies.cs file. Update as needed.

        // https://review.docs.microsoft.com/en-us/new-hope/information-architecture/metadata/taxonomies?branch=master#dev-lang
        [Fact]
        public void Sandbox()
        {
            var table = @"| aspx | ASP.NET |  |  |
| aspx-csharp | ASP.NET (C#) |  | aspx-csharp |
| aspx-vb | ASP.NET (VB) |  | aspx-vb |
| azcopy | AZCopy |  | azcopy |
| azurecli | Azure CLI | azure-cli | azurecli |
| azurepowershell | Azure PowerShell |  | azurepowershell |
| brainscript | BrainScript | brainscript |  |
| c | C | c |   |
| cpp | C++ | cpp | cpp |
| cppcx | C++/CX |  | cppcx |
| cppwinrt | C++/WIN RT |  | cppwinrt |
| csharp | C# | csharp | csharp |
| cshtml | CSHTML |  | cshtml |
| dax | DAX |  | dax |
| fsharp | F# | fsharp | fsharp |
| go | Go | go | go |
| html | HTML |  | html |
| http | HTTP |  | http |
| java | Java | java | java |
| javascript | JavaScript | javascript | javascript |
| json | JSON | json | json |
| kusto | Kusto |  | kusto |
| md | Markdown |  | md |
| mof | Managed Object Format | mof |  |
| nodejs | Node.js | nodejs | nodejs |
| objc | Objective-C | objective-c | objc |
| odata | Odata |  | odata |
| php | PHP | php | php |
| powerappsfl | PowerApps Formula |  | powerappsfl |
| powershell | PowerShell | powershell | powershell |
| python | Python | python | python |
| qsharp | Q# | qsharp | qsharp |
| r | R | r |  |
| rest | REST API | rest-api | rest |
| ruby | Ruby | ruby | ruby |
| sql | SQL |  | sql |
| scala | Scala | spark-scala |   |
| solidity | Solidity | |   |
| swift | Swift |  | swift |
| tsql | Transact-SQL |  |  |
| typescript | TypeScript | typescript | typescript |
| usql | U-SQL |  | usql |
| vb | Visual Basic | vb | vb |
| vba | Visual Basic for Applications |   |   |
| vbs | Visual Basic Script | vbs |  |
| vstscli | VSTS CLI | vstscli | vstscli |
| XAML | XAML |  | XAML |
| xml | XML |  | xml |
| yaml | YAML |  | yaml |";

            string TrimAndNullIfEmpty(string s)
            {
                return string.IsNullOrWhiteSpace(s) ? "null" : $"\"{s.Trim()}\"";
            }

            var lines = table.Split('\r');
            foreach (var line in lines)
            {
                var split = line.Split("|");

                var slug = TrimAndNullIfEmpty(split[1]);
                var labl = TrimAndNullIfEmpty(split[2]);

                _output.WriteLine($"                [{slug}] = new Taxonomy({slug}, {labl}),");
            }
        }

        #endregion
    }
}