using System.Collections.Generic;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;

sealed class AdvancedCommandMenuMod : MonoBehaviour {
    bool MenuVisible { get; set; }
    int CurrentCategoryIndex { get; set; }
    int CurrentItemIndex { get; set; }

    // Draggable window support
    bool IsDragging { get; set; }
    Vector2 DragOffset { get; set; }
    Vector2 windowPosition = new(50, 50);
    public Vector2 WindowPosition { get => this.windowPosition; set => this.windowPosition = value; }
    const int WindowWidth = 600;
    const int WindowHeight = 700;

    // Menu state
    public MenuState CurrentState { get; set; } = MenuState.CategorySelection;
    public PlayerControllerB? SelectedPlayer { get; set; }
    public BaseMenuScreen? CurrentScreen { get; set; } = null;

    // Tab scrolling
    int TabScrollOffset { get; set; }
    const int MaxVisibleTabs = 5;

    // Command scrolling per category
    readonly Dictionary<int, Vector2> commandScrollPositions = [];
    const int CommandButtonHeight = 35;
    const int CommandViewportHeight = 300;

    // Screen management
    public readonly Stack<BaseMenuScreen> screenStack = new();

    public enum MenuState {
        CategorySelection,
        PlayerSelection,
        ParameterInput,
        PlayerCommandSelection
    }

    public struct CommandInfo {
        internal string name;
        internal string syntax;
        internal bool isPrivileged;
        internal string[] parameters;
        internal string[] parameterDescriptions;
    }

    struct CommandCategory {
        internal string name;
        internal CommandInfo[] commands;
    }

    enum PlayerCommandType {
        Teleportation,
        EffectsAttacks,
        HealthState,
        BuildingItems,
        HostOnly
    }

    void OnEnable() {
        Logger.Write("AdvancedCommandMenuMod: OnEnable called");
        InputListener.OnInsertPress += this.ToggleMenu;
        InputListener.OnMPress += this.ToggleMenu;
        InputListener.OnNumpad8Press += this.NavigateUp;
        InputListener.OnNumpad2Press += this.NavigateDown;
        InputListener.OnNumpad5Press += this.ExecuteSelected;
        InputListener.OnNumpad4Press += this.NavigatePreviousTab;
        InputListener.OnNumpad6Press += this.NavigateNextTab;
        InputListener.OnBackspacePress += this.HandleBackNavigation;

        InitializeCategories();
        this.LoadWindowPosition();
        Logger.Write("AdvancedCommandMenuMod: Subscribed to all events");
    }

    void OnDisable() {
        Logger.Write("AdvancedCommandMenuMod: OnDisable called");
        InputListener.OnInsertPress -= this.ToggleMenu;
        InputListener.OnMPress -= this.ToggleMenu;
        InputListener.OnNumpad8Press -= this.NavigateUp;
        InputListener.OnNumpad2Press -= this.NavigateDown;
        InputListener.OnNumpad5Press -= this.ExecuteSelected;
        InputListener.OnNumpad4Press -= this.NavigatePreviousTab;
        InputListener.OnNumpad6Press -= this.NavigateNextTab;
        InputListener.OnBackspacePress -= this.HandleBackNavigation;

        this.SaveWindowPosition();
    }

    static void InitializeCategories() {
        // Initialize the existing categories from the original CommandMenuMod
        // plus the new Players category
    }

    void ToggleMenu() {
        this.MenuVisible = !this.MenuVisible;
        if (this.MenuVisible) {
            this.CurrentState = MenuState.CategorySelection;
            this.CurrentCategoryIndex = 0;
            this.CurrentItemIndex = 0;
            this.TabScrollOffset = 0;
            this.screenStack.Clear();
            this.EnsureSelectedCommandVisible(this.CurrentCategoryIndex, GetCategory(this.CurrentCategoryIndex).commands.Length);
        }
        Logger.Write($"AdvancedCommandMenuMod: Menu toggled to {this.MenuVisible}");
    }

    void NavigatePreviousTab() {
        if (!this.MenuVisible) return;

        if (this.CurrentState == MenuState.CategorySelection) {
            this.CurrentCategoryIndex--;
            if (this.CurrentCategoryIndex < 0) {
                this.CurrentCategoryIndex = GetTotalCategories() - 1;
            }
            this.CurrentItemIndex = 0;
            this.UpdateTabScrollOffset();
            this.EnsureSelectedCommandVisible(this.CurrentCategoryIndex, GetCategory(this.CurrentCategoryIndex).commands.Length);
        }
    }

