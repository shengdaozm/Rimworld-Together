# RimWorld Together - Quick Start Guide

## Understanding the Codebase in 5 Minutes

### What is RimWorld Together?
A multiplayer mod that synchronizes game state across multiple players via a central server.

### Architecture at a Glance
```
RimWorld Game (Client) <--TCP:25555--> GameServer.exe (Central Server) <---> RimWorld Game (Client 2)
     + RTClient.dll
     + RTNetwork.dll
     + RTShared.dll
```

### Three Core Layers

**1. NETWORKING LAYER (RTNetwork.dll)**
- TCP-based packet communication
- Serializes data with MessagePack
- Message types: Login, Map, Settlement, Chat, Transfer, Raid, etc.
- Pattern: Client sends packet → Server receives → Handler processes → Response sent back

**2. STATE SYNCHRONIZATION (Server/PacketManagers/)**
- 29 packet handlers process different game actions
- File-based persistence in Assets/ directory
- Three sync patterns:
  - **Broadcast**: Everyone sees it (chat, settlements added)
  - **P2P Through Server**: Two players interact (trading, raiding)
  - **Lazy Load**: Only requested (raid maps on demand)

**3. GAME INTEGRATION (RTClient.dll + Defs/)**
- Harmony patches hook into RimWorld game engine
- XML definitions add UI, factions, objects to the game
- Cooldowns prevent action spam
- Goodwill tracks relationships between players

### Key Files to Know

| File | Purpose | Lines |
|------|---------|-------|
| Program.cs | Start server, init everything | 124 |
| Master.cs | Hold all configuration | 68 |
| ServerNetwork.cs | TCP listener and packet router | 98 |
| PM_Login.cs | Authenticate users | 102 |
| PM_Settlement.cs | Manage world settlements | 187 |
| PM_Synchronous.cs | Real-time player interactions | 101 |
| PM_Chat.cs | Messages and commands | 169 |
| UserManager.cs | Player account management | 155 |
| GlobalDataManager.cs | Assemble world state snapshot | 38 |

### How a Player Joins

1. **Connection**: Client TCP connects to server:25555
2. **Login**: Sends PKT_Login with username/password
3. **Validation**: Server checks credentials, ban list, mod compatibility
4. **Init**: Server sends PKT_ServerGlobalData (full world state)
5. **Ready**: Client can now interact with world

### How World State Stays in Sync

**On Settlement Creation:**
```
Player A places settlement on tile 500
  ↓
Client sends PKT_Settlement to server
  ↓
Server saves settlement file to Assets/Settlements/500.json
  ↓
Server broadcasts PKT_Settlement to ALL other clients
  ↓
All players' worlds updated with new settlement
```

**On Settlement Trade:**
```
Player A wants to trade with Player B
  ↓
PM_Transfers finds Player B on server (or returns error)
  ↓
Server sends negotiation packets between both players
  ↓
Trade accepted → Item transfer packets exchanged
  ↓
Both players' settlement saves updated
```

### Server Directory Structure

```
Assets/
├── Users/           ← Player account files (FL_Player.json)
├── Saves/           ← Settlement save files (one per player)
├── Maps/            ← Settlement maps (one per tile)
├── Settlements/     ← Settlement metadata
├── Sites/           ← Raid site metadata
├── WorldObjects/    ← Roads, events, etc.
└── WorldValuesFile.json  ← Shared world generation data

Configs/            ← JSON configuration files
├── ServerConfig.json
├── ModConfig.json
├── ActionConfig.json
└── ... (difficulty, scenario, storyteller)

Logs/
├── System/          ← Server operation logs
└── Chat/            ← Daily chat logs

Backups/            ← Periodic backup archives
```

### Synchronization Patterns

**Pattern A: Broadcast (Chat, Settlements, Caravans)**
```csharp
ServerNetwork.SendPacketToAllClients(PacketHeader.Chat, chatData);
// All players receive the update immediately
```

**Pattern B: Peer-to-Peer (Trades, Real-time interactions)**
```csharp
ServerClient clientA = ServerNetwork.GetConnectedClientFromUsername("Alice");
ServerClient clientB = ServerNetwork.GetConnectedClientFromUsername("Bob");
clientA.Listener.EnqueuePacket(header, data);  // Send to Alice
clientB.Listener.EnqueuePacket(header, data);  // Send to Bob
// Both see the same action
```

