module ShadersMoreAttributes
open OpenTK.Mathematics
open OpenTK.Windowing.Common
open OpenTK.Windowing.Desktop
open Window

[<EntryPoint>]
let main argv =
    let nativeSettings =
        new NativeWindowSettings(
            ClientSize = new Vector2i(800, 600),
            Title = "LearnOpenTK - Shaders More Attributes!",
            // This is needed to run on macos
            Flags = ContextFlags.ForwardCompatible
        )
    // To create a new window, create a class that extends GameWindow, then call Run() on it.
    use window = new Window(GameWindowSettings.Default, nativeSettings)
    window.Run()
    0
