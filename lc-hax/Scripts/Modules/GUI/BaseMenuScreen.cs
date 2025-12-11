class BaseMenuScreen {
    protected AdvancedCommandMenuMod Menu { get; }

    protected BaseMenuScreen(AdvancedCommandMenuMod menu) => this.Menu = menu;

    public virtual void NavigateUp() { }
    public virtual void NavigateDown() { }
    public virtual void ExecuteSelected() { }
    public virtual void Draw() { }

    protected void Back() {
        if (this.Menu.screenStack.Count > 0) {
            this.Menu.CurrentScreen = this.Menu.screenStack.Pop();
            // Restore the appropriate state based on the screen type
            if (this.Menu.CurrentScreen is PlayersMenuScreen) {
                this.Menu.CurrentState = AdvancedCommandMenuMod.MenuState.PlayerSelection;
            }
            else if (this.Menu.CurrentScreen is PlayerCommandsMenuScreen) {
                this.Menu.CurrentState = AdvancedCommandMenuMod.MenuState.PlayerCommandSelection;
            }
            else if (this.Menu.CurrentScreen is ParameterInputMenuScreen) {
                this.Menu.CurrentState = AdvancedCommandMenuMod.MenuState.ParameterInput;
            }
            else if (this.Menu.CurrentScreen is PlayerPickerMenuScreen) {
                this.Menu.CurrentState = AdvancedCommandMenuMod.MenuState.ParameterInput;
            }
        }
        else {
            this.Menu.CurrentState = AdvancedCommandMenuMod.MenuState.CategorySelection;
            this.Menu.CurrentScreen = null;
        }
    }
}
