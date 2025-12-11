using UnityEngine;

sealed class TeleportationOptionsMenuScreen(AdvancedCommandMenuMod menu) : BaseMenuScreen(menu) {
    enum TeleportOption {
        Location,
        Player,
        PlayerToPlayer
    }

    TeleportOption SelectedOption { get; set; } = TeleportOption.Location;

    public override void NavigateUp() {
        if (this.SelectedOption > TeleportOption.Location) {
            this.SelectedOption--;
        }
        else {
            this.SelectedOption = TeleportOption.PlayerToPlayer;
        }
    }

    public override void NavigateDown() {
        if (this.SelectedOption < TeleportOption.PlayerToPlayer) {
            this.SelectedOption++;
        }
        else {
            this.SelectedOption = TeleportOption.Location;
        }
    }

    public override void ExecuteSelected() {
        switch (this.SelectedOption) {
            case TeleportOption.Location:
                this.Menu.screenStack.Push(this);
                this.Menu.CurrentState = AdvancedCommandMenuMod.MenuState.TeleportationSubmenu;
                this.Menu.CurrentScreen = new TeleportCoordinatesMenuScreen(this.Menu);
                break;
            case TeleportOption.Player:
                this.Menu.screenStack.Push(this);
                this.Menu.CurrentState = AdvancedCommandMenuMod.MenuState.TeleportationSubmenu;
                this.Menu.CurrentScreen = new TeleportToPlayerMenuScreen(this.Menu);
                break;
            case TeleportOption.PlayerToPlayer:
                this.Menu.screenStack.Push(this);
                this.Menu.CurrentState = AdvancedCommandMenuMod.MenuState.TeleportationSubmenu;
                this.Menu.CurrentScreen = new TeleportPlayerTransferMenuScreen(this.Menu);
                break;
            default:
                break;
        }
    }

    public override void Draw() {
        GUILayout.Label("Teleportation Options", GUI.skin.box, GUILayout.Height(40));

        GUILayout.Space(10);

        this.DrawOption(TeleportOption.Location, "Teleport to Location", "Enter X/Y/Z coordinates to teleport to a specific position");
        this.DrawOption(TeleportOption.Player, "Teleport to Player", "Teleport to a selected player's current location");
        this.DrawOption(TeleportOption.PlayerToPlayer, "Teleport Player to Player", "Teleport one player to another player's location");

        GUILayout.Space(10);

        // Show status message if available
        if (!string.IsNullOrEmpty(TeleportationMenuManager.StatusMessage)) {
            GUILayout.Label(TeleportationMenuManager.StatusMessage, GUI.skin.box, GUILayout.Height(30));
        }

        GUILayout.Label("Numpad: 8/2=Navigate, 5=Select, Backspace=Back", GUI.skin.label);
    }

    void DrawOption(TeleportOption option, string title, string description) {
        bool isSelected = this.SelectedOption == option;

        GUILayout.BeginVertical(GUI.skin.box);

        if (isSelected) {
            GUI.color = Color.green;
            GUI.backgroundColor = Color.green;
        }

        GUILayout.Label(title, GUI.skin.label, GUILayout.Height(25));
        GUILayout.Label(description, GUI.skin.label);

        GUI.color = Color.white;
        GUI.backgroundColor = Color.white;

        GUILayout.EndVertical();
        GUILayout.Space(5);
    }
}
