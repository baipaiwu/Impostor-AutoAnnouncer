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

            // 订阅玩家加入与游戏结束事件
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
                // 将消息广播到每个游戏实例 — 不同 Impostor 版本可能需要调整具体方法名
                foreach (var instance in _server.Instances)
                {
                    try
                    {
                        // 常见实现：实例上有向所有玩家发送聊天的方法，若不存在请按你当前版本替换
                        instance.SendChatToAll(message);
                    }
                    catch
                    {
                        // 回退到日志输出（便于排查）
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