    void NavigateNextTab() {
        if (!this.MenuVisible) return;

        if (this.CurrentState == MenuState.CategorySelection) {
            this.CurrentCategoryIndex++;
            if (this.CurrentCategoryIndex >= GetTotalCategories()) {
                this.CurrentCategoryIndex = 0;
            }
            this.CurrentItemIndex = 0;
            this.UpdateTabScrollOffset();
            this.EnsureSelectedCommandVisible(this.CurrentCategoryIndex, GetCategory(this.CurrentCategoryIndex).commands.Length);
        }
    }

    void UpdateTabScrollOffset() {
        int totalCategories = GetTotalCategories();
        if (totalCategories <= MaxVisibleTabs) {
            this.TabScrollOffset = 0;
            return;
        }

        if (this.CurrentCategoryIndex < this.TabScrollOffset) {
            this.TabScrollOffset = this.CurrentCategoryIndex;
        }
        else if (this.CurrentCategoryIndex >= this.TabScrollOffset + MaxVisibleTabs) {
            this.TabScrollOffset = this.CurrentCategoryIndex - MaxVisibleTabs + 1;
        }
    }

    void EnsureSelectedCommandVisible(int categoryIndex, int commandCount) {
        if (commandCount == 0) return;

        if (!this.commandScrollPositions.TryGetValue(categoryIndex, out Vector2 scrollPos)) {
            scrollPos = Vector2.zero;
        }

        float selectedItemTop = this.CurrentItemIndex * CommandButtonHeight;
        float selectedItemBottom = selectedItemTop + CommandButtonHeight;
        float viewportTop = scrollPos.y;
        float viewportBottom = scrollPos.y + CommandViewportHeight;

        if (selectedItemTop < viewportTop) {
            scrollPos.y = selectedItemTop;
        }
        else if (selectedItemBottom > viewportBottom) {
            scrollPos.y = Mathf.Max(0, selectedItemBottom - CommandViewportHeight);
        }

        this.commandScrollPositions[categoryIndex] = scrollPos;
    }

    void HandleBackNavigation() {
        if (!this.MenuVisible) return;

        if (this.screenStack.Count > 0) {
            this.CurrentScreen = this.screenStack.Pop();
        }
        else {
            this.CurrentState = MenuState.CategorySelection;
            this.CurrentScreen = null;
        }
    }

    void NavigateUp() {
        if (!this.MenuVisible) return;

        switch (this.CurrentState) {
            case MenuState.CategorySelection:
                this.CurrentItemIndex--;
                if (this.CurrentCategoryIndex < GetTotalCategories() - 1) {
                    // Regular category navigation
                    CommandCategory category = GetCategory(this.CurrentCategoryIndex);
                    if (this.CurrentItemIndex < 0) {
                        this.CurrentItemIndex = category.commands.Length - 1;
                    }
                }
                this.EnsureSelectedCommandVisible(this.CurrentCategoryIndex, GetCategory(this.CurrentCategoryIndex).commands.Length);
                break;
            case MenuState.PlayerSelection:
                this.CurrentScreen?.NavigateUp();
                break;
            case MenuState.ParameterInput:
                this.CurrentScreen?.NavigateUp();
                break;
            case MenuState.PlayerCommandSelection:
                this.CurrentScreen?.NavigateUp();
                break;
            default:
                break;
        }
    }

    void NavigateDown() {
        if (!this.MenuVisible) return;

        switch (this.CurrentState) {
            case MenuState.CategorySelection:
                this.CurrentItemIndex++;
                if (this.CurrentCategoryIndex < GetTotalCategories() - 1) {
                    // Regular category navigation
                    CommandCategory category = GetCategory(this.CurrentCategoryIndex);
                    if (this.CurrentItemIndex >= category.commands.Length) {
                        this.CurrentItemIndex = 0;
                    }
                }
                this.EnsureSelectedCommandVisible(this.CurrentCategoryIndex, GetCategory(this.CurrentCategoryIndex).commands.Length);
                break;
            case MenuState.PlayerSelection:
                this.CurrentScreen?.NavigateDown();
                break;
            case MenuState.ParameterInput:
                this.CurrentScreen?.NavigateDown();
                break;
            case MenuState.PlayerCommandSelection:
                this.CurrentScreen?.NavigateDown();
                break;
            default:
                break;
        }
    }

