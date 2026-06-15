# RimWorld Together 架构分析

## 1. 整体架构

RimWorld Together 是一个 RimWorld 多人 Mod，采用 **Client-Server 架构** 同步多个玩家的游戏状态。

| 组件 | 技术栈 | 说明 |
|------|--------|------|
| **Server** | C# .NET 8.0 | `Source/Server/`，独立可执行程序，71 个源文件 |
| **Client** | C# DLL + Harmony Patch | `Source/Assemblies/RTClient.dll`（327KB），作为 RimWorld mod 注入 |
| **Network** | TCP + 自定义包协议 | `RTNetwork.dll`（44KB），底层通信层 |
| **Shared** | 共享数据结构 | `RTShared.dll`（50KB），MessagePack 序列化约定 |
| **Game Defs** | XML | `1.5/` `1.6/` 中的 FactionDef、ThingDef、WorldObjectDef 等 |

### 目录结构

```
Source/
├── Server/
│   ├── Core/             # Master.cs（全局配置/路径）、Program.cs（入口）
│   ├── Hooks/TCPNetwork/ # ServerNetwork.cs（TCP Listener、连接管理）
│   ├── Managers/         # UserManager、GlobalDataManager、BackupManager、WhitelistManager
│   ├── PacketManagers/   # 26 个包管理器，各处理一种 PacketHeader
│   ├── Commands/         # 服务器控制台命令
│   ├── Files/            # 配置文件加载器
│   └── Misc/             # InformationDisplayer、UPnP
└── Assemblies/
    ├── RTClient.dll
    ├── RTNetwork.dll
    └── RTShared.dll
1.5/ & 1.6/               # 版本特定的 RimWorld mod 内容（Defs、Languages）
About/                     # Mod 元数据
LoadFolders.xml            # RimWorld 版本加载配置
```

### 网络协议

- **传输层**: TCP，默认端口 25555，绑定 0.0.0.0
- **序列化**: MessagePack 二进制格式（网络传输）+ JSON（文件持久化）
- **包路由**: `ServerNetwork.cs` 中 `TcpListener` 接受连接，每个 `ServerClient` 设 `NetworkRuleset` 回调（`OnReadPacket` / `OnDisconnect`）
- **包分发**: `HandlesPacket(PacketHeader.X)` 特性标记处理器方法，通过反射调用

---

## 2. 客户端状态同步机制

状态同步有 **三种模式** + **四种策略**。

### 2.1 三种同步模式

| 模式 | 数据流 | 典型场景 |
|------|--------|----------|
| **Broadcast** | Server → 所有 Client | Chat、Settlement 增删、Site、Caravan、WorldObject、Event、PlayerRecount |
| **P2P 经 Server 中转** | A ↔ Server ↔ B | 实时同屏访问（Synchronous）、物品交易（Transfer） |
| **Lazy / On-Demand** | Client 请求 → Server 单点响应 | Raid 地图加载、Goodwill 按需查询 |

### 2.2 四种同步策略

#### 策略一：登入全量同步（InitSync）

客户端登录后，`PM_Login.PostLogin()` → `GlobalDataManager.SendServerGlobalData()` 发送 `PKT_ServerGlobalData`，包含世界完整快照：

```
PKT_ServerGlobalData:
  IsClientAdmin          // 管理员权限
  IsClientFactionMember  // Guild 成员状态
  ActionValues           // 各动作冷却配置（Raid/Event/Road 等）
  RoadValues             // 道路建造规则
  WorldObjects           // 所有世界物件
  PlayerSettlements      // 所有定居点（已计算该客户端 Goodwill）
  PlayerSites            // 所有 Raid 据点（已计算 Goodwill）
  ScenarioValues         // 场景参数
  DifficultyValues       // 难度设置
  StorytellerValues      // 叙事者设置
  ModConfigs             // Mod 要求
  EventValues            // 可用事件列表
  Roads                  // 已建道路（来自 PlanetConfig）
  PollutedTiles          // 污染地块
```

#### 策略二：增量 Broadcast

以 `PM_Settlements.AddSettlement()` 为例：

```
1. 检查 Tile 是否被占用 → CheckIfTileIsInUse()
2. 创建 FL_Settlement JSON 文件写入 Assets/Settlements/
3. 遍历所有在线玩家（排除操作者）：
   a. 为该玩家计算 Goodwill
   b. EnqueuePacket(PacketHeader.Settlement, data)
```

同步一致性：Server 先写文件（持久化），再逐一广播给在线客户端。

#### 策略三：P2P 多步握手

**实时同屏访问（PM_Synchronous）**：

```
A: Ask    → Server 查找目标 Settlement 在线情况 → 转发给 B
B: Accept → Server 将双方 SynchronousClientID 互绑 → 通知 A
双方 Start → Action 包由 RouteToManager 直接互相转发（双向直通）
```

