module Window

open OpenTK.Windowing.Desktop
open OpenTK.Windowing.GraphicsLibraryFramework

type Window(gameWindowSettings, nativeWindowSettings) =
    inherit GameWindow(gameWindowSettings, nativeWindowSettings)
    
    // This function runs on every update frame.
    override this.OnUpdateFrame(e) =
        // Check if the Escape button is currently being pressed.
        if (this.KeyboardState.IsKeyDown Keys.Escape) then
            // If it is, close the window.
            this.Close()
        base.OnUpdateFrame(e)            