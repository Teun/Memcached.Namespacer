using System;

namespace CacheNamespacer
{
    public class NamespacerOptions
    {
        public NamespacerOptions()
        {
            this.Prefix = "___";
            this.OptimizeWithDefaultCounterAndEvidence = true;
            this.EvidenceSize = 80;
            ResetWhenEvidenceMuddled = true;
            RollingPeriod = TimeSpan.FromSeconds(180);
        }
        public string Prefix { get; set; }
        public bool OptimizeWithDefaultCounterAndEvidence { get; set; }
        public int EvidenceSize { get; set; }
        public bool ResetWhenEvidenceMuddled { get; set; }
        public TimeSpan RollingPeriod { get; set; }
    }
}