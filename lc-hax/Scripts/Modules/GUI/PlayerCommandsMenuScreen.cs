using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;

sealed class PlayerCommandsMenuScreen(AdvancedCommandMenuMod menu, PlayerControllerB targetPlayer) : BaseMenuScreen(menu) {
    PlayerControllerB TargetPlayer { get; } = targetPlayer;
    int SelectedCommandIndex { get; set; }

    // Parameter input state
    string[] ParameterValues { get; set; } = new string[10]; // Max parameters
    bool IsAwaitingInput { get; set; }
    int CurrentParameterIndex { get; set; }

    enum CommandCategory {
        Teleportation,
        EffectsAttacks,
        HealthState,
        BuildingItems,
        HostOnly
    }

    struct PlayerCommand {
        internal string name;
        internal string syntax;
        internal CommandCategory category;
        internal string[] parameters;
        internal bool requiresHost;
    }

    PlayerCommand[] Commands { get; } = [
        // TELEPORTATION
        new PlayerCommand { name = "Teleport to player", syntax = "tp", category = CommandCategory.Teleportation, parameters = ["targetPlayer"] },
        new PlayerCommand { name = "Teleport player to location", syntax = "tp", category = CommandCategory.Teleportation, parameters = ["player", "x", "y", "z"] },
        new PlayerCommand { name = "Teleport player to another player", syntax = "tp", category = CommandCategory.Teleportation, parameters = ["sourcePlayer", "targetPlayer"] },
        new PlayerCommand { name = "Teleport to hell", syntax = "void", category = CommandCategory.Teleportation, parameters = ["player"] },
        new PlayerCommand { name = "Send to random location", syntax = "random", category = CommandCategory.Teleportation, parameters = ["player"] },
        
        // EFFECTS/ATTACKS
        new PlayerCommand { name = "Play virtual noise", syntax = "noise", category = CommandCategory.EffectsAttacks, parameters = ["player", "duration"] },
        new PlayerCommand { name = "Bomb player", syntax = "bomb", category = CommandCategory.EffectsAttacks, parameters = ["player"] },
        new PlayerCommand { name = "Bombard player", syntax = "bombard", category = CommandCategory.EffectsAttacks, parameters = ["player"] },
        new PlayerCommand { name = "Lure enemies to player", syntax = "hate", category = CommandCategory.EffectsAttacks, parameters = ["player"] },
        new PlayerCommand { name = "Spawn masked enemy", syntax = "mask", category = CommandCategory.EffectsAttacks, parameters = ["player", "amount"] },
        new PlayerCommand { name = "Kill player", syntax = "kill", category = CommandCategory.EffectsAttacks, parameters = ["player"] },
        new PlayerCommand { name = "Kill player with animation", syntax = "fatality", category = CommandCategory.EffectsAttacks, parameters = ["player", "enemy"] },
        new PlayerCommand { name = "Poison player", syntax = "poison", category = CommandCategory.EffectsAttacks, parameters = ["player", "damage", "duration", "delay"] },
        new PlayerCommand { name = "Spoof server message", syntax = "say", category = CommandCategory.EffectsAttacks, parameters = ["player", "message"] },
        
        // HEALTH/STATE
        new PlayerCommand { name = "Heal/revive player", syntax = "heal", category = CommandCategory.HealthState, parameters = ["player"] },
        new PlayerCommand { name = "Fake their death", syntax = "fakedeath", category = CommandCategory.HealthState, parameters = ["player"] },
        new PlayerCommand { name = "Make invisible", syntax = "invis", category = CommandCategory.HealthState, parameters = ["player"] },
        new PlayerCommand { name = "Toggle god mode", syntax = "god", category = CommandCategory.HealthState, parameters = ["player"] },
        
        // BUILDING/ITEMS
        new PlayerCommand { name = "Give suit", syntax = "suit", category = CommandCategory.BuildingItems, parameters = ["suit"] },
        new PlayerCommand { name = "Buy item for them", syntax = "buy", category = CommandCategory.BuildingItems, parameters = ["item", "quantity"] },
        new PlayerCommand { name = "Give/take credit", syntax = "credit", category = CommandCategory.BuildingItems, parameters = ["amount"] },
        new PlayerCommand { name = "Grab scrap", syntax = "grab", category = CommandCategory.BuildingItems, parameters = ["item"] },
        
        // HOST ONLY
        new PlayerCommand { name = "Spawn enemies", syntax = "spawn", category = CommandCategory.HostOnly, parameters = ["enemy", "player", "amount"], requiresHost = true },
        new PlayerCommand { name = "Eject player", syntax = "eject", category = CommandCategory.HostOnly, parameters = ["player"], requiresHost = true },
        new PlayerCommand { name = "Land ship", syntax = "land", category = CommandCategory.HostOnly, parameters = ["player"], requiresHost = true },
        new PlayerCommand { name = "Revive all", syntax = "revive", category = CommandCategory.HostOnly, parameters = ["player"], requiresHost = true }
    ];

