namespace CacheNamespacer
{
    public class NamespacerOptions
    {
        public NamespacerOptions()
        {
            this.Prefix = "___";

            this.OptimizeWithDefaultCounterAndEvidence = false;
            this.EvidenceSize = 80;
            ResetWhenEvidenceMuddled = true;
        }
        public string Prefix { get; set; }
        public bool OptimizeWithDefaultCounterAndEvidence { get; set; }
        public int EvidenceSize { get; set; }
        public bool ResetWhenEvidenceMuddled { get; set; }
    }
}