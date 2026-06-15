# RimWorld Together - Complete Documentation Index

This project now includes comprehensive documentation of its architecture and codebase. Use this index to find the information you need.

## Documents Overview

### 1. QUICK_START.md
**Best for: Getting oriented quickly (5-minute read)**
- High-level architecture overview
- Three core layers explained
- Key files at a glance
- How players join and interact
- Common debugging tips
- Visual diagrams

### 2. ARCHITECTURE.md
**Best for: Deep understanding of system design (30-minute read)**
- Complete module breakdown
- Client-server protocol details
- All 29 packet types explained
- State synchronization patterns
- RimWorld integration points
- Network diagrams
- Security and validation approach

### 3. FILE_REFERENCE.md
**Best for: Finding specific code (reference document)**
- Line-by-line file descriptions
- Method signatures and purposes
- Configuration file formats
- Directory structure documentation
- Data persistence details
- Dependency diagrams

### 4. This File (DOCUMENTATION_INDEX.md)
**Best for: Navigation and quick lookup**
- Document references
- How to use the documentation
- Quick links to common topics

---

## Quick Topic Lookup

### Architecture & Design
- **Overall structure**: QUICK_START.md → Architecture at a Glance
- **System layers**: QUICK_START.md → Three Core Layers
- **Complete architecture**: ARCHITECTURE.md → Sections 1-2
- **Network diagram**: ARCHITECTURE.md → Section 7

### Networking & Communication
- **Protocol overview**: ARCHITECTURE.md → Section 2.1
- **Connection lifecycle**: ARCHITECTURE.md → Section 2.2
- **Packet types**: ARCHITECTURE.md → Section 2.3 (table of all 29 types)
- **Implementation details**: FILE_REFERENCE.md → ServerNetwork.cs

### State Synchronization
- **Sync patterns**: QUICK_START.md → Synchronization Patterns
- **Data categories**: ARCHITECTURE.md → Section 3.1
- **Sync mechanisms**: ARCHITECTURE.md → Section 3.2
- **Conflict handling**: ARCHITECTURE.md → Section 3.4
- **Cooldown system**: QUICK_START.md → Rate Limiting

### Game Integration
- **RimWorld integration**: ARCHITECTURE.md → Section 4
- **Mod metadata**: ARCHITECTURE.md → Section 4.1
- **Game loop integration**: ARCHITECTURE.md → Section 4.4
- **Harmony patches**: QUICK_START.md → Modding Integration

### File Organization
- **Directory structure**: FILE_REFERENCE.md → Server Directory
- **Core modules**: FILE_REFERENCE.md → Server Application Files
- **Configuration files**: FILE_REFERENCE.md → Configuration Files
- **Asset/data files**: FILE_REFERENCE.md → Asset Files

### Specific Systems
- **Chat system**: FILE_REFERENCE.md → PM_Chat.cs
- **Settlement management**: FILE_REFERENCE.md → PM_Settlement.cs
- **Authentication**: FILE_REFERENCE.md → PM_Login.cs
- **Trading**: FILE_REFERENCE.md → PM_Transfer.cs
- **Raiding**: FILE_REFERENCE.md → PM_Raid.cs
- **User management**: FILE_REFERENCE.md → UserManager.cs

### Server Operation
- **Startup sequence**: FILE_REFERENCE.md → Program.cs
- **Server configuration**: FILE_REFERENCE.md → Master.cs
- **Network operations**: FILE_REFERENCE.md → ServerNetwork.cs
- **Data persistence**: ARCHITECTURE.md → Section 3.1

### Security
- **Authentication**: ARCHITECTURE.md → Section 10
- **Authorization**: ARCHITECTURE.md → Section 10
- **Data validation**: ARCHITECTURE.md → Section 10
- **Admin system**: QUICK_START.md → Admin System

### Deployment
- **Server deployment**: ARCHITECTURE.md → Section 9
- **Client installation**: ARCHITECTURE.md → Section 9
- **Configuration**: ARCHITECTURE.md → Section 9
- **Docker support**: README.md

---

## Document Map by Section

### QUICK_START.md Structure
```
1. What is RimWorld Together?
2. Architecture at a Glance
3. Three Core Layers
4. Key Files to Know (table)
5. How a Player Joins
6. How World State Stays in Sync
7. Server Directory Structure
8. Synchronization Patterns (A, B, C)
9. Conflict Prevention
10. Admin System
11. Rate Limiting
12. Data Persistence
13. Modding Integration
14. Performance Considerations
15. Testing a Change
16. Common Debugging
17. Key Takeaways
```

### ARCHITECTURE.md Structure
```
1. Overall Architecture Overview
2. Client-Server Architecture & Communication Model
3. State Synchronization Between Clients
4. RimWorld MOD Integration
5. Key Classes & Files
6. Synchronization Mechanics Summary
7. Network Architecture Diagram
8. Key Features Implementation
9. Deployment & Configuration
10. Security & Validation
```

### FILE_REFERENCE.md Structure
```
1. Source Code Organization
   - Core Module
   - Hooks Module
   - Managers Module
   - Files Module
   - PacketManagers Module
   - Commands Module
   - Misc Module
2. Assembly/DLL Files
3. Game Content Files
4. Configuration Files
5. Asset Files
6. Solution & Project Files
7. Key File Dependencies
8. Server Flow Summary
```

---

## Reading Recommendations

### For Quick Overview (15 minutes)
1. Read QUICK_START.md entirely
2. Skim ARCHITECTURE.md → Sections 1-2

### For Implementation Understanding (45 minutes)
1. QUICK_START.md → Full read
2. ARCHITECTURE.md → Sections 1-4 (skim networking, focus on sync)
3. FILE_REFERENCE.md → Skim Core and Managers sections

### For Complete Mastery (2+ hours)
1. QUICK_START.md → Full read
2. ARCHITECTURE.md → Full read (all 10 sections)
3. FILE_REFERENCE.md → Full read
4. Return to source code with documentation as reference

### For Specific Task
1. **Adding a new packet type**: 
   - ARCHITECTURE.md 2.3, FILE_REFERENCE.md PacketManagers
   - See PM_Settlement.cs or PM_Chat.cs as template

2. **Understanding state sync for a feature**: 
   - ARCHITECTURE.md 3.2 (sync patterns)
   - QUICK_START.md (sync patterns)
   - FILE_REFERENCE.md (specific PM_* file)

3. **Adding admin commands**: 
   - FILE_REFERENCE.md → Commands Module
   - See CMD_*.cs files for examples

4. **Debugging a sync issue**: 
   - QUICK_START.md → Common Debugging
   - ARCHITECTURE.md → Section 10 (validation)
   - FILE_REFERENCE.md → Logs and Assets locations

---

## Key Concepts Explained in Each Doc

### QUICK_START.md
- **Concept**: Accessibility first
- **Detail Level**: High-level, conceptual
- **Best for**: Orientation, quick reference
- **Contains**: Diagrams, code snippets, debugging

### ARCHITECTURE.md
- **Concept**: Complete technical design
- **Detail Level**: Medium-deep, comprehensive
- **Best for**: Understanding design decisions
- **Contains**: Data structures, flow diagrams, all features

### FILE_REFERENCE.md
- **Concept**: Implementation details
- **Detail Level**: Deep, file-by-file
- **Best for**: Code navigation, implementation reference
- **Contains**: Line counts, method names, directory layouts

---

## Navigation Tips

### Use Ctrl+F (Find) To Search For:
- File names (e.g., "PM_Login")
- Method names (e.g., "SendPacketToAllClients")
- Key terms (e.g., "synchronization", "cooldown")
- Directory paths (e.g., "Assets/Settlements")

### Correlation Table

