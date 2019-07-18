namespace DocFX.Repository.Sweeper.Core
{
    public class SweepSummary
    {
        public TokenizationStatus Status { get; set; }

        public int TotalFilesProcessed { get; set; }

        public int TotalCrossReferences { get; set; }

        public override string ToString()
            => Status == TokenizationStatus.Success 
            ? $"[ SUCCESS ]: Out of the {TotalFilesProcessed:#,#} files processed, there were {TotalCrossReferences:#,#} cross references evaluated."
            : "[ FAILED ]";
    }
}