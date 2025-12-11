using System;
using UnityEngine;

sealed class ParameterInputMenuScreen : BaseMenuScreen {
    string CommandName { get; }
    string CommandSyntax { get; }
    string[] Parameters { get; }
    string[] ParameterDescriptions { get; }
    string[] ParameterValues { get; set; }
    int CurrentParameterIndex { get; set; }
    string CurrentInputValue { get; set; } = "";

    public ParameterInputMenuScreen(AdvancedCommandMenuMod menu, string name, string syntax, string[] parameters, string[] parameterDescriptions) : base(menu) {
        this.CommandName = name;
        this.CommandSyntax = syntax;
        this.Parameters = parameters ?? [];
        this.ParameterDescriptions = parameterDescriptions ?? [];
        this.ParameterValues = new string[this.Parameters.Length];
        this.CurrentParameterIndex = 0;
    }

    public override void NavigateUp() {
        if (this.CurrentParameterIndex > 0) {
            this.CurrentParameterIndex--;
            this.CurrentInputValue = this.ParameterValues[this.CurrentParameterIndex] ?? "";
        }
    }

    public override void NavigateDown() {
        if (this.CurrentParameterIndex < this.ParameterValues.Length - 1) {
            this.ParameterValues[this.CurrentParameterIndex] = this.CurrentInputValue;
            this.CurrentParameterIndex++;
            this.CurrentInputValue = this.ParameterValues[this.CurrentParameterIndex] ?? "";
        }
    }

    public override void ExecuteSelected() {
        if (this.Parameters == null || this.Parameters.Length == 0) {
            this.ExecuteCommand([]);
            return;
        }

        // Save current input
        this.ParameterValues[this.CurrentParameterIndex] = this.CurrentInputValue;

        // Validate all required parameters are filled
        bool allFilled = true;
        for (int i = 0; i < this.Parameters.Length; i++) {
            if (i >= this.ParameterValues.Length) break;

            string param = this.Parameters[i];
            string value = this.ParameterValues[i];

            // Check if parameter is optional (contains "optional" or "default" in description)
            bool isOptional = this.ParameterDescriptions != null &&
                            i < this.ParameterDescriptions.Length &&
                            (this.ParameterDescriptions[i].Contains("optional", System.StringComparison.OrdinalIgnoreCase) ||
                             this.ParameterDescriptions[i].Contains("default", System.StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(value) && !isOptional) {
                allFilled = false;
                break;
            }

            // Validate numeric parameters
            if (!string.IsNullOrWhiteSpace(value)) {
                if (param.Contains("duration", System.StringComparison.OrdinalIgnoreCase) ||
                    param.Contains("amount", System.StringComparison.OrdinalIgnoreCase) ||
                    param.Contains("damage", System.StringComparison.OrdinalIgnoreCase) ||
                    param.Contains("delay", System.StringComparison.OrdinalIgnoreCase) ||
                    param.Contains("force", System.StringComparison.OrdinalIgnoreCase) ||
                    param.Contains("scale", System.StringComparison.OrdinalIgnoreCase) ||
                    param.Contains("quantity", System.StringComparison.OrdinalIgnoreCase)) {
                    if (!float.TryParse(value, out _)) {
                        TeleportationMenuManager.StatusMessage = $"{param} must be a number!";
                        return;
                    }
                }
            }
        }

        if (!allFilled) {
            TeleportationMenuManager.StatusMessage = "Please fill all required parameters!";
            return;
        }

        // Only use the parameters we actually have
        string[] finalParams = new string[this.Parameters.Length];
        Array.Copy(this.ParameterValues, finalParams, this.Parameters.Length);

        this.ExecuteCommand(finalParams);
    }

    void ExecuteCommand(string[] parameters) {
        // Execute the command directly
        AdvancedCommandMenuMod.ExecuteRegularCommandWithParams(this.CommandName, this.CommandSyntax, parameters);

        // Go back to category selection
        this.Back();
    }

    public override void Draw() {
        GUILayout.Label($"Enter Parameters: {this.CommandName}", GUI.skin.box, GUILayout.Height(40));

        GUILayout.Space(10);

        if (this.Parameters == null || this.Parameters.Length == 0) {
            GUILayout.Label("No parameters needed", GUI.skin.label);
            return;
        }

        // Draw all parameters with the current one highlighted
        for (int i = 0; i < this.Parameters.Length; i++) {
            string param = this.Parameters[i];
            string description = this.ParameterDescriptions != null && i < this.ParameterDescriptions.Length
                ? this.ParameterDescriptions[i]
                : param;

            // Determine if optional
            bool isOptional = description.Contains("optional", System.StringComparison.OrdinalIgnoreCase) || description.Contains("default", System.StringComparison.OrdinalIgnoreCase);
            bool isCurrent = i == this.CurrentParameterIndex;

            GUILayout.BeginVertical(GUI.skin.box);

            // Parameter label
            GUIStyle labelStyle = new(GUI.skin.label);
            if (isCurrent) {
                labelStyle.normal.textColor = Color.green;
                labelStyle.fontStyle = FontStyle.Bold;
            }
            string optionalLabel = isOptional ? " (optional)" : " (required)";
            GUILayout.Label($"{i + 1}. {description}{optionalLabel}", labelStyle);

            // Show current value or input field
            if (isCurrent) {
                GUI.SetNextControlName($"Input_{i}");
                this.CurrentInputValue = GUILayout.TextField(this.CurrentInputValue, GUILayout.Height(30));
                GUI.FocusControl($"Input_{i}");
            }
            else {
                string displayValue = this.ParameterValues[i];
                if (string.IsNullOrWhiteSpace(displayValue)) {
                    displayValue = "(empty)";
                }
                GUILayout.Label($"Value: {displayValue}", GUI.skin.label);
            }

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Execute", GUILayout.Height(35))) {
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

        GUILayout.Label("Numpad: 8/2=Navigate Fields, 5=Execute, Backspace=Cancel", GUI.skin.label);
    }
}
