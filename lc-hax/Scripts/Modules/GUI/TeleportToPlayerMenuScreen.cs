using System.Linq;
using GameNetcodeStuff;
using UnityEngine;

sealed class TeleportToPlayerMenuScreen : BaseMenuScreen {
    PlayerControllerB?[] AvailablePlayers { get; }
    int SelectedIndex { get; set; } = 0;

    public TeleportToPlayerMenuScreen(AdvancedCommandMenuMod menu) : base(menu) => this.AvailablePlayers = GetAvailablePlayers();

    public override void NavigateUp() {
        if (this.AvailablePlayers.Length > 0) {
            if (this.SelectedIndex > 0) {
                this.SelectedIndex--;
            }
            else {
                this.SelectedIndex = this.AvailablePlayers.Length - 1;
            }
        }
    }

    public override void NavigateDown() {
        if (this.AvailablePlayers.Length > 0) {
            if (this.SelectedIndex < this.AvailablePlayers.Length - 1) {
                this.SelectedIndex++;
            }
            else {
                this.SelectedIndex = 0;
            }
        }
    }

    public override void ExecuteSelected() {
        if (this.AvailablePlayers.Length == 0) {
            TeleportationMenuManager.StatusMessage = "No other players available to teleport to!";
            this.Back();
            return;
        }

        PlayerControllerB? targetPlayer = this.AvailablePlayers[this.SelectedIndex];
        if (targetPlayer == null) {
            TeleportationMenuManager.StatusMessage = "Selected player is not available!";
            return;
        }

        // Execute teleport command to selected player
        AdvancedCommandMenuMod.ExecuteRegularCommandWithParams("Teleport to Player", "tp", [targetPlayer.playerUsername]);

        // Go back to the options menu
        this.Back();
    }

    public override void Draw() {
        GUILayout.Label("Teleport to Player", GUI.skin.box, GUILayout.Height(40));

        GUILayout.Space(10);

        if (this.AvailablePlayers.Length == 0) {
            GUILayout.Label("No other players available", GUI.skin.label, GUILayout.Height(50));
        }
        else {
            GUILayout.Label($"Select a player to teleport to ({this.AvailablePlayers.Length} available):", GUI.skin.label);

            GUILayout.Space(10);

            for (int i = 0; i < this.AvailablePlayers.Length; i++) {
                PlayerControllerB? player = this.AvailablePlayers[i];
                if (player == null) continue;

                bool isSelected = i == this.SelectedIndex;

                GUILayout.BeginVertical(GUI.skin.box);

                if (isSelected) {
                    GUI.color = Color.green;
                    GUI.backgroundColor = Color.green;
                }

                string playerInfo = $"{player.playerUsername} (ID: {player.playerClientId})";
                GUILayout.Label(playerInfo, GUI.skin.label, GUILayout.Height(25));

                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;

                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Teleport", GUILayout.Height(35), GUILayout.ExpandWidth(false))) {
            this.ExecuteSelected();
        }
        if (GUILayout.Button("Cancel", GUILayout.Height(35), GUILayout.ExpandWidth(false))) {
            this.Back();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Show status message if available
        if (!string.IsNullOrEmpty(TeleportationMenuManager.StatusMessage)) {
            GUILayout.Label(TeleportationMenuManager.StatusMessage, GUI.skin.box, GUILayout.Height(30));
        }

        GUILayout.Label("Numpad: 8/2=Navigate Players, 5=Teleport, Backspace=Cancel", GUI.skin.label);
    }

    static PlayerControllerB?[] GetAvailablePlayers() {
        PlayerControllerB[] activePlayers = Helper.ActivePlayers;
        PlayerControllerB? localPlayer = Helper.LocalPlayer;

        // Filter out the local player and dead players
        return [.. activePlayers.Where(player =>
            player != localPlayer &&
            player.isPlayerControlled &&
            !player.isPlayerDead
        )];
    }
}
