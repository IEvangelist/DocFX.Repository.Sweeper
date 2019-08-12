namespace DocFX.Repository.Services
{
    public class GitHubOptions
    {
        public string ApiToken { get; set; }

        public string Owner { get; set; } = "MicrosoftDocs";

        public string Repo { get; set; } = "azure-docs-pr";
    }
}