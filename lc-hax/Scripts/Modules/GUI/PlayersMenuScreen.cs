using System.Linq;
using GameNetcodeStuff;
using UnityEngine;

sealed class PlayersMenuScreen : BaseMenuScreen {
    PlayerControllerB[] CurrentPlayers { get; set; } = [];
    int SelectedPlayerIndex { get; set; }

    public PlayersMenuScreen(AdvancedCommandMenuMod menu) : base(menu) => this.RefreshPlayerList();

    public override void NavigateUp() {
        if (this.CurrentPlayers.Length == 0) return;

        this.SelectedPlayerIndex--;
        if (this.SelectedPlayerIndex < 0) {
            this.SelectedPlayerIndex = this.CurrentPlayers.Length - 1;
        }
    }

    public override void NavigateDown() {
        if (this.CurrentPlayers.Length == 0) return;

        this.SelectedPlayerIndex++;
        if (this.SelectedPlayerIndex >= this.CurrentPlayers.Length) {
            this.SelectedPlayerIndex = 0;
        }
    }

    public override void ExecuteSelected() {
        if (this.CurrentPlayers.Length == 0) return;

        if (this.SelectedPlayerIndex < 0 || this.SelectedPlayerIndex >= this.CurrentPlayers.Length) return;

        PlayerControllerB selectedPlayer = this.CurrentPlayers[this.SelectedPlayerIndex];
        this.Menu.SelectedPlayer = selectedPlayer;

        // Push player commands screen
        this.Menu.screenStack.Push(this);
        this.Menu.CurrentState = AdvancedCommandMenuMod.MenuState.PlayerCommandSelection;
        this.Menu.CurrentScreen = new PlayerCommandsMenuScreen(this.Menu, selectedPlayer);
    }

    public override void Draw() {
        this.RefreshPlayerList();

        GUILayout.Label("Players in Lobby", GUI.skin.box, GUILayout.Height(40));

        if (this.CurrentPlayers.Length == 0) {
            GUILayout.Label("No players found", GUI.skin.box, GUILayout.Height(100));
        }
        else {
            // Show player count
            GUILayout.Label($"Found {this.CurrentPlayers.Length} players", GUI.skin.label);

            GUILayout.Space(10);

            // Show players with navigation
            int maxVisible = 8;
            int startIndex = Mathf.Max(0, this.SelectedPlayerIndex - (maxVisible / 2));
            int endIndex = Mathf.Min(this.CurrentPlayers.Length, startIndex + maxVisible);

            if (endIndex - startIndex < maxVisible && startIndex > 0) {
                startIndex = Mathf.Max(0, endIndex - maxVisible);
            }

            for (int i = startIndex; i < endIndex; i++) {
                PlayerControllerB player = this.CurrentPlayers[i];
                bool isSelected = i == this.SelectedPlayerIndex;

                if (isSelected) {
                    GUI.color = Color.green;
                    GUI.backgroundColor = Color.green;
                }

                string playerInfo = $"{player.playerUsername} (ID: {player.playerClientId})";
                if (player.isPlayerDead) {
                    playerInfo += " [DEAD]";
                }
                if (player.IsHost) {
                    playerInfo += " [HOST]";
                }

                if (GUILayout.Button(playerInfo, GUILayout.Height(35))) {
                    this.SelectedPlayerIndex = i;
                    this.ExecuteSelected();
                }

                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;
            }
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

    void RefreshPlayerList() {
        PlayerControllerB[] players = Helper.Players;
        if (players != null && players.Length > 0) {
            this.CurrentPlayers = [.. players.Where(p => p != null && !string.IsNullOrEmpty(p.playerUsername))];

            // Adjust selected index if needed
            if (this.SelectedPlayerIndex >= this.CurrentPlayers.Length) {
                this.SelectedPlayerIndex = this.CurrentPlayers.Length - 1;
            }
            if (this.SelectedPlayerIndex < 0 && this.CurrentPlayers.Length > 0) {
                this.SelectedPlayerIndex = 0;
            }
        }
        else {
            this.CurrentPlayers = [];
            this.SelectedPlayerIndex = 0;
        }
    }
}