    public override void NavigateUp() {
        if (this.IsAwaitingInput) return;

        this.SelectedCommandIndex--;
        if (this.SelectedCommandIndex < 0) {
            this.SelectedCommandIndex = this.GetVisibleCommands().Length - 1;
        }
    }

    public override void NavigateDown() {
        if (this.IsAwaitingInput) return;

        this.SelectedCommandIndex++;
        if (this.SelectedCommandIndex >= this.GetVisibleCommands().Length) {
            this.SelectedCommandIndex = 0;
        }
    }

    public override void ExecuteSelected() {
        if (this.IsAwaitingInput) {
            this.HandleParameterInput();
            return;
        }

        PlayerCommand[] visibleCommands = this.GetVisibleCommands();
        if (this.SelectedCommandIndex < 0 || this.SelectedCommandIndex >= visibleCommands.Length) return;

        PlayerCommand selectedCommand = visibleCommands[this.SelectedCommandIndex];

        // Check if command requires parameters
        if (selectedCommand.parameters.Length > 0) {
            // Start parameter input
            this.IsAwaitingInput = true;
            this.CurrentParameterIndex = 0;
            Array.Clear(this.ParameterValues, 0, this.ParameterValues.Length);
        }
        else {
            // Execute command immediately
            this.ExecutePlayerCommand(selectedCommand, []);
        }
    }

    PlayerCommand[] GetVisibleCommands() {
        bool isHost = Helper.LocalPlayer?.IsHost ?? false;
        return [.. this.Commands.Where(cmd => !cmd.requiresHost || isHost)];
    }

    void HandleParameterInput() {
        PlayerCommand[] visibleCommands = this.GetVisibleCommands();
        PlayerCommand currentCommand = visibleCommands[this.SelectedCommandIndex];

        if (this.CurrentParameterIndex < currentCommand.parameters.Length) {
            string currentParam = currentCommand.parameters[this.CurrentParameterIndex];
            this.ShowParameterInput(currentParam, currentCommand);
        }
        else {
            // All parameters collected, execute command
            string[] finalArgs = [.. this.ParameterValues.Take(currentCommand.parameters.Length)];
            this.ExecutePlayerCommand(currentCommand, finalArgs);
            this.IsAwaitingInput = false;
        }
    }

    void ShowParameterInput(string parameterType, PlayerCommand _) {
        // This would show input fields, menus, etc. based on parameter type
        // For now, we'll use GUI.TextField for simplicity
        GUILayout.Label($"Enter {parameterType}:", GUI.skin.box);

        string currentValue = this.ParameterValues[this.CurrentParameterIndex] ?? "";
        string newValue = GUILayout.TextField(currentValue, GUILayout.Height(30));
        this.ParameterValues[this.CurrentParameterIndex] = newValue;

        if (GUILayout.Button("Confirm", GUILayout.Height(30))) {
            if (!ValidateParameter(parameterType, newValue)) {
                TeleportationMenuManager.StatusMessage = $"Invalid {parameterType}!";
                return;
            }

            this.CurrentParameterIndex++;
        }
    }

    static bool ValidateParameter(string parameterType, string value) {
        return parameterType.ToLower() switch {
            "duration" or "damage" or "delay" or "amount" or "x" or "y" or "z" or "quantity" => float.TryParse(value, out _),
            "player" => IsValidPlayer(value),
            "enemy" => IsValidEnemy(value),
            "suit" => !string.IsNullOrEmpty(value),
            "item" => !string.IsNullOrEmpty(value),
            "message" => !string.IsNullOrEmpty(value),
            "targetPlayer" or "sourcePlayer" => IsValidPlayer(value),
            _ => !string.IsNullOrEmpty(value)
        };
    }

    static bool IsValidPlayer(string playerName) => Helper.GetPlayer(playerName) != null;