**物品交易（PM_Transfers）**：

```
A: TradeRequest → Server 查目标在线 → 转发给 B
B: TradeAccept / TradeReject → Server 返给 A
A: TradeReRequest（还价）→ Server 转发给 B
B: TradeReAccept / TradeReReject → Server 返给 A
目标离线时任何步骤返回 StepMode.Recover
```

#### 策略四：Lazy 按需拉取

**Raid（PM_Raid）**：

```
A 请求 Raid tile X → 检查冷却（FL_PlayerCooldown.CheckIfCanRaid）
  → 检查 Assets/Maps/{tile}.json 是否存在
  → 从磁盘读取完整地图数据 → 单点返回给 A（不广播给其他玩家）
  → 设置 Raid 冷却计时器
```

### 2.3 冲突解决策略

Server 采用**预防式**冲突处理，而非事后合并：

| 策略 | 实现位置 | 说明 |
|------|----------|------|
| **Tile 占用检查** | `PM_Settlements.CheckIfTileIsInUse()` | Settlement/Caravan 创建前扫文件系统验证唯一性 |
| **冷却系统** | `FL_PlayerCooldown` | Raid/Event/Road/WorldObject 等操作频率限制，服务端强校验 |
| **在线检查** | `UserManagerH.CheckIfUserIsConnected()` | P2P 操作前验证目标在线，离线返回 `Recover` 状态码 |
| **所有权验证** | `PM_Settlements.RemoveSettlement()` | 只有 Settlement 所有者才能删除自己的定居点 |
| **Guild/Goodwill 规则** | `PM_Goodwills` | 同 Guild 成员 Goodwill.Guild（不可互伤），Goodwill 按客户端独立计算 |
| **首个玩家自动 Admin** | `PM_Login.PostLogin()` | 无世界时首个加入者拿到管理员权限 |
| **Admin 特权** | 各处 `IsAdmin` 判断 | 可设 World/Events/Mods，可绕过 Mod 限制 |
| **旧 Session 清理** | `PM_Login.RemoveOldClientSessions()` | 同用户名重复登入时强制踢掉旧连接 |

---

## 3. 服务端状态持久化

所有共享状态以 JSON 文件形式存储在 Server 的 `Assets/` 目录：

| 目录 | 数据类型 | FL_ 类 | 同步方式 |
|------|----------|--------|----------|
| `Users/` | 玩家账号 | `FL_Player` | 登入加载，含 Admin/Guild/冷却计时器/Goodwill 关系 |
| `Settlements/` | 世界定居点 | `FL_Settlement` | Broadcast（增删时写文件后通知所有在线玩家） |
| `Sites/` | Raid 据点 | `FL_Site` | Broadcast |
| `Maps/` | 定居点内部地图 | `FL_Map` | Client Upload + Lazy 拉取（Raid 时按需返回） |
| `WorldObjects/` | 世界物件 | `FL_WorldObject` | Broadcast |
| `Caravans/` | 贸易商队 | `FL_Caravan` | Broadcast |
| `Events/` | 场景事件 | `FL_Event` | Broadcast（仅 Admin 可管理） |
| `Guilds/` | 公会 | `FL_Guild` | Broadcast |
| `Configs/` | 服务端配置 | PlanetConfig 等 | JSON 配置，Server 启动时加载 |
| `Saves/` | 玩家存档 | save 文件 | 请求-响应，不影响其他玩家 |

### 数据一致性原则

- **Server 是唯一真理源（Source of Truth）**：所有写操作经过 Server
- **先持久化再广播**：`SerializeToFile()` → `SendPacketToAllClients()`
- **无 P2P 直传文件**：所有数据传输以 Server 为中介
- **冷却系统防竞态**：高频操作有时间锁

---

## 4. 通信流程

### 4.1 连接生命周期

```
1. Client 通过 TcpClient 连接 → ServerNetwork.ListenForNewClients()
2. 创建 ServerClient 实例，检查连接限制：
   - MaxPlayers（可配，默认 100）
   - World 不存在时只允许一个 Client（首个玩家创建世界）
3. NetworkRuleset 绑定回调：
   - OnReadPacket → PacketHeader 分发到对应的 PM_* 处理器
   - OnDisconnect → 清理 + 广播 PlayerRecount + 退出通知
```

### 4.2 登录流程

```
1. Client 发送 PKT_Login（username + password）
2. Server 检查 UserManagerH.CheckIfUserExists()
   - 新用户 → RegisterUser（创建 FL_Player) → LoginUser
   - 已有用户 → 验证密码（CheckIfUserAuthCorrect）
3. LoginUser 中的检查链：
   - CheckIfUserBanned → 封禁检查
   - CheckWhitelist → 白名单检查
   - CheckIfModConflict → Mod 兼容性检查
   - RemoveOldClientSessions → 踢掉旧连接
4. PostLogin：
   - UserManager.SendPlayerRecount() → 广播当前玩家数
   - GlobalDataManager.SendServerGlobalData() → 全量世界快照
   - PM_Chat.SendLoginChatMessages() → 登录欢迎/公告
   - 有世界：PM_Saves.SendSaveToClient() 或 PM_World.SendWorld()
   - 无世界：PM_World.RequireWorldFile() + 授予 Admin
```

---

## 5. RimWorld 引擎集成

### 5.1 Mod 加载

- **About.xml**: Package ID `nova.rimworldtogether`，依赖 Harmony
- **LoadFolders.xml**: 按版本（1.5/1.6）加载对应内容
- **版本支持**: RimWorld 1.5 和 1.6

### 5.2 Harmony Patch 注入

Client（RTClient.dll）通过 Harmony 补丁挂钩 RimWorld 引擎：
- 游戏 Tick 事件处理
- Save/Load 文件集成
- UI（Chat 标签页、多人交互界面）
- Settlement/Site 变更拦截和同步
- Map 数据序列化上传

### 5.3 XML 定义扩展

| 类型 | 定义 | 用途 |
|------|------|------|
| MainButtonDef | Chat | 底部工具栏添加聊天标签，Backspace 快捷键 |
| FactionDef | RTPlayerFaction / RTNeutralFaction / RTEnemyFaction / RTAllyFaction | 多人派系关系系统 |
| ThingDef | RTTransferSpot / RTDefenseSpot / RTChillSpot | 交易投放点 / 防御点 / 社交点 |
| WorldObjectDef | RTCaravan | 世界地图上的贸易商队 |
| SitePartDef | Raid 相关结构 | Raid 据点的内容定义 |

---

## 6. 包类型总览

| PacketHeader | 处理器 | 方向 | 说明 |
|-------------|--------|------|------|
| Login | PM_Login | Req-Resp | 登录/注册认证 |
| GlobalData | GlobalDataManager | Server→Client | 登入时全量世界快照 |
| Settlement | PM_Settlements | Broadcast | 定居点增删 |
| Site | PM_Sites | Broadcast | Raid 据点管理 |
| World | PM_World | Req-Resp | 世界文件上传/下载 |
| Map | PM_Map | Upload | 定居点地图上传 |
| Save | PM_Saves | Bidirectional | 存档上传/下载 |
| Synchronous | PM_Synchronous | P2P 中转 | 实时同屏访问 |
| Transfer | PM_Transfers | P2P 中转 | 物品交易（多步握手） |
| Raid | PM_Raid | Lazy | Raid 地图按需加载 |
| Caravan | PM_Caravan | Broadcast | 商队移动 |
| Chat | PM_Chat | Broadcast | 聊天消息/命令 |
| WorldObject | PM_WorldObject | Broadcast | 世界物件（道路等） |
| GoodWill | PM_Goodwills | Lazy | 派系关系查询 |
| Events | PM_Events | Admin | 事件管理 |
| Guilds | PM_Guilds | Broadcast | 公会管理 |
| Mods | PM_Mods | Broadcast | Mod 配置验证 |
| Version | PM_Version | Validation | 版本兼容性检查 |
| PlayerRecount | PM_Recount | Broadcast | 在线人数变动 |
| Aid | PM_Aid | P2P | 援助发放 |
| Zoom | PM_Zoom | P2P | 视口同步 |
| Leaderboard | PM_Leaderboard | Req-Resp | 排行榜统计 |
| GameParameter | PM_GameParameter | Admin | 游戏参数下发 |
| Pollution | PM_Pollution | Broadcast | 污染地图同步 |
| Roads | PM_Roads | Broadcast | 道路建造/更新 |
| Information | PM_Information | Server→Client | 通知信息 |

---

## 7. 服务器生命周期

### 启动（Program.cs）

```
1. 加载配置（ServerConfig、ActionConfig、ModConfig 等 JSON）
2. 从文件加载 Events
3. 注册控制台命令（CMD_Base 通过反射扫描）
4. 缓存包处理器（PM_Base.PacketDictionary，反射绑定 HandlesPacket）
5. 启动 TCP Listener → ServerNetwork.StartFeature()
6. 后台任务：BackupManager（定时备份）、ServerBrowserManager（公共服务器列表）
```

### 运行时

```
while (true) CMD_Base.ListenForCommands();  // 主线程处理控制台命令
```

网络操作（包收发、连接管理）在独立的 Task 线程中运行。

### 部署

- **独立执行文件**: .NET 8.0 可执行程序，跨平台（Windows/Linux/ARM）
- **Docker**: `ghcr.io/rimworld-together/rimworld-together:latest`，挂载 `RWTData` 目录
- **Client**: Steam Workshop 订阅，需 Harmony 依赖