    void ExecuteSelected() {
        if (!this.MenuVisible) return;

        switch (this.CurrentState) {
            case MenuState.CategorySelection:
                this.HandleCategorySelection();
                break;
            case MenuState.PlayerSelection:
            case MenuState.ParameterInput:
            case MenuState.PlayerCommandSelection:
                this.CurrentScreen?.ExecuteSelected();
                break;
            default:
                break;
        }
    }

    void HandleCategorySelection() {
        if (this.CurrentCategoryIndex == GetTotalCategories() - 1) {
            // Players tab - switch to player selection
            this.CurrentState = MenuState.PlayerSelection;
            this.screenStack.Clear();
            this.CurrentScreen = new PlayersMenuScreen(this);
        }
        else {
            // Regular category - check if command needs parameters
            CommandCategory category = GetCategory(this.CurrentCategoryIndex);
            if (this.CurrentItemIndex < 0 || this.CurrentItemIndex >= category.commands.Length) return;

            CommandInfo command = category.commands[this.CurrentItemIndex];

            bool isHost = Helper.LocalPlayer?.IsHost ?? false;
            if (command.isPrivileged && !isHost) {
                TeleportationMenuManager.StatusMessage = "Command requires host privileges!";
                return;
            }

            if (command.parameters != null && command.parameters.Length > 0) {
                // Show parameter input screen
                this.CurrentState = MenuState.ParameterInput;
                this.CurrentScreen = new ParameterInputMenuScreen(
                    this,
                    command.name,
                    command.syntax,
                    command.parameters,
                    command.parameterDescriptions
                );
            }
            else {
                // Execute command directly
                ExecuteRegularCommand(command, []);
            }
        }
    }

    internal static async void ExecuteRegularCommand(CommandInfo command, string[] parameters) {
        TeleportationMenuManager.StatusMessage = $"Executing {command.name}...";

        CommandResult result = await CommandExecutor.ExecuteAsync(command.syntax, new Arguments { Span = parameters }, CommandInvocationSource.Direct);

        if (result.Success) {
            TeleportationMenuManager.StatusMessage = $"{command.name} executed successfully!";
        }
        else {
            TeleportationMenuManager.StatusMessage = result.Message ?? $"Failed to execute {command.name}";
            if (result.Message is not null) {
                Chat.Print(result.Message);
            }
        }

        // Clear message after 3 seconds
        _ = Task.Run(async () => {
            await Task.Delay(3000);
            if (TeleportationMenuManager.StatusMessage?.Contains(command.name) ?? false) {
                TeleportationMenuManager.StatusMessage = null;
            }
        });
    }

    internal static async void ExecuteRegularCommandWithParams(string commandName, string commandSyntax, string[] parameters) {
        TeleportationMenuManager.StatusMessage = $"Executing {commandName}...";

        CommandResult result = await CommandExecutor.ExecuteAsync(commandSyntax, new Arguments { Span = parameters }, CommandInvocationSource.Direct);

        if (result.Success) {
            TeleportationMenuManager.StatusMessage = $"{commandName} executed successfully!";
        }
        else {
            TeleportationMenuManager.StatusMessage = result.Message ?? $"Failed to execute {commandName}";
            if (result.Message is not null) {
                Chat.Print(result.Message);
            }
        }

        // Clear message after 3 seconds
        _ = Task.Run(async () => {
            await Task.Delay(3000);
            if (TeleportationMenuManager.StatusMessage?.Contains(commandName) ?? false) {
                TeleportationMenuManager.StatusMessage = null;
            }
        });
    }

    static int GetTotalCategories() => GetCategories().Length;

