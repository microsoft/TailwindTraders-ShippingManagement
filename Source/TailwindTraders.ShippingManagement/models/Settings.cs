namespace TailwindTraders.ShippingManagement.Models
{
    public class Settings
    {
        public string FormRecognizedSubscriptionKey { get; set; }

        public string FormRecognizedEndPoint { get; set; }

        public string FormRecognizedTrainningDataUrl { get; set; }

        public double FormRecognizedMinAccuracyAllowed { get; set; }
    }
}
