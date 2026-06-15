# RimWorld Together - File Reference Guide

## Source Code Organization

### Server Application Files (Source/Server/)

#### Core Module (Source/Server/Core/)
- **Program.cs** (124 lines)
  - Main entry point for GameServer.exe
  - Sets up paths for all server resources
  - Creates directory structure (Assets, Configs, Logs, Backups, etc.)
  - Loads configuration files in order
  - Initializes event system, command system, packet handlers
  - Starts TCP listener, backup manager, server browser
  - Runs main console command loop

- **Master.cs** (68 lines)
  - Static configuration holder for entire server
  - Path definitions:
    - Assets/ → MainPath/Assets
    - Configs/ → MainPath/Configs
    - Backups/ → MainPath/Backups
    - Logs/ → MainPath/Logs
    - Users/ → Assets/Users
    - Saves/ → Assets/Saves
    - Maps/ → Assets/Maps
    - Settlements/ → Assets/Settlements
    - Sites/ → Assets/Sites
    - Guilds/ → Assets/Guilds
    - Events/ → Assets/Events
    - WorldObjects/ → Assets/WorldObjects
  - Global references:
    - ServerConfig, WorldValues, ActionConfigs
    - DifficultyValues, StorytellerValues, ScenarioValues
    - BackupConfig, ModConfig, ChatConfig
    - LeaderboardFile, Whitelist

#### Hooks Module (Source/Server/Hooks/)

**TCPNetwork/ServerNetwork.cs** (98 lines)
- Static TCP listener implementation
- Delegates: OnReadPacket, OnDisconnect
- StartFeature() - Initializes TCP listener on configured IP:Port
- ListenForNewClients() - Accepts incoming TCP connections
- GetConnectedClients() - Returns array of connected ServerClient objects
- GetConnectedClientFromUsername() - Lookup by player name
- GetClientFromID() - Lookup by client ID
- SendPacketToAllClients() - Broadcast packet to all or exclude specific client

**ServerBrowser/ServerBrowserManager.cs** (113 lines)
- Manages public server listing feature
- Connects to master server for telemetry
- StartFeature() - Initiates server browser connection
- ConnectToServerBrowser() - TCP connection to master server
- GetPublicIP() - Queries external service for public IP
- Optional public/private server modes

**Shared/ServerPrinter.cs** (varies)
- Logging utilities
- CreateLogger() - Initializes logging system

#### Managers Module (Source/Server/Managers/)

**UserManager.cs** (155 lines)
- Player account management
- Public methods:
  - SendPlayerRecount() - Broadcast current player count
  - BanPlayerFromName() - Ban a player account
  - PardonPlayerFromName() - Remove ban from player
- Helper methods:
  - GetUserFile(), GetUserFileFromName()
  - GetAllUserFiles() - Load all player data
  - CheckIfUserIsConnected() - Verify player online
  - CheckIfUserExists() - Lookup by username
  - CheckIfUserAuthCorrect() - Validate password
  - CheckIfUserBanned() - Check ban status
  - CheckWhitelist() - Whitelist verification
  - GetUserStructuresTilesFromUsername() - Get player's settlements/sites

**GlobalDataManager.cs** (38 lines)
- Assembles world state snapshot for new player
- SendServerGlobalData() - Creates PKT_ServerGlobalData containing:
  - Admin status, faction membership
  - Action cooldown configs
  - All world objects
  - Visible settlements/sites (filtered by goodwill)
  - Difficulty, scenario, storyteller settings
  - Mod configurations
  - Event definitions
  - Roads and pollution data

**WhitelistManager.cs** (varies)
- Whitelist enforcement for server access
- Load/save whitelist configuration

**BackupManager.cs** (varies)
- Automated server and player backup system
- StartFeature() - Periodic backup routine
- BackupUser() - Create player backup
- Backup structure in Backups/Users/ and Backups/Servers/

**ResponseShortcutManager.cs** (varies)
- Error response packet generation
- SendIllegalPacket() - Unauthorized action response
- SendUnavailablePacket() - Resource unavailable response
- SendBreakPacket() - Break/abort action response

#### Files Module (Source/Server/Files/)