    static bool IsValidEnemy(string enemyName) =>
        // Basic validation - in a real implementation you'd check against actual enemy types
        !string.IsNullOrEmpty(enemyName) && enemyName.Length > 1;

    async void ExecutePlayerCommand(PlayerCommand command, string[] arguments) {
        try {
            TeleportationMenuManager.StatusMessage = $"Executing {command.name}...";

            // Replace {player} placeholder with actual player name
            string[] finalArgs = [.. arguments.Select(arg =>
                arg.Contains("{player}") ? this.TargetPlayer.playerUsername : arg
            )];

            // Add player name as first argument if needed
            if (!finalArgs.Any(arg => arg.Contains(this.TargetPlayer.playerUsername))) {
                finalArgs = [this.TargetPlayer.playerUsername, .. finalArgs];
            }

            CommandResult result = await CommandExecutor.ExecuteAsync(command.syntax,
                new Arguments { Span = finalArgs }, CommandInvocationSource.Direct);

            if (result.Success) {
                TeleportationMenuManager.StatusMessage = $"{command.name} executed on {this.TargetPlayer.playerUsername}!";
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
        catch (Exception ex) {
            TeleportationMenuManager.StatusMessage = $"Error: {ex.Message}";
            Logger.Write(ex);
        }

        this.IsAwaitingInput = false;
    }

    public override void Draw() {
        GUILayout.Label($"Commands for: {this.TargetPlayer.playerUsername}", GUI.skin.box, GUILayout.Height(40));

        if (this.IsAwaitingInput) {
            this.DrawParameterInput();
            return;
        }

        PlayerCommand[] visibleCommands = this.GetVisibleCommands();

        if (visibleCommands.Length == 0) {
            GUILayout.Label("No commands available", GUI.skin.box, GUILayout.Height(100));
        }
        else {
            // Group commands by category
            this.DrawCommandsByCategory(visibleCommands);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Back", GUILayout.Height(30))) {
            this.Back();
        }

        GUILayout.Space(10);

        // Show status message if available
        if (!string.IsNullOrEmpty(TeleportationMenuManager.StatusMessage)) {
            GUILayout.Label(TeleportationMenuManager.StatusMessage, GUI.skin.box, GUILayout.Height(30));
        }

        GUILayout.Label("Numpad: 8=Up, 2=Down, 5=Select, Backspace=Back", GUI.skin.label);
    }

    void DrawCommandsByCategory(PlayerCommand[] commands) {
        IEnumerable<IGrouping<CommandCategory, PlayerCommand>> groupedCommands = commands.GroupBy(cmd => cmd.category);

        foreach (IGrouping<CommandCategory, PlayerCommand>? group in groupedCommands) {
            string categoryName = group.Key.ToString().Replace("CommandCategory.", "");
            GUILayout.Label(categoryName, GUI.skin.box);

            PlayerCommand[] commandList = [.. group];
            int maxVisible = 6;
            int startIndex = Mathf.Max(0, this.SelectedCommandIndex - (maxVisible / 2));
            int endIndex = Mathf.Min(commandList.Length, startIndex + maxVisible);

            if (endIndex - startIndex < maxVisible && startIndex > 0) {
                startIndex = Mathf.Max(0, endIndex - maxVisible);
            }

            for (int i = startIndex; i < endIndex; i++) {
                PlayerCommand cmd = commandList[i];
                bool isSelected = i == this.SelectedCommandIndex;

                if (isSelected) {
                    GUI.color = Color.green;
                    GUI.backgroundColor = Color.green;
                }

                string buttonText = cmd.name;
                if (cmd.parameters.Length > 0) {
                    buttonText += $" ({string.Join(", ", cmd.parameters)})";
                }

                if (GUILayout.Button(buttonText, GUILayout.Height(35))) {
                    this.SelectedCommandIndex = Array.IndexOf(commands, cmd);
                    this.ExecuteSelected();
                }

                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;
            }

            GUILayout.Space(5);
        }
    }

    void DrawParameterInput() {
        PlayerCommand[] visibleCommands = this.GetVisibleCommands();
        PlayerCommand currentCommand = visibleCommands[this.SelectedCommandIndex];

        GUILayout.Label($"Entering parameters for: {currentCommand.name}", GUI.skin.box);
        GUILayout.Label($"Player: {this.TargetPlayer.playerUsername}", GUI.skin.label);

        this.HandleParameterInput();

        if (GUILayout.Button("Cancel", GUILayout.Height(25))) {
            this.IsAwaitingInput = false;
        }
    }
}
