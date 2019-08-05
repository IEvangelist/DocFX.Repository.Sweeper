﻿using System;
using System.Collections.Generic;

namespace DocFX.Repository.Sweeper.OpenPublishing
{
    public class Taxonomies
    {
        private static ISet<string> _uniqueMonikers;

        public static ISet<string> UniqueMonikers
        {
            get
            {
                if (_uniqueMonikers != null)
                {
                    return _uniqueMonikers;
                }
                else
                {
                    _uniqueMonikers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    _uniqueMonikers.UnionWith(Aliases);
                    _uniqueMonikers.UnionWith(Languages.Keys);
                    return _uniqueMonikers;
                }
            }
        }

        // https://review.docs.microsoft.com/en-us/new-hope/information-architecture/metadata/taxonomies?branch=master#dev-lang
        static IDictionary<string, Taxonomy> Languages { get; } =
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

        static ISet<string> Aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "1c",
            "abnf",
            "accesslog",
            "ada",
            "armasm",
            "arm",
            "avrasm",
            "actionscript",
            "as",
            "apache",
            "apacheconf",
            "applescript",
            "osascript",
            "asciidoc",
            "adoc",
            "aspectj",
            "autohotkey",
            "autoit",
            "awk",
            "mawk",
            "nawk",
            "gawk",
            "axapta",
            "bash",
            "sh",
            "zsh",
            "basic",
            "bnf",
            "brainfuck",
            "bf",
            "cs",
            "csharp",
            "cpp",
            "c",
            "cc",
            "h",
            "c++",
            "h++",
            "hpp",
            "cal",
            "cos",
            "cls",
            "cmake",
            "cmake.in",
            "coq",
            "csp",
            "css",
            "capnproto",
            "capnp",
            "clojure",
            "clj",
            "coffeescript",
            "coffee",
            "cson",
            "iced",
            "crmsh",
            "crm",
            "pcmk",
            "crystal",
            "cr",
            "d",
            "dns",
            "zone",
            "bind",
            "dos",
            "bat",
            "cmd",
            "dart",
            "delphi",
            "dpr",
            "dfm",
            "pas",
            "pascal",
            "freepascal",
            "lazarus",
            "lpr",
            "lfm",
            "diff",
            "patch",
            "django",
            "jinja",
            "dockerfile",
            "docker",
            "dsconfig",
            "dts",
            "dust",
            "dst",
            "ebnf",
            "elixir",
            "elm",
            "erlang",
            "erl",
            "excel",
            "xls",
            "xlsx",
            "fsharp",
            "fs",
            "fix",
            "fortran",
            "f90",
            "f95",
            "gcode",
            "nc",
            "gams",
            "gms",
            "gauss",
            "gss",
            "gherkin",
            "go",
            "golang",
            "golo",
            "gololang",
            "gradle",
            "groovy",
            "xml",
            "html",
            "xhtml",
            "rss",
            "atom",
            "xjb",
            "xsd",
            "xsl",
            "plist",
            "http",
            "https",
            "haml",
            "handlebars",
            "hbs",
            "html.hbs",
            "html.handlebars",
            "haskell",
            "hs",
            "haxe",
            "hx",
            "hy",
            "hylang",
            "ini",
            "inform7",
            "i7",
            "irpf90",
            "json",
            "java",
            "jsp",
            "javascript",
            "js",
            "jsx",
            "leaf",
            "lasso",
            "ls",
            "lassoscript",
            "less",
            "ldif",
            "lisp",
            "livecodeserver",
            "livescript",
            "ls",
            "lua",
            "makefile",
            "mk",
            "mak",
            "markdown",
            "md",
            "mkdown",
            "mkd",
            "mathematica",
            "mma",
            "matlab",
            "maxima",
            "mel",
            "mercury",
            "mizar",
            "mojolicious",
            "monkey",
            "moonscript",
            "moon",
            "n1ql",
            "nsis",
            "nginx",
            "nginxconf",
            "nimrod",
            "nim",
            "nix",
            "ocaml",
            "ml",
            "objectivec",
            "mm",
            "objc",
            "obj-c",
            "glsl",
            "openscad",
            "scad",
            "ruleslanguage",
            "oxygene",
            "pf",
            "pf.conf",
            "php",
            "php3",
            "php4",
            "php5",
            "php6",
            "parser3",
            "perl",
            "pl",
            "pm",
            "pony",
            "powershell",
            "ps",
            "processing",
            "prolog",
            "protobuf",
            "puppet",
            "pp",
            "python",
            "py",
            "gyp",
            "profile",
            "k",
            "kdb",
            "qml",
            "r",
            "rib",
            "rsl",
            "graph",
            "instances",
            "ruby",
            "rb",
            "gemspec",
            "podspec",
            "thor",
            "irb",
            "rust",
            "rs",
            "scss",
            "sql",
            "p21",
            "step",
            "stp",
            "scala",
            "scheme",
            "scilab",
            "sci",
            "shell",
            "console",
            "smali",
            "smalltalk",
            "st",
            "stan",
            "stata",
            "stylus",
            "styl",
            "subunit",
            "swift",
            "tap",
            "tcl",
            "tk",
            "tex",
            "thrift",
            "tp",
            "twig",
            "craftcms",
            "typescript",
            "ts",
            "vbnet",
            "vb",
            "vbscript",
            "vbs",
            "vhdl",
            "vala",
            "verilog",
            "v",
            "vim",
            "x86asm",
            "xl",
            "tao",
            "xpath",
            "xq",
            "zephir",
            "zep"
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