**FL_ServerConfig.cs** (35 lines)
- Server configuration file loader (JSON)
- Properties:
  - Name, Description
  - DiscordURL, SteamWorkshopURL
  - IP, Port (default 25555)
  - MaxPlayers (default 100)
  - Verbosity, DisplayChatInConsole
  - UseUPnP, SyncLocalSave
  - EnableServerBrowser, EnableServerTelemetry

#### PacketManagers Module (Source/Server/PacketManagers/)

Each file implements `PM_Base` and handles a specific packet type via `[HandlesPacket(PacketHeader.X)]` attribute.

**PM_Login.cs** (102 lines)
- Authentication and user registration
- Receive() → Checks user existence → LoginUser() or RegisterUser()
- LoginUser() - Credential validation, ban check, whitelist check, mod conflict check
- RegisterUser() - New user file creation
- PostLogin() - Send welcome data, save/world setup
- RemoveOldClientSessions() - Disconnect duplicate logins
- DenyConnectionWithReason() - Rejection with reason (Full, NoWorld, Mods, Version, Ban, Whitelist)

**PM_World.cs** (57 lines)
- World generation file management
- CheckIfWorldExists() - Verify world file
- RequireWorldFile() - Request client to send world
- SendWorld() - Send world to client
- ReceiveWorld() - Accept world upload (admin only)

**PM_Map.cs** (44 lines)
- Settlement map persistence
- SaveUserMap() - Store map file by tile, update leaderboard
- GetAllMaps() - List all map files
- CheckIfMapExists() - Verify map at tile
- GetMapFromTile() - Load map data

**PM_Settlement.cs** (187 lines)
- Settlement creation and removal
- AddSettlement() - Create new settlement, verify tile unique, broadcast to others
- RemoveSettlement() - Delete settlement, broadcast removal
- CheckIfTileIsInUse() - Verify tile has settlement
- GetSettlementFileFromTile() - Load settlement by tile
- GetSettlementFileFromUsername() - Load player's settlements
- GetAllSettlements() - List all settlements
- GetSettlementsFromGoodwill() - Filter by goodwill relationship

**PM_Site.cs** (varies)
- Raid site management similar to settlements

**PM_Synchronous.cs** (101 lines)
- Real-time player interactions (visits, trades)
- StepMode enum: Ask, Accept, Reject, Start, Action
- TryStartSynchronousSession() - Player A initiates, server finds Player B, sends Ask
- AcceptSynchronousSession() - Player B accepts, server links both players
- RejectSynchronousSession() - Player B rejects
- StartSynchronousSession() - Begin interaction
- RouteToManager() - Forward action packets to both players

**PM_Transfer.cs** (varies)
- Trading between settlements
- TransferThings() - Request items
- RejectTransfer() - Decline trade
- TransferThingsRebound() - Counter-offer
- AcceptReboundTransfer() / RejectReboundTransfer() - Negotiate

**PM_Caravan.cs** (61 lines)
- Trade caravan movement
- AddCaravan() - Place caravan on world
- RemoveCaravan() - Remove caravan
- MoveCaravan() - Update caravan position

**PM_Raid.cs** (50 lines)
- Raid mechanics
- SendRequestedMap() - Attacker requests defender's settlement map
- Validates map exists, sends map data

**PM_Chat.cs** (169+ lines)
- Chat messaging and console commands
- BroadcastChatMessage() - Send chat to all players
- SendConsoleMessage() - System message to specific player
- SendServerMessage() - Server notification
- BroadcastConsoleMessage() - System message to all
- BroadcastServerNotification() - Server-wide notification
- ExecuteChatCommand() - Process chat commands with semaphore locking
- WriteToLogs() - Thread-safe chat logging

**PM_Events.cs** (108 lines)
- Event/scenario management (admin only)
- SendEvent() - Send event from one settlement to another
- SetEvents() - Admin update to event definitions
- LoadAllEvents() - Load all event files

**PM_Goodwills.cs** (106 lines)
- Faction relationship tracking
- ChangeUserGoodwills() - Update relationship
- UpdateClientGoodwills() - Send relationship snapshot
- GetSettlementGoodwill() - Calculate relationship (Personal/Guild/Individual)
- GetSiteGoodwill() - Same for sites
- FindGoodwillFromUsername() - Lookup relationship value

