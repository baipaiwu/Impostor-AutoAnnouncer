# AutoAnnouncer for Impostor.Server

这个插件会在玩家加入和游戏结束时自动发送公告。基于 Impostor.Server（Impostor API）实现，目标为 .NET 6.0。

功能
- 玩家进入时公告
- 游戏结束时公告
- 可配置的模板（config/announcements.json），支持占位符：{player}, {room}, {reason}, {time}

快速开始
1. 在 GitHub 上创建仓库并 push 本项目文件，或在本地创建仓库并 push（见下方命令示例）。
2. CI (GitHub Actions) 会在 push 到 main 时构建并上传 AutoAnnouncer.dll 为 artifact，或你可以在本地构建：
   - dotnet build --configuration Release
3. 将生成的 DLL（src/AutoAnnouncer/bin/Release/net6.0/AutoAnnouncer.dll）复制到你的 Impostor.Server 的插件文件夹（通常是 Impostor/Plugins）
4. 将 config/announcements.json 与 DLL 放在一起或���在服务器可读取的 config 目录中并编辑模板
5. 重启 Impostor.Server

配置
- config/announcements.json：示例在仓库内

CI
- .github/workflows/dotnet-build.yml 会在 push/pull_request 时构建并上传 artifact 名为 `AutoAnnouncer-dll`（包含 DLL）。

许可：MIT
