using Rocket.API;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Commands;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace AutoMaxPlayers
{
    public class AutoMaxPlayers : RocketPlugin<AutoMaxPlayersConfiguration>
    {
        private Timer checkTimer;
        private int currentMaxPlayers = 0;

        protected override void Load()
        {
            Logger.Log("[AutoMaxPlayers] Plugin yüklendi.");

            checkTimer = new Timer(10000); // 10 saniyede bir kontrol
            checkTimer.Elapsed += CheckPlayerCount;
            checkTimer.Start();
        }

        protected override void Unload()
        {
            checkTimer?.Stop();
            Logger.Log("[AutoMaxPlayers] Plugin devre dışı bırakıldı.");
        }

        private void CheckPlayerCount(object sender, ElapsedEventArgs e)
        {
            try
            {
                int playerCount = Provider.clients.Count;
                int targetMaxPlayers = GetMaxPlayersForCount(playerCount, currentMaxPlayers);

                if (targetMaxPlayers != currentMaxPlayers)
                {
                    currentMaxPlayers = targetMaxPlayers;

                    string command = $"maxplayers {currentMaxPlayers}";
                    R.Commands.Execute(null, command); // console üzerinden gönderim
                    Logger.Log($"[AutoMaxPlayers] Oyuncu sayısı {playerCount}, komut gönderildi: {command}");

                    SendDiscordNotification(playerCount, currentMaxPlayers);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[AutoMaxPlayers] Hata: {ex.Message}");
            }
        }

        private int GetMaxPlayersForCount(int count, int current)
        {
            int maxPlayers = Configuration.Instance.DefaultMaxPlayers;

            // Yükseltme Mantığı
            foreach (var threshold in Configuration.Instance.SlotThresholds)
            {
                if (count >= threshold.PlayerCount)
                {
                    maxPlayers = threshold.MaxPlayers;
                }
            }

            // Düşürme Mantığı
            if (current == 48 && count < 28)
                maxPlayers = 36;
            else if (current == 36 && count < 10)
                maxPlayers = Configuration.Instance.DefaultMaxPlayers;

            return maxPlayers;
        }

        private async void SendDiscordNotification(int playerCount, int maxPlayers)
        {
            try
            {
                if (string.IsNullOrEmpty(Configuration.Instance.DiscordWebhookUrl))
                    return;

                string message = Configuration.Instance.DiscordMessageFormat
                    .Replace("{playerCount}", playerCount.ToString())
                    .Replace("{maxPlayers}", maxPlayers.ToString());

                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                        { "content", message }
                    };

                    var content = new FormUrlEncodedContent(values);
                    await client.PostAsync(Configuration.Instance.DiscordWebhookUrl, content);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[AutoMaxPlayers] Discord mesajı gönderilemedi: {ex.Message}");
            }
        }
    }

    public class SlotThreshold
    {
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
    }
}