**PM_Saves.cs** (105+ lines)
- Save file management
- CheckIfUserHasSave() - Verify save exists
- ResetClientSave() - Delete player save and structures
- SendSaveToClient() - Send player's save file
- ReceiveSaveFromClient() - Receive save file upload

**PM_Mods.cs** (106 lines)
- Mod configuration and validation
- SaveModConfig() - Update mod list (admin only)
- CheckIfModConflict() - Validate player's mods against server config
- Checks: required mods present, disallowed mods absent

**PM_Recount.cs** (varies)
- Player count broadcast

**PM_Leaderboard.cs** (varies)
- Leaderboard statistics updates

**PM_Information.cs** (varies)
- Server information queries

**PM_GlobalData.cs** (varies)
- Server-wide data package

**PM_WorldObject.cs** (varies)
- World map objects (roads, etc.)
- AddWorldObject(), RemoveWorldObject()
- GetAllWorldObjects()

**PM_Zoom.cs** (varies)
- Viewport/camera synchronization

**PM_Aid.cs** (varies)
- Trade aid system

**PM_Guilds.cs** (varies)
- Guild management

**PM_Pollution.cs** (varies)
- Pollution tracking

**PM_Roads.cs** (varies)
- Road network management

**PM_GameParameter.cs** (varies)
- Game parameter synchronization

**PM_Version.cs** (varies)
- Version compatibility checks

**PM_Transfers.cs** (varies)
- Item trading mechanics

**ServerBrowser/PM_Listing.cs**, **PM_Telemetry.cs**
- Server browser packet handlers

#### Commands Module (Source/Server/Commands/)

30+ command implementations, each handling a server console command.

Examples:
- **CMD_ForceSave.cs** - Save all open worlds
- **CMD_Ban.cs** - Ban player
- **CMD_Kick.cs** - Disconnect player
- **CMD_List.cs** - List connected players
- **CMD_Broadcast.cs** - Send server message to all
- **CMD_Help.cs** - Show available commands
- **CMD_Op.cs** - Make player admin
- **CMD_Deop.cs** - Remove admin from player

#### Misc Module (Source/Server/Misc/)

**InformationDisplayer.cs** (63 lines)
- Console output for various events
- DisplayConnect(), DisplayDisconnect()
- DisplayLogin(), DisplayRegister()
- DisplaySaveMap(), DisplaySetWorld()
- DisplayAddSettlement(), DisplayRemoveSettlement()
- etc.

**UPnP.cs** (varies)
- UPnP port forwarding setup

---

## Assembly/DLL Files (Source/Assemblies/)

These are precompiled and not available as source in this repository:

**RTClient.dll** (327 KB)
- Client-side multiplayer implementation
- GameClient.Patches.Tabs.ChatTab - Chat UI tab class
- Harmony patches for RimWorld engine integration
- Game loop hooks for save/load/action synchronization

**RTNetwork.dll** (44 KB)
- Low-level networking layer
- PacketHeader enum - All packet type identifiers
- Packet classes - PKT_Login, PKT_Map, etc.
- Serialization utilities with MessagePack
- NetworkRuleset - Callback-based packet routing

**RTShared.dll** (50 KB)
- Shared data structures between client and server
- FL_* classes:
  - FL_Player, FL_Settlement, FL_Site
  - FL_Map, FL_Caravan, FL_Guild
  - FL_Event, FL_WorldObject
  - FL_PlanetConfig
- Configuration classes:
  - FL_ServerConfig, FL_ModConfig
  - FL_DifficultyConfig, FL_ScenarioConfig
  - Cooldown and goodwill tracking
- Serialization helpers

---

## Game Content Files (1.5/ and 1.6/)

### Definitions (Defs/)

**Defs/MainButtonDefs/RTChat.xml**
- Defines Chat tab button
- References GameClient.Patches.Tabs.ChatTab
- Default hotkey: Backspace

