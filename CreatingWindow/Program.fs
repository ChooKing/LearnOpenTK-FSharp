module OpenTKTest

open OpenTK.Graphics.OpenGL4
open OpenTK.Mathematics
open OpenTK.Windowing.Common
open OpenTK.Windowing.Desktop

type Window(gameWindowSettings, nativeWindowSettings) =
    inherit GameWindow(gameWindowSettings, nativeWindowSettings)
    override this.OnLoad () =
        GL.ClearColor(0.0f, 0.0f, 1.0f, 0.0f)
        base.OnLoad()
        
    override this.OnResize e =
        GL.Viewport(0, 0, e.Width, e.Height)
        base.OnResize e

    override this.OnRenderFrame(e) =
        GL.Clear(ClearBufferMask.ColorBufferBit)
        this.Context.SwapBuffers()
        base.OnRenderFrame(e)

[<EntryPoint>]
let main argv =
    let nativeSettings = new NativeWindowSettings(ClientSize = new Vector2i(1600, 1200), Title = "Testing", Flags = ContextFlags.ForwardCompatible)    
    use window = new Window(GameWindowSettings.Default, nativeSettings)
    window.Run()
    0