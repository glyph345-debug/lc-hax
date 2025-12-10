# Advanced Command Menu System

## Overview

The Advanced Command Menu System provides a sophisticated, draggable menu interface with enhanced navigation capabilities and comprehensive player-specific command execution. This system replaces and extends the basic CommandMenuMod with advanced features.

## Key Features

### 🎯 Draggable Windows
- **Title Bar Dragging**: Click and drag the menu title bar to reposition
- **Position Persistence**: Window position saved to PlayerPrefs and restored on menu reopen
- **Screen Bounds**: Menu stays within screen boundaries to prevent off-screen dragging

### ⌨️ Enhanced Numpad Navigation
- **Numpad 4**: Navigate to previous tab (wraps around)
- **Numpad 6**: Navigate to next tab (wraps around)
- **Numpad 8**: Navigate up within current menu/screen
- **Numpad 2**: Navigate down within current menu/screen
- **Numpad 5**: Select/execute highlighted item
- **Backspace**: Navigate back to previous screen

### 👥 Players Tab - Auto-Scanning Lobby Roster
- **Real-time Updates**: Automatically scans lobby each frame to detect player joins/leaves
- **Player Information**: Shows username, client ID, host status, and death status
- **Dynamic Selection**: Navigate through players with numpad 8/2, select with numpad 5
- **Comprehensive Commands**: Access to all player-specific commands upon selection

### 📋 Command Categories

#### Standard Categories (Numpad 4/6 Navigation)
1. **Teleportation**: Exit, Enter, Teleport, Void, Home, Mob, Random
2. **Combat**: Noise, Bomb, Bombard, Hate, Mask, Fatality, Poison, Stun, Kill
3. **Utilities**: Say, Translate, Buy, Grab
4. **World**: Block, Build, Suit, Visit, Spin, Explode
5. **Toggles**: God Mode, No Clip, Jump, Rapid Fire, Fake Death, Invisibility
6. **Game**: Start Game, End Game, Heal, Players, Clear
7. **Privileged**: Spawn Enemy, Credit, Land, Eject, Revive (Host Only)
8. **Players**: Special tab for player-specific commands

#### Player-Specific Commands (After Player Selection)
**Teleportation:**
- Teleport to player (/tp <player>)
- Teleport player to location (/tp <player> <x> <y> <z>)
- Teleport player to another player (/tp <player> <player>)
- Teleport to hell (/void <player>)
- Send to random location (/random <player>)

**Effects/Attacks:**
- Play virtual noise (/noise <player> <duration=30>)
- Bomb player (/bomb <player>)
- Bombard player (/bombard <player>)
- Lure enemies to player (/hate <player>)
- Spawn masked enemy (/mask <player?> <amount=1>)
- Kill player (/kill <player>)
- Kill player with animation (/fatality <player> <enemy>)
- Poison player (/poison <player> <damage> <duration> <delay=1>)
- Spoof server message (/say <player> <message>)

**Health/State:**
- Heal/revive player (/heal <player>)
- Fake their death (/fakedeath)
- Make invisible (/invis)
- Toggle god mode (/god)

**Building/Items:**
- Give suit (/suit <suit>)
- Buy item for them (/buy <item> <quantity=1>)
- Give/take credit (/credit <amount>)
- Grab scrap (/grab <item?>)

**Host Only:**
- Spawn enemies (/spawn <enemy> <player> <amount=1>)
- Eject player (/eject)
- Land ship (/land)
- Revive all (/revive)

### 🔧 Parameter Input Handling
- **Numeric Parameters**: Text fields with validation for damage, duration, amount, coordinates
- **Text Parameters**: Text input fields for messages, suit types, item names
- **Choice Parameters**: Selection menus for enemies, suits, items
- **Player Parameters**: Player selection for target/source player selection
- **Real-time Validation**: Input validation before command execution
- **Clear Display**: Parameter requirements shown before execution

### 🛡️ Host Privileges
- **Automatic Detection**: Checks if local player is host
- **Command Filtering**: Host-only commands only appear for hosts
- **Clear Indication**: Non-host users see "[HOST ONLY]" indicators
- **Privilege Enforcement**: Server-side validation through existing command system

## Technical Implementation

### Core Components

#### AdvancedCommandMenuMod.cs
- Main menu controller with window management
- Handles all navigation input and state management
- Draggable window with persistence
- Tab navigation system
- Integration with CommandExecutor

#### PlayersMenuScreen.cs
- Real-time lobby scanning
- Player list management
- Navigation between players
- Entry point to player-specific commands

#### PlayerCommandsMenuScreen.cs
- Comprehensive player command interface
- Parameter input handling
- Command categorization
- Execution context management

#### BaseMenuScreen.cs
- Base class for all menu screens
- Common navigation methods
- Screen stack management
- Back navigation functionality

### Input System Integration
- Enhanced InputListener with numpad 4/6 support
- Backspace navigation support
- Seamless integration with existing input system

### Command System Integration
- Uses CommandExecutor.ExecuteAsync for command execution
- Proper parameter passing and validation
- Error handling and status messaging
- Integration with existing command infrastructure

## Usage Instructions

### Opening the Menu
- Press **M** to toggle the advanced command menu

### Basic Navigation
- **Numpad 4**: Previous tab
- **Numpad 6**: Next tab
- **Numpad 8**: Navigate up
- **Numpad 2**: Navigate down
- **Numpad 5**: Select/execute
- **Backspace**: Go back/close current screen
- **Drag**: Click and drag title bar to move window

### Player Commands Workflow
1. Press **Numpad 6** until you reach "Players" tab
2. Press **Numpad 5** to enter Players menu
3. Use **Numpad 8/2** to select a player
4. Press **Numpad 5** to view player commands
5. Navigate through command categories and select a command
6. Enter required parameters if prompted
7. Press **Numpad 5** to execute

### Parameter Input
- **Text Fields**: Type directly into highlighted input field
- **Validation**: Invalid inputs will show error messages
- **Navigation**: Use standard numpad controls within parameter screens
- **Confirmation**: Press numpad 5 or click "Confirm" button

## Features Summary

✅ **Draggable Windows** with position persistence  
✅ **Numpad 4/6 Tab Navigation** across all categories  
✅ **Real-time Player Scanning** with lobby updates  
✅ **Comprehensive Player Commands** organized by category  
✅ **Parameter Input System** with validation  
✅ **Host-Only Command Filtering**  
✅ **Screen Stack Navigation** with back functionality  
✅ **Command Integration** with existing system  
✅ **Error Handling** and status messaging  
✅ **Responsive Design** with proper bounds checking  

The Advanced Command Menu System provides a professional, intuitive interface for executing complex commands with player-specific targeting and parameter input, significantly enhancing the cheat's usability and functionality.