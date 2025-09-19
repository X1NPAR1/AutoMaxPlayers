using Rocket.API;
using System.Collections.Generic;

namespace AutoMaxPlayers
{
    public class AutoMaxPlayersConfiguration : IRocketPluginConfiguration
    {
        public int DefaultMaxPlayers { get; set; }
        public List<SlotThreshold> SlotThresholds { get; set; }
        public string DiscordWebhookUrl { get; set; }
        public string DiscordMessageFormat { get; set; }

        public void LoadDefaults()
        {
            DefaultMaxPlayers = 24;

            SlotThresholds = new List<SlotThreshold>
            {
                new SlotThreshold { PlayerCount = 20, MaxPlayers = 36 },
                new SlotThreshold { PlayerCount = 32, MaxPlayers = 48 }
            };

            DiscordWebhookUrl = "https://discord.com/api/webhooks/WEBHOOK_ID/WEBHOOK_TOKEN";
            DiscordMessageFormat = "@here 📢 Oyuncu sayısı {playerCount} oldu! Slotlar {maxPlayers} olarak güncellendi.";
        }
    }
}
