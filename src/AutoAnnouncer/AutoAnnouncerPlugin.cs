using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Game;
using Impostor.Api.Plugins;
using Impostor.Api.Instances;

namespace AutoAnnouncer
{
    [Plugin("com.baipaiwu.autoannouncer", "AutoAnnouncer", "1.0.0")]
    public class AutoAnnouncerPlugin : IPlugin
    {
        private readonly IEventManager _events;
        private readonly IServer _server;
        private readonly ILogger<AutoAnnouncerPlugin> _logger;
        private readonly string _configPath;
        private AnnouncementConfig _config = new AnnouncementConfig();

        public AutoAnnouncerPlugin(IEventManager events, IServer server, ILogger<AutoAnnouncerPlugin> logger)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _configPath = Path.Combine(AppContext.BaseDirectory, "config", "announcements.json");
        }

        public ValueTask EnableAsync()
        {
            TryLoadConfig();

            // 订阅事件（签名与命名空间已基于 Impostor.Api v1.10.x）
            _events.PlayerJoined += OnPlayerJoined;
            _events.GameEnded += OnGameEnded;

            _logger.LogInformation("AutoAnnouncer enabled");
            return default;
        }

        public ValueTask DisableAsync()
        {
            _events.PlayerJoined -= OnPlayerJoined;
            _events.GameEnded -= OnGameEnded;

            _logger.LogInformation("AutoAnnouncer disabled");
            return default;
        }

        // v1.10.x 要求实现 ReloadAsync
        public ValueTask ReloadAsync()
        {
            TryLoadConfig();
            _logger.LogInformation("AutoAnnouncer reloaded");
            return default;
        }

        private void TryLoadConfig()
        {
            try
            {
                var dir = Path.GetDirectoryName(_configPath) ?? Path.Combine(AppContext.BaseDirectory, "config");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                if (!File.Exists(_configPath))
                {
                    var defaultJson = JsonSerializer.Serialize(AnnouncementConfig.CreateDefault(), new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_configPath, defaultJson);
                    _logger.LogInformation("Wrote default config to {path}", _configPath);
                }

                var json = File.ReadAllText(_configPath);
                _config = JsonSerializer.Deserialize<AnnouncementConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? AnnouncementConfig.CreateDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load announcements config, using defaults");
                _config = AnnouncementConfig.CreateDefault();
            }
        }

        private Task OnPlayerJoined(object? sender, PlayerJoinedEventArgs args)
        {
            try
            {
                var player = args.Player;
                var playerName = player?.Data?.Name ?? "Unknown";
                var room = player?.Server?.Name ?? string.Empty;
                var msg = FormatTemplate(_config.PlayerJoinMessage, playerName, room, null);
                Broadcast(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling PlayerJoined");
            }

            return Task.CompletedTask;
        }

        private Task OnGameEnded(object? sender, GameEndedEventArgs args)
        {
            try
            {
                var reason = args.Reason?.ToString() ?? "Unknown";
                var msg = FormatTemplate(_config.GameEndedMessage, string.Empty, string.Empty, reason);
                Broadcast(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GameEnded");
            }

            return Task.CompletedTask;
        }

        private string FormatTemplate(string template, string player, string room, string? reason)
        {
            if (string.IsNullOrWhiteSpace(template)) return string.Empty;
            return template.Replace("{player}", player)
                           .Replace("{room}", room)
                           .Replace("{reason}", reason ?? string.Empty)
                           .Replace("{time}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private void Broadcast(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            try
            {
                // 广播到每个实例；不同版本 API 名称可能不同，这里使用常见方法 SendChatToAll（若你的版本方法名不同请告诉我）
                foreach (var instance in _server.Instances)
                {
                    try
                    {
                        // 若编译通过但运行时提示方法不存在，请告诉我你的 Impostor.Api 中 IInstance 的实际方法名
                        instance.SendChatToAll(message);
                    }
                    catch
                    {
                        _logger.LogInformation("Announcement for instance {id}: {message}", instance.Id, message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast announcement");
            }
        }
    }

    public class AnnouncementConfig
    {
        public string PlayerJoinMessage { get; set; } = "欢迎 {player} 加入游戏！";
        public string GameEndedMessage { get; set; } = "游戏结束！原因：{reason}";

        public static AnnouncementConfig CreateDefault() => new AnnouncementConfig();
    }
}
