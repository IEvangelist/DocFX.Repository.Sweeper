using System;

namespace DocFX.Repository.Sweeper.OpenPublishing
{
    public class Index
    {
        const string LandingData = nameof(LandingData);

        public string documentType { get; set; }
        public string title { get; set; }

        public bool IsLandingPage 
            => string.Equals(documentType, LandingData, StringComparison.OrdinalIgnoreCase);
    }
}