    static CommandCategory[] GetCategories() {
        return [
            new CommandCategory {
                name = "Teleportation",
                commands = [
                    new CommandInfo { name = "Exit", syntax = "exit" },
                    new CommandInfo { name = "Enter", syntax = "enter" },
                    new CommandInfo { name = "Teleport", syntax = "tp" },
                    new CommandInfo { name = "Void", syntax = "void" },
                    new CommandInfo { name = "Home", syntax = "home" },
                    new CommandInfo { name = "Mob", syntax = "mob" },
                    new CommandInfo { name = "Random", syntax = "random" },
                ]
            },
            new CommandCategory {
                name = "Combat",
                commands = [
                    new CommandInfo { name = "Noise", syntax = "noise", parameters = ["player", "duration"], parameterDescriptions = ["Player name", "Duration in seconds (optional, default 30)"] },
                    new CommandInfo { name = "Bomb", syntax = "bomb", parameters = ["player"], parameterDescriptions = ["Player name"] },
                    new CommandInfo { name = "Bombard", syntax = "bombard", parameters = ["player"], parameterDescriptions = ["Player name"] },
                    new CommandInfo { name = "Hate", syntax = "hate", parameters = ["player"], parameterDescriptions = ["Player name"] },
                    new CommandInfo { name = "Mask", syntax = "mask", parameters = ["player", "amount"], parameterDescriptions = ["Player name", "Amount (optional, default 1)"] },
                    new CommandInfo { name = "Fatality", syntax = "fatality", parameters = ["player", "enemy"], parameterDescriptions = ["Player name", "Enemy type"] },
                    new CommandInfo { name = "Poison", syntax = "poison", parameters = ["player", "damage", "duration", "delay"], parameterDescriptions = ["Player name", "Damage per tick", "Duration in seconds", "Delay in seconds"] },
                    new CommandInfo { name = "Stun", syntax = "stun", parameters = ["player", "duration"], parameterDescriptions = ["Player name", "Duration in seconds"] },
                    new CommandInfo { name = "Kill", syntax = "kill", parameters = ["player"], parameterDescriptions = ["Player name"] },
                ]
            },
            new CommandCategory {
                name = "Utilities",
                commands = [
                    new CommandInfo { name = "Say", syntax = "say", parameters = ["player", "message"], parameterDescriptions = ["Player name", "Message to send"] },
                    new CommandInfo { name = "Translate", syntax = "translate", parameters = ["language", "message"], parameterDescriptions = ["Language code", "Message to translate"] },
                    new CommandInfo { name = "Buy", syntax = "buy", parameters = ["item", "quantity"], parameterDescriptions = ["Item name", "Quantity (optional)"] },
                    new CommandInfo { name = "Grab", syntax = "grab", parameters = ["item"], parameterDescriptions = ["Item name (optional)"] },
                ]
            },
            new CommandCategory {
                name = "World",
                commands = [
                    new CommandInfo { name = "Block", syntax = "block" },
                    new CommandInfo { name = "Build", syntax = "build", parameters = ["unlockable"], parameterDescriptions = ["Unlockable name"] },
                    new CommandInfo { name = "Suit", syntax = "suit", parameters = ["suit"], parameterDescriptions = ["Suit type"] },
                    new CommandInfo { name = "Visit", syntax = "visit", parameters = ["moon"], parameterDescriptions = ["Moon name"] },
                    new CommandInfo { name = "Spin", syntax = "spin", parameters = ["duration"], parameterDescriptions = ["Duration in seconds"] },
                    new CommandInfo { name = "Horn", syntax = "horn", parameters = ["duration"], parameterDescriptions = ["Duration in seconds"] },
                    new CommandInfo { name = "Explode", syntax = "explode" },
                ]
            },
            new CommandCategory {
                name = "Toggles",
                commands = [
                    new CommandInfo { name = "God Mode", syntax = "god" },
                    new CommandInfo { name = "No Clip", syntax = "noclip" },
                    new CommandInfo { name = "Jump", syntax = "jump" },
                    new CommandInfo { name = "Rapid Fire", syntax = "rapid" },
                    new CommandInfo { name = "Fake Death", syntax = "fakedeath" },
                    new CommandInfo { name = "Invisibility", syntax = "invis" },
                ]
            },
            new CommandCategory {
                name = "Game",
                commands = [
                    new CommandInfo { name = "Start Game", syntax = "start" },
                    new CommandInfo { name = "End Game", syntax = "end" },
                    new CommandInfo { name = "Heal", syntax = "heal", parameters = ["player"], parameterDescriptions = ["Player name"] },
                    new CommandInfo { name = "XP", syntax = "xp", parameters = ["amount"], parameterDescriptions = ["XP amount (number)"] },
                    new CommandInfo { name = "Shovel", syntax = "shovel", parameters = ["force"], parameterDescriptions = ["Force value (optional, default 1)"] },
                    new CommandInfo { name = "Clear", syntax = "clear" },
                ]
            },
            new CommandCategory {
                name = "Privileged",
                commands = [
                    new CommandInfo { name = "Spawn Enemy", syntax = "spawn", parameters = ["enemy", "amount"], parameterDescriptions = ["Enemy type", "Amount (number)"], isPrivileged = true },
                    new CommandInfo { name = "Credit", syntax = "credit", parameters = ["amount"], parameterDescriptions = ["Credit amount (number)"], isPrivileged = true },
                    new CommandInfo { name = "Quota", syntax = "quota", parameters = ["amount", "fulfilled"], parameterDescriptions = ["Quota amount (number)", "Fulfilled amount (optional, default 0)"], isPrivileged = true },
                    new CommandInfo { name = "Timescale", syntax = "timescale", parameters = ["scale"], parameterDescriptions = ["Time scale value (1.0 = normal)"], isPrivileged = true },
                    new CommandInfo { name = "Land", syntax = "land", isPrivileged = true },
                    new CommandInfo { name = "Eject", syntax = "eject", parameters = ["player"], parameterDescriptions = ["Player name"], isPrivileged = true },
                    new CommandInfo { name = "Revive", syntax = "revive", isPrivileged = true },
                ]
            },
            new CommandCategory {
                name = "Players",
                commands = [] // Special handling for Players tab
            }
        ];
    }

