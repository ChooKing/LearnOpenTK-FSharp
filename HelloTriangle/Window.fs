module Window

open OpenTK.Graphics.OpenGL4
open OpenTK.Windowing.GraphicsLibraryFramework
open OpenTK.Windowing.Desktop
open LearnOpenTK.Common

type Window(gameWindowSettings, nativeWindowSettings) =
    inherit GameWindow(gameWindowSettings, nativeWindowSettings)
    // Create the vertices for our triangle. These are listed in normalized device coordinates (NDC)
    // In NDC, (0, 0) is the center of the screen.
    // Negative X coordinates move to the left, positive X move to the right.
    // Negative Y coordinates move to the bottom, positive Y move to the top.
    // OpenGL only supports rendering in 3D, so to create a flat triangle, the Z coordinate will be kept as 0.
    let _vertices = [|-0.5f; -0.5f; 0.0f; 0.5f; -0.5f; 0.0f; 0.0f;  0.5f; 0.0f|]
    // These are the handles to OpenGL objects. A handle is an integer representing where the object lives on the
    // graphics card. Consider them sort of like a pointer; we can't do anything with them directly, but we can
    // send them to OpenGL functions that need them.

    // What these objects are will be explained in OnLoad.
    let mutable _vertexBufferObject = 0
    let mutable _vertexArrayObject = 0    
    let mutable program = 0    
    let _shader = Shader("Shaders/shader.vert", "Shaders/shader.frag")
            
        
    override this.OnLoad() =
        base.OnLoad()
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f)
        _vertexBufferObject <- GL.GenBuffer()
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject)
        GL.BufferData(BufferTarget.ArrayBuffer, nativeint(_vertices.Length * sizeof<float32>), _vertices, BufferUsageHint.StaticDraw)
        _vertexArrayObject <- GL.GenVertexArray()
        GL.BindVertexArray _vertexArrayObject
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof<float32>, 0)
        GL.EnableVertexAttribArray(0)        
        _shader.Use()          
            
    override this.OnRenderFrame(e) =
        base.OnRenderFrame e
        GL.Clear(ClearBufferMask.ColorBufferBit)        
        _shader.Use()
        GL.BindVertexArray _vertexArrayObject
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3)
        this.SwapBuffers()
        
    
    // This function runs on every update frame.
    override this.OnUpdateFrame(e) =
        base.OnUpdateFrame(e)
        let input = this.KeyboardState
        // Check if the Escape button is currently being pressed.
        if (input.IsKeyDown Keys.Escape) then
            // If it is, close the window.
            this.Close()
            
    override this.OnResize(e) =
        base.OnResize e
        GL.Viewport(0, 0, e.Width, e.Height)
        
    override this.OnUnload() =
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0)
        GL.BindVertexArray 0
        GL.UseProgram 0
        GL.DeleteBuffer _vertexBufferObject
        GL.DeleteVertexArray _vertexArrayObject
        GL.DeleteProgram program
        base.OnUnload()
                  