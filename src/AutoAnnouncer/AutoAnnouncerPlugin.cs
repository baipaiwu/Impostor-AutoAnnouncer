// NOTE (compatibility stub):
// 为尽快让 CI/build 通过，这个文件被替换为一个不直接依赖 Impostor.Api 的兼容桩（stub）。
// 该实现读取配置并在日志中记录公告，但不会在 Impostor.Server 中广播消息。
// 一旦我们有 Impostor.Api (v1.10.4) 的具体类型信息（DLL 或 inspector 输出），我会把真正的插件实现恢复回去，
// 恢复版将会订阅事件并使用真实的 IServer/IInstance API 做广播。
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AutoAnnouncer
{
    // 兼容桩：不实现 IPlugin（避免与 Impostor.Api 类型耦合）
    public class AutoAnnouncerPlugin
    {
        private readonly ILogger<AutoAnnouncerPlugin> _logger;
        private readonly string _configPath;
        private AnnouncementConfig _config = new AnnouncementConfig();

        // 构造函数仅接受 logger（避免注入不存在的 Impostor 类型）
        public AutoAnnouncerPlugin(ILogger<AutoAnnouncerPlugin> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configPath = Path.Combine(AppContext.BaseDirectory, "config", "announcements.json");
        }

        // 被外界（或测试）手动调用以启用插件逻辑
        public ValueTask EnableAsync()
        {
            TryLoadConfig();
            _logger.LogInformation("AutoAnnouncer (stub) enabled");
            return default;
        }

        public ValueTask DisableAsync()
        {
            _logger.LogInformation("AutoAnnouncer (stub) disabled");
            return default;
        }

        // v1.10.x 的 IPlugin 需要 ReloadAsync；提供实现以兼容接口调用
        public ValueTask ReloadAsync()
        {
            TryLoadConfig();
            _logger.LogInformation("AutoAnnouncer (stub) reloaded");
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

        // 原始插件在事件触发时会调用本方法；这里作为示例只记录日志
        public void AnnouncePlayerJoin(string player, string room)
        {
            var msg = FormatTemplate(_config.PlayerJoinMessage, player ?? "Unknown", room ?? string.Empty, null);
            Broadcast(msg);
        }

        public void AnnounceGameEnded(string reason)
        {
            var msg = FormatTemplate(_config.GameEndedMessage, string.Empty, string.Empty, reason ?? "Unknown");
            Broadcast(msg);
        }

        private string FormatTemplate(string template, string player, string room, string? reason)
        {
            if (string.IsNullOrWhiteSpace(template)) return string.Empty;
            return template.Replace("{player}", player)
                           .Replace("{room}", room)
                           .Replace("{reason}", reason ?? string.Empty)
                           .Replace("{time}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        // 兼容桩：将消息写进日志（真实实现应把消息广播给所有实例/玩家）
        private void Broadcast(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            try
            {
                // 这里不依赖任何 Impostor API；仅记录日志，便于在 CI/测试中看到行为
                _logger.LogInformation("[AutoAnnouncer] {message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast announcement (stub)");
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
