# AutoAnnouncer for Impostor.Server

杩欎釜鎻掍欢浼氬湪鐜╁鍔犲叆鍜屾父鎴忕粨鏉熸椂鑷姩鍙戦€佸叕鍛娿€傚熀浜?Impostor.Server锛圛mpostor API锛夊疄鐜帮紝鐩爣涓?.NET 6.0銆?
鍔熻兘
- 鐜╁杩涘叆鏃跺叕鍛?- 娓告垙缁撴潫鏃跺叕鍛?- 鍙厤缃殑妯℃澘锛坈onfig/announcements.json锛夛紝鏀寔鍗犱綅绗︼細{player}, {room}, {reason}, {time}

蹇€熷紑濮?1. 鍦?GitHub 涓婂垱寤轰粨搴撳苟 push 鏈」鐩枃浠讹紝鎴栧湪鏈湴鍒涘缓浠撳簱骞?push锛堣涓嬫柟鍛戒护绀轰緥锛夈€?2. CI (GitHub Actions) 浼氬湪 push 鍒?main 鏃舵瀯寤哄苟涓婁紶 AutoAnnouncer.dll 涓?artifact锛屾垨浣犲彲浠ュ湪鏈湴鏋勫缓锛?   - dotnet build --configuration Release
3. 灏嗙敓鎴愮殑 DLL锛坰rc/AutoAnnouncer/bin/Release/net6.0/AutoAnnouncer.dll锛夊鍒跺埌浣犵殑 Impostor.Server 鐨勬彃浠舵枃浠跺す锛堥€氬父鏄?Impostor/Plugins锛?4. 灏?config/announcements.json 涓?DLL 鏀惧湪涓€璧锋垨鏀惧湪鏈嶅姟鍣ㄥ彲璇诲彇鐨?config 鐩綍涓苟缂栬緫妯℃澘
5. 閲嶅惎 Impostor.Server

閰嶇疆
- config/announcements.json锛氱ず渚嬪湪浠撳簱鍐?
CI
- .github/workflows/dotnet-build.yml 浼氬湪 push/pull_request 鏃舵瀯寤哄苟涓婁紶 artifact 鍚嶄负 `AutoAnnouncer-dll`锛堝寘鍚?DLL锛夈€?
璁稿彲锛歁IT