| You Want to Know | Go To |
|---|---|
| How TCP connection works | ARCHITECTURE.md 2.2 |
| What happens when player joins | FILE_REFERENCE.md Program.cs + PM_Login.cs |
| How settlements sync | QUICK_START.md Sync Example + FILE_REFERENCE.md PM_Settlement |
| Server directory structure | FILE_REFERENCE.md Asset Files |
| All packet types | ARCHITECTURE.md 2.3 (table) |
| How raids work | FILE_REFERENCE.md PM_Raid.cs |
| How trades work | FILE_REFERENCE.md PM_Transfer.cs |
| How RimWorld is integrated | ARCHITECTURE.md 4 |
| Where to add new commands | FILE_REFERENCE.md Commands Module |
| Security model | ARCHITECTURE.md 10 |

---

## Code References

When reading source code, refer to these documentation sections:

**ServerNetwork.cs** → FILE_REFERENCE.md ServerNetwork.cs (98 lines)
- Understand TCP listener pattern

**PM_Login.cs** → FILE_REFERENCE.md PM_Login.cs (102 lines)
- See authentication and registration flow

**PM_Settlement.cs** → ARCHITECTURE.md 3.2 Pattern 1 + FILE_REFERENCE.md PM_Settlement
- Understand broadcast synchronization

**PM_Synchronous.cs** → ARCHITECTURE.md 3.2 Pattern 2 + FILE_REFERENCE.md PM_Synchronous
- See P2P through server pattern

**PM_Chat.cs** → QUICK_START.md + FILE_REFERENCE.md PM_Chat
- Understand messaging and logging

**Program.cs** → FILE_REFERENCE.md Program.cs + ARCHITECTURE.md 4.4
- See full initialization sequence

**Master.cs** → FILE_REFERENCE.md Master.cs
- Understand configuration management

---

## Working With the Documentation

### Adding Information
If you add new features or make architectural changes:
1. Update QUICK_START.md (if user-facing)
2. Add to ARCHITECTURE.md section 5 (if system design)
3. Add to FILE_REFERENCE.md (file-level details)
4. Update this index if new sections added

### Keeping Current
- Documents generated from codebase exploration
- Update when code changes significantly
- Especially: new packet types, new managers, new features

### Cross-References
- QUICK_START.md → Best entry point
- ARCHITECTURE.md → Most comprehensive
- FILE_REFERENCE.md → Most detailed
- Cross-reference in headers (e.g., "See FILE_REFERENCE.md")

---

## Quick Stats

- **Server C# Files**: 71 source files
- **Packet Handlers**: 29 different types
- **Manager Classes**: 5 main managers
- **Server Commands**: 30+ commands
- **Configuration Files**: 8 config types
- **Key Data Files**: 8 persistent data types
- **Lines of Generated Docs**: 2000+

---

## Questions This Documentation Answers

- What is the overall architecture? (QUICK_START.md, ARCHITECTURE.md 1)
- How do clients communicate? (ARCHITECTURE.md 2, QUICK_START.md Networking)
- How is state kept in sync? (ARCHITECTURE.md 3, QUICK_START.md Sync patterns)
- How does it integrate with RimWorld? (ARCHITECTURE.md 4, QUICK_START.md Modding)
- What does each file do? (FILE_REFERENCE.md)
- How do I debug issues? (QUICK_START.md Common Debugging)
- What are the data structures? (ARCHITECTURE.md 3.1, FILE_REFERENCE.md)
- How does authentication work? (FILE_REFERENCE.md PM_Login.cs)
- How do I add a new feature? (FILE_REFERENCE.md, then examine similar PM_* file)
- What are the security considerations? (ARCHITECTURE.md 10)

---

## Final Notes

This documentation reflects the current state of the RimWorld Together codebase as of June 15, 2026.

- **3 comprehensive documents** provide 360-degree coverage
- **Progressive detail levels** suit different use cases
- **Cross-referenced** for easy navigation
- **Code-focused** with real examples and line numbers
- **Production-ready** structure for team collaboration

Start with QUICK_START.md, then dive deeper into ARCHITECTURE.md and FILE_REFERENCE.md as needed!

