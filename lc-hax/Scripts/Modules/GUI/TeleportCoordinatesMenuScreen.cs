using System;
using UnityEngine;

sealed class TeleportCoordinatesMenuScreen(AdvancedCommandMenuMod menu) : BaseMenuScreen(menu) {
    string XValue { get; set; } = "";
    string YValue { get; set; } = "";
    string ZValue { get; set; } = "";
    int CurrentFieldIndex { get; set; } = 0;

    public override void NavigateUp() {
        if (this.CurrentFieldIndex > 0) {
            this.CurrentFieldIndex--;
        }
        else {
            this.CurrentFieldIndex = 2; // Loop to Z
        }
    }

    public override void NavigateDown() {
        if (this.CurrentFieldIndex < 2) {
            this.CurrentFieldIndex++;
        }
        else {
            this.CurrentFieldIndex = 0; // Loop to X
        }
    }

    public override void ExecuteSelected() {
        // Save current input
        SaveCurrentInput();

        // Validate coordinates - use local variables for TryParse
        string xValue = this.XValue;
        string yValue = this.YValue;
        string zValue = this.ZValue;
        bool isValidX = float.TryParse(xValue, out _);
        bool isValidY = float.TryParse(yValue, out _);
        bool isValidZ = float.TryParse(zValue, out _);

        if (!isValidX || !isValidY || !isValidZ) {
            TeleportationMenuManager.StatusMessage = "Invalid coordinates! Please enter valid numbers for X, Y, and Z.";
            return;
        }

        // Execute teleport command
        AdvancedCommandMenuMod.ExecuteRegularCommandWithParams("Teleport to Location", "tp", [xValue, yValue, zValue]);

        // Go back to the options menu
        this.Back();
    }

    public override void Draw() {
        GUILayout.Label("Teleport to Coordinates", GUI.skin.box, GUILayout.Height(40));

        GUILayout.Space(10);

        // X Coordinate
        this.DrawCoordinateField("X Coordinate", 0, this.XValue, value => this.XValue = value);
        // Y Coordinate  
        this.DrawCoordinateField("Y Coordinate", 1, this.YValue, value => this.YValue = value);
        // Z Coordinate
        this.DrawCoordinateField("Z Coordinate", 2, this.ZValue, value => this.ZValue = value);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Teleport", GUILayout.Height(35))) {
            this.ExecuteSelected();
        }
        if (GUILayout.Button("Cancel", GUILayout.Height(35))) {
            this.Back();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Show status message if available
        if (!string.IsNullOrEmpty(TeleportationMenuManager.StatusMessage)) {
            GUILayout.Label(TeleportationMenuManager.StatusMessage, GUI.skin.box, GUILayout.Height(30));
        }

        GUILayout.Label("Numpad: 8/2=Navigate Fields, 5=Teleport, Backspace=Cancel", GUI.skin.label);
    }

    void DrawCoordinateField(string label, int fieldIndex, string currentValue, Action<string> updateValue) {
        bool isCurrent = fieldIndex == this.CurrentFieldIndex;

        GUILayout.BeginVertical(GUI.skin.box);

        // Label
        GUIStyle labelStyle = new(GUI.skin.label);
        if (isCurrent) {
            labelStyle.normal.textColor = Color.green;
            labelStyle.fontStyle = FontStyle.Bold;
        }
        GUILayout.Label(label, labelStyle);

        // Input field
        if (isCurrent) {
            GUI.SetNextControlName($"CoordInput_{fieldIndex}");
            string newValue = GUILayout.TextField(currentValue, GUILayout.Height(30));
            GUI.FocusControl($"CoordInput_{fieldIndex}");

            // Only update if the value actually changed to avoid cursor jumping
            if (newValue != currentValue) {
                updateValue(newValue);
            }
        }
        else {
            string displayValue = string.IsNullOrWhiteSpace(currentValue) ? "(empty)" : currentValue;
            GUILayout.Label($"Value: {displayValue}", GUI.skin.label);
        }

        GUILayout.EndVertical();
        GUILayout.Space(5);
    }

    static void SaveCurrentInput() {
        // This method ensures the current input field's value is captured
        // In a more complex implementation, this might handle cursor position, etc.
    }
}
