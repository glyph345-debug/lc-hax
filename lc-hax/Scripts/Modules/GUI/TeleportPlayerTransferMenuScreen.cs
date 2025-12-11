using System;
using System.Linq;
using GameNetcodeStuff;
using UnityEngine;

sealed class TeleportPlayerTransferMenuScreen : BaseMenuScreen {
    enum SelectionPhase {
        Source,
        Destination
    }

    PlayerControllerB?[] AvailablePlayers { get; }
    SelectionPhase CurrentPhase { get; set; } = SelectionPhase.Source;
    PlayerControllerB? SelectedSourcePlayer { get; set; }
    PlayerControllerB? SelectedDestinationPlayer { get; set; }
    int SourceIndex { get; set; } = 0;
    int DestinationIndex { get; set; } = 0;

    public TeleportPlayerTransferMenuScreen(AdvancedCommandMenuMod menu) : base(menu) => this.AvailablePlayers = GetAvailablePlayers();

    public override void NavigateUp() {
        PlayerControllerB?[] players = this.GetCurrentPhasePlayers();
        if (players.Length == 0) return;

        if (this.CurrentPhase == SelectionPhase.Source) {
            if (this.SourceIndex > 0) {
                this.SourceIndex--;
            }
            else {
                this.SourceIndex = players.Length - 1;
            }
        }
        else {
            if (this.DestinationIndex > 0) {
                this.DestinationIndex--;
            }
            else {
                this.DestinationIndex = players.Length - 1;
            }
        }
    }

    public override void NavigateDown() {
        PlayerControllerB?[] players = this.GetCurrentPhasePlayers();
        if (players.Length == 0) return;

        if (this.CurrentPhase == SelectionPhase.Source) {
            if (this.SourceIndex < players.Length - 1) {
                this.SourceIndex++;
            }
            else {
                this.SourceIndex = 0;
            }
        }
        else {
            if (this.DestinationIndex < players.Length - 1) {
                this.DestinationIndex++;
            }
            else {
                this.DestinationIndex = 0;
            }
        }
    }

    public override void ExecuteSelected() {
        PlayerControllerB?[] players = this.GetCurrentPhasePlayers();
        if (players.Length == 0) {
            TeleportationMenuManager.StatusMessage = "No players available for selection!";
            return;
        }

        if (this.CurrentPhase == SelectionPhase.Source) {
            // Select source player and move to destination phase
            if (this.CurrentPhase == SelectionPhase.Source) {
                this.SelectedSourcePlayer = players[this.SourceIndex];
            }
            this.CurrentPhase = SelectionPhase.Destination;

            // Adjust destination index to avoid selecting the same player
            if (this.SelectedSourcePlayer != null) {
                int sourceIndexInDest = Array.FindIndex(players, p => p?.playerClientId == this.SelectedSourcePlayer.playerClientId);
                if (sourceIndexInDest >= 0 && this.DestinationIndex >= sourceIndexInDest) {
                    this.DestinationIndex = (this.DestinationIndex + 1) % players.Length;
                }
            }
        }
        else {
            // Destination phase - execute the teleport
            this.SelectedDestinationPlayer = players[this.DestinationIndex];

            if (this.SelectedSourcePlayer == null || this.SelectedDestinationPlayer == null) {
                TeleportationMenuManager.StatusMessage = "Invalid player selection!";
                this.Back();
                return;
            }

            // Execute teleport player to player command
            AdvancedCommandMenuMod.ExecuteRegularCommandWithParams(
                "Teleport Player to Player",
                "tp",
                [this.SelectedSourcePlayer.playerUsername, this.SelectedDestinationPlayer.playerUsername]
            );

            // Go back to the options menu
            this.Back();
        }
    }

    public override void Draw() {
        string phaseTitle = this.CurrentPhase == SelectionPhase.Source ? "Select Source Player" : "Select Destination Player";
        GUILayout.Label(phaseTitle, GUI.skin.box, GUILayout.Height(40));

        GUILayout.Space(10);

        // Show current selections
        if (this.SelectedSourcePlayer != null) {
            GUILayout.Label($"Source: {this.SelectedSourcePlayer.playerUsername}", GUI.skin.label);
        }
        if (this.SelectedDestinationPlayer != null) {
            GUILayout.Label($"Destination: {this.SelectedDestinationPlayer.playerUsername}", GUI.skin.label);
        }

        GUILayout.Space(10);

        PlayerControllerB?[] players = this.GetCurrentPhasePlayers();
        if (players.Length == 0) {
            GUILayout.Label("No players available for selection", GUI.skin.label, GUILayout.Height(50));
        }
        else {
            GUILayout.Label($"Select {this.CurrentPhase.ToString().ToLower()} player ({players.Length} available):", GUI.skin.label);

            GUILayout.Space(10);

            int currentIndex = this.CurrentPhase == SelectionPhase.Source ? this.SourceIndex : this.DestinationIndex;

            for (int i = 0; i < players.Length; i++) {
                PlayerControllerB? player = players[i];
                if (player == null) continue;

                bool isSelected = i == currentIndex;

                GUILayout.BeginVertical(GUI.skin.box);

                if (isSelected) {
                    GUI.color = Color.green;
                    GUI.backgroundColor = Color.green;
                }

                string playerInfo = $"{player.playerUsername} (ID: {player.playerClientId})";
                GUILayout.Label(playerInfo, GUI.skin.label, GUILayout.Height(25));

                // Show if this player is already selected in the other phase
                if (this.SelectedSourcePlayer?.playerClientId == player.playerClientId) {
                    GUILayout.Label("[Already selected as source]", GUI.skin.label);
                }
                else if (this.SelectedDestinationPlayer?.playerClientId == player.playerClientId) {
                    GUILayout.Label("[Already selected as destination]", GUI.skin.label);
                }

                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;

                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Select", GUILayout.Height(35), GUILayout.ExpandWidth(false))) {
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

        string instruction = this.CurrentPhase == SelectionPhase.Source
            ? "Numpad: 8/2=Navigate Players, 5=Select, Backspace=Back"
            : "Numpad: 8/2=Navigate Players, 5=Execute Transfer, Backspace=Back";
        GUILayout.Label(instruction, GUI.skin.label);
    }

    static PlayerControllerB?[] GetAvailablePlayers() {
        return [.. Helper.ActivePlayers.Where(player =>
            player.isPlayerControlled &&
            !player.isPlayerDead
        )];
    }

    PlayerControllerB?[] GetCurrentPhasePlayers() {
        // In destination phase, exclude the source player to avoid teleporting player to themselves
        return this.CurrentPhase == SelectionPhase.Destination && this.SelectedSourcePlayer != null
            ? [.. this.AvailablePlayers.Where(player =>
                player != null &&
                player.playerClientId != this.SelectedSourcePlayer.playerClientId
            )]
            : this.AvailablePlayers;
    }
}
