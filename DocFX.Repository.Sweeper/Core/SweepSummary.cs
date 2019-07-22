namespace DocFX.Repository.Sweeper.Core
{
    public class SweepSummary
    {
        public Status Status { get; set; }

        public int TotalFilesProcessed { get; set; }

        public int TotalCrossReferences { get; set; }

        public override string ToString()
            => Status == Status.Success 
            ? $"[ SUCCESS ]: Out of the {TotalFilesProcessed:#,#} files processed, there were {TotalCrossReferences:#,#} cross references evaluated."
            : "[ FAILED ]";
    }
}