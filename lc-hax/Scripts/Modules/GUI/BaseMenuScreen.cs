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
        }
        else {
            this.Menu.CurrentState = AdvancedCommandMenuMod.MenuState.CategorySelection;
            this.Menu.CurrentScreen = null;
        }
    }
}