**Pattern C: On-Demand (Raid maps)**
```csharp
FL_Map map = PM_Map.GetMapFromTile(targetTile);
// Send ONLY to the requesting player
client.Listener.EnqueuePacket(PacketHeader.Raid, raidData);
```

### Conflict Prevention

No lock-based conflict resolution. Instead, conflicts prevented upfront:

```csharp
// Check if tile already occupied before adding settlement
if (CheckIfTileIsInUse(tile)) 
    return error;  // Can't build there!

// Check if player online before sending trade
if (!UserManagerH.CheckIfUserIsConnected(username))
    return error;  // Player offline, cancel trade

// Guild members can't raid each other
if (guild.HasMember(otherPlayer))
    return error;  // Can't raid guild member!
```

### Admin System

**First player auto-promoted:**
```csharp
if (!PM_World.CheckIfWorldExists()) {
    client.GetData<FL_Player>().UpdateAdmin(true);
    // First player becomes admin automatically
}
```

**Admin can:**
- Set world (upload world generation)
- Set mods (change required/optional mod list)
- Set events (create event definitions)
- Bypass mod restrictions
- Reset players
- Ban/unban players
- Force server saves

### Rate Limiting

**Cooldown System** - Server-enforced action delays:
```csharp
// Client tries to raid
if (!FL_PlayerCooldown.CheckIfCanRaid(player, cooldownConfig))
    return "You can raid again in X seconds";

// After successful raid
player.Cooldowns.SetRaidTimer(player);  // Start cooldown
```

Prevents spam of:
- Raids
- Events
- World objects
- Road building

### Data Persistence

All data stored as JSON files, two serialization methods:

1. **MessagePack** - Binary format for network transmission (efficient)
2. **JSON** - Human-readable format for file storage (debuggable)

Example: Player data flow
```
RimWorld Game
  ↓ (MessagePack)
Network packet
  ↓
ServerNetwork TCP
  ↓
PM_Login receives
  ↓ (JSON)
Assets/Users/PlayerName.json
```

### Modding Integration

**Harmony patches** (in RTClient.dll) intercept RimWorld code:
- Game save/load → Upload/download to server
- Settlement changes → Send to server
- Map modifications → Sync across network

**XML Definitions** (in 1.5/ and 1.6/):
- Chat tab button (MainButtonDef)
- Multiplayer factions (FactionDef)
- Transfer/defense spots (ThingDef)
- Raid site types (SitePartDef)

### Performance Considerations

1. **File-based storage** - Simple but slower than databases
   - OK for small servers (< 50 players)
   - Each settlement is a separate file
   
2. **Semaphore locking** - Thread-safe chat logging
   - Prevents concurrent write corruption
   - Slight performance hit but ensures data integrity

3. **No database** - Pure file system
   - Trivial to backup/restore
   - No DB dependency
   - Easy to understand and debug

### Testing a Change

1. Modify server C# code (Source/Server/)
2. Rebuild GameServer.csproj
3. Run GameServer.exe
4. Connect RimWorld client with mod
5. Check Assets/ directory for saved state
6. Check Logs/ for debug information

### Common Debugging

**"Server won't start"**
- Check port 25555 not in use
- Check assets/config directories created
- Check JSON config files valid
- Check UPnP not causing port binding issues

**"Client can't join"**
- Check server IP/port correct
- Check client mod installed (RTClient.dll)
- Check version compatibility
- Check mods match (if not admin)

**"World not synced"**
- Check Assets/Settlements/ for settlement files
- Check Assets/Maps/ for map data
- Check Assets/WorldValuesFile.json exists
- Check Logs/System/ for error messages

**"Trade failed"**
- Check both players online
- Check target player tile has settlement
- Check tile exists in Assets/Settlements/
- Check neither player banned

### Key Takeaways

1. **Client-Server Model**: Single authoritative server, multiple thin clients
2. **Packet-Based**: All communication via discrete message types
3. **File Persistence**: JSON-based storage, simple and debuggable
4. **Broadcast on Mutation**: World changes immediately propagated to relevant players
5. **Conflict Prevention**: Validation before action, not locking after
6. **Harmony Patching**: Integrates seamlessly with RimWorld engine
7. **Extensible**: 29 packet handlers for different game features