    static CommandCategory GetCategory(int index) => GetCategories()[index];

    void LoadWindowPosition() {
        Vector2 position = this.WindowPosition;
        position.x = PlayerPrefs.GetFloat("AdvancedMenu_X", 50);
        position.y = PlayerPrefs.GetFloat("AdvancedMenu_Y", 50);
        this.WindowPosition = position;
    }

    void SaveWindowPosition() {
        PlayerPrefs.SetFloat("AdvancedMenu_X", this.WindowPosition.x);
        PlayerPrefs.SetFloat("AdvancedMenu_Y", this.WindowPosition.y);
        PlayerPrefs.Save();
    }

    void KeepWindowInBounds() {
        Vector2 position = this.WindowPosition;
        position.x = Mathf.Clamp(position.x, 0, Screen.width - WindowWidth);
        position.y = Mathf.Clamp(position.y, 0, Screen.height - WindowHeight);
        this.WindowPosition = position;
    }

    void HandleDragging() {
        if (Input.GetMouseButtonDown(0)) {
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y; // Flip Y coordinate

            Rect titleBarRect = new(this.WindowPosition.x, this.WindowPosition.y, WindowWidth, 30);
            if (titleBarRect.Contains(mousePos)) {
                this.IsDragging = true;
                this.DragOffset = mousePos - this.WindowPosition;
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            this.IsDragging = false;
        }

        if (this.IsDragging) {
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y; // Flip Y coordinate
            Vector2 position = mousePos - this.DragOffset;
            this.WindowPosition = position;
            this.KeepWindowInBounds();
        }
    }

    void OnGUI() {
        if (!this.MenuVisible) return;

        Logger.Write("AdvancedCommandMenuMod: Drawing menu");
        this.HandleDragging();

        GUI.Box(new Rect(this.WindowPosition.x, this.WindowPosition.y, WindowWidth, WindowHeight), GUIContent.none);

        GUILayout.BeginArea(new Rect(this.WindowPosition.x, this.WindowPosition.y, WindowWidth, WindowHeight));

        // Title bar for dragging
        GUILayout.BeginHorizontal();
        GUILayout.Label("Advanced Command Menu", GUILayout.Height(30));
        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20))) {
            this.ToggleMenu();
        }
        GUILayout.EndHorizontal();

        this.DrawContent();

        GUILayout.EndArea();
    }

    void DrawContent() {
        switch (this.CurrentState) {
            case MenuState.CategorySelection:
                this.DrawCategorySelection();
                break;
            case MenuState.PlayerSelection:
            case MenuState.ParameterInput:
            case MenuState.PlayerCommandSelection:
                this.CurrentScreen?.Draw();
                break;
            default:
                break;
        }
    }

    void DrawCategorySelection() {
        CommandCategory[] categories = GetCategories();
        int totalCategories = categories.Length;

        // Draw category tabs with improved scrolling support
        GUILayout.BeginHorizontal();

        // Calculate available width for tabs
        float availableWidth = WindowWidth - 20;
        float tabWidth = Mathf.Max(70, availableWidth / Mathf.Min(totalCategories, MaxVisibleTabs));

        // Show scroll indicator if needed
        if (totalCategories > MaxVisibleTabs && this.TabScrollOffset > 0) {
            if (GUILayout.Button("◀", GUILayout.Width(30), GUILayout.Height(30))) {
                this.TabScrollOffset = Mathf.Max(0, this.TabScrollOffset - 1);
            }
        }

        // Draw visible tabs
        int endIndex = Mathf.Min(this.TabScrollOffset + MaxVisibleTabs, totalCategories);
        for (int i = this.TabScrollOffset; i < endIndex; i++) {
            GUIStyle tabStyle = new(GUI.skin.button);
            if (i == this.CurrentCategoryIndex) {
                tabStyle.normal.background = GUI.skin.box.normal.background;
                GUI.backgroundColor = Color.green;
            }

            if (GUILayout.Button(categories[i].name, tabStyle, GUILayout.Width(tabWidth), GUILayout.Height(30))) {
                this.CurrentCategoryIndex = i;
                this.CurrentItemIndex = 0;
            }

            GUI.backgroundColor = Color.white;
        }

        // Show scroll indicator if needed
        if (totalCategories > MaxVisibleTabs && this.TabScrollOffset + MaxVisibleTabs < totalCategories) {
            if (GUILayout.Button("▶", GUILayout.Width(30), GUILayout.Height(30))) {
                this.TabScrollOffset = Mathf.Min(totalCategories - MaxVisibleTabs, this.TabScrollOffset + 1);
            }
        }

        GUILayout.EndHorizontal();

        // Show current tab indicator
        if (totalCategories > MaxVisibleTabs) {
            GUILayout.Label($"Tab {this.CurrentCategoryIndex + 1}/{totalCategories}: {categories[this.CurrentCategoryIndex].name}", GUI.skin.label);
        }

        GUILayout.Space(10);

        // Draw commands for current category
        if (this.CurrentCategoryIndex == categories.Length - 1) {
            // Players tab - show message to press numpad 5
            GUILayout.Label("Players Tab - Press Numpad 5 to view all players in lobby", GUI.skin.box, GUILayout.Height(50));
            GUILayout.Space(10);
            GUILayout.Label("Select players to perform player-specific actions", GUI.skin.label);
        }
        else {
            CommandCategory currentCategory = categories[this.CurrentCategoryIndex];
            this.DrawCommandsForCategory(currentCategory);
        }

        GUILayout.Space(10);

        // Show status message if available
        if (!string.IsNullOrEmpty(TeleportationMenuManager.StatusMessage)) {
            GUILayout.Label(TeleportationMenuManager.StatusMessage, GUI.skin.box, GUILayout.Height(30));
        }

        GUILayout.Label("Numpad: 4=Prev Tab, 6=Next Tab, 8=Up, 2=Down, 5=Select, M=Close", GUI.skin.label);
    }

    void DrawCommandsForCategory(CommandCategory category) {
        GUILayout.Label(category.name, GUI.skin.box);

        bool isHost = Helper.LocalPlayer?.IsHost ?? false;

        if (!this.commandScrollPositions.TryGetValue(this.CurrentCategoryIndex, out Vector2 scrollPos)) {
            scrollPos = Vector2.zero;
        }

        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(CommandViewportHeight));
        this.commandScrollPositions[this.CurrentCategoryIndex] = scrollPos;

        for (int i = 0; i < category.commands.Length; i++) {
            CommandInfo cmd = category.commands[i];

            if (i == this.CurrentItemIndex) {
                GUI.color = Color.green;
                GUI.backgroundColor = Color.green;
            }

            string buttonText = cmd.name;
            if (cmd.parameters != null && cmd.parameters.Length > 0) {
                buttonText += " [→]";
            }
            if (cmd.isPrivileged && !isHost) {
                buttonText += " [HOST ONLY]";
                GUI.color = Color.gray;
                GUI.enabled = false;
            }

            if (GUILayout.Button(buttonText, GUILayout.Height(CommandButtonHeight), GUILayout.ExpandWidth(true))) {
                this.CurrentItemIndex = i;
                this.EnsureSelectedCommandVisible(this.CurrentCategoryIndex, category.commands.Length);
                if (!cmd.isPrivileged || isHost) {
                    this.HandleCategorySelection();
                }
            }

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        GUILayout.EndScrollView();
    }
}
