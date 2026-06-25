namespace DiscordRPC.Models
{
    public class RpcProfile
    {
        public string ClientId { get; set; } = "";
        public string Details { get; set; } = "";
        public string State { get; set; } = "";

        // Timestamps
        public bool UseTimestamps
        {
            get; set;
        }
        public bool AsTimeRemaining
        {
            get; set;
        }
        public int TotalDurationMinutes { get; set; } = 60;

        // Assets
        public string LargeImageKey { get; set; } = "";
        public string LargeImageText { get; set; } = "";
        public string SmallImageKey { get; set; } = "";
        public string SmallImageText { get; set; } = "";

        // Party
        public string PartyId { get; set; } = "";
        public int PartyCurrentSize { get; set; } = 1;
        public int PartyMaxSize { get; set; } = 4;

        // Secrets
        public string JoinSecret { get; set; } = "";
        public string SpectateSecret { get; set; } = "";

        // Buttons
        public string Button1Label { get; set; } = "";
        public string Button1Url { get; set; } = "";
        public string Button2Label { get; set; } = "";
        public string Button2Url { get; set; } = "";
    }
}