**Defs/FactionDefs/**
- RTPlayerFaction.xml - Player settlements faction
- RTNeutralFaction.xml - Neutral relations
- RTEnemyFaction.xml - Enemy relations
- RTAllyFaction.xml - Allied relations

**Defs/ThingDefs/**
- TransferSpot.xml - Trade item drop location
- DefenseSpot.xml - Defense area
- ChillSpot.xml - Recreation/social spot

**Defs/WorldObjectDefs/RTCaravan.xml**
- Trade caravan world object definition

**Defs/SitePartDefs/SiteParts.xml**
- Raid site components

**Defs/SoundDefs/Chat.xml**
- Chat notification sounds

**Defs/MapGeneration/Empty.xml**
- Empty map generation rules

### Languages (Languages/)

Localization files for each supported language:
- ChineseSimplified, ChineseTraditional
- French, Spanish, Ukrainian, Hungarian, Russian
- Example (template for translation)

Each language folder contains DefInjected/ translations of:
- MainButtonDef/RTChat.xml
- FactionDef/* definitions
- ThingDef/* definitions
- SitePartDef/SiteParts.xml

---

## Configuration Files (Generated on Server Startup)

Located in Configs/ directory:

**ServerConfig.json**
- Server name and description
- IP and port binding
- Max players limit
- Logging/chat display settings
- UPnP configuration
- Server browser settings

**ActionConfig.json**
- Cooldown timings for actions:
  - Raids, events, world objects, roads

**ModConfig.json**
- List of required mods
- List of optional mods
- List of disallowed mods
- Bypass flag for admins

**DifficultyConfig.json**
- Game difficulty settings

**ScenarioConfig.json**
- Scenario parameters

**StorytellerConfig.json**
- Storyteller configuration

**WhitelistConfig.json**
- Whitelist enabled flag
- List of whitelisted usernames

**ChatConfig.json**
- Chat settings (disconnect notifications, etc.)

---

## Asset Files (Generated During Runtime)

Located in Assets/ directory:

**Assets/Users/**
- FL_Player.json files (one per player)
- Account data, progress, relationships

**Assets/Saves/**
- Player settlement save files (one per player)
- RimWorld save data for their settlement

**Assets/Maps/**
- Settlement map files (one per tile)
- Full map state for each settlement

**Assets/Settlements/**
- Settlement metadata files
- Location, owner, status

**Assets/Sites/**
- Raid site metadata files

**Assets/WorldObjects/**
- World object files (roads, events, etc.)

**Assets/Guilds/**
- Guild membership and settings

**Assets/Events/**
- Event definition files

**Assets/WorldValuesFile.json**
- World generation parameters
- Shared across all players
- Contains road network and pollution map

**Logs/**
- System logs in Logs/System/
- Chat logs in Logs/Chat/ (daily files)

**Backups/**
- User backups in Backups/Users/
- Server backups in Backups/Servers/

---

## Solution & Project Files

**Source/RimworldTogether.sln**
- Visual Studio solution file
- Contains:
  - GameClient.csproj (client - source not in repo)
  - GameServer.csproj (server - see above)

**Source/Server/GameServer.csproj**
- .NET 8.0 console application
- Multi-platform (Windows x64/x86, Linux ARM/ARM64)
- Dependencies:
  - MessagePack 3.1.7 (binary serialization)
  - Mono.Nat 3.0.4 (UPnP support)
  - Newtonsoft.Json 13.0.4 (JSON config)
  - System.Security.Permissions 10.0.9
- References:
  - RTNetwork.dll
  - RTShared.dll

---

## Key File Dependencies

```
Program.cs (entry point)
  ↓
  Master.cs (configuration holder)
  ↓
  ServerNetwork.cs (TCP listener)
  ↓
  PM_* classes (packet handlers)
  ├── PM_Login (initial connection)
  ├── PM_World (world setup)
  ├── PM_Settlement (sync world state)
  └── ... 26 other packet types
  ↓
  Manager classes
  ├── UserManager (player data)
  ├── GlobalDataManager (state assembly)
  ├── WhitelistManager (access control)
  └── BackupManager (persistence)
```

---

## Server Flow Summary

1. Program.cs → SetPaths() → CreateFolders() → LoadFiles()
2. Master.cs holds all configuration
3. ServerNetwork.StartFeature() opens TCP listener
4. Client connects → ListenForNewClients() creates ServerClient
5. Client sends PKT_Login → PM_Login.Receive()
6. PM_Login validates, loads FL_Player
7. GlobalDataManager.SendServerGlobalData() sends initial state
8. Client receives updates via PM_* handlers
9. All world changes broadcast via ServerNetwork.SendPacketToAllClients()
10. BackupManager saves state periodically
11. ServerBrowserManager publishes to public listing (optional)

