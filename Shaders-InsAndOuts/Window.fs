module Window

open OpenTK.Graphics.OpenGL4
open LearnOpenTK.Common
open OpenTK.Windowing.Desktop
open OpenTK.Windowing.Common
open OpenTK.Windowing.GraphicsLibraryFramework



// Here we'll be elaborating on what shaders can do from the Hello World project we worked on before.
// Specifically we'll be showing how shaders deal with input and output from the main program 
// and between each other.
type Window(gameWindowSettings, nativeWindowSettings) =
    inherit GameWindow(gameWindowSettings, nativeWindowSettings)
    let _vertices = [|
         -0.5f; -0.5f; 0.0f; // Bottom-left vertex
         0.5f; -0.5f; 0.0f; // Bottom-right vertex
         0.0f;  0.5f; 0.0f  // Top vertex
    |]
    let mutable _vertexBufferObject = 0
    let mutable _vertexArrayObject = 0
    [<DefaultValue>]
    val mutable private _shader: Shader
    
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
        
        // Vertex attributes are the data we send as input into the vertex shader from the main program.
        // So here we're checking to see how many vertex attributes our hardware can handle.
        // OpenGL at minimum supports 16 vertex attributes. This only needs to be called 
        // when your intensive attribute work and need to know exactly how many are available to you.
        let mutable maxAttributeCount = 0
        GL.GetInteger(GetPName.MaxVertexAttribs, ref maxAttributeCount)
        System.Diagnostics.Debug.Print $"Maximum number of vertex attributes supported: {maxAttributeCount}\n"
        this._shader <- new Shader("Shaders/shader.vert", "Shaders/shader.frag")
        this._shader.Use()
        
    override this.OnRenderFrame(e: FrameEventArgs) =
        base.OnRenderFrame(e)
        GL.Clear(ClearBufferMask.ColorBufferBit)
        this._shader.Use()
        GL.BindVertexArray(_vertexArrayObject)
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3)
        this.SwapBuffers()
        
    override this.OnUpdateFrame(e: FrameEventArgs) =
        base.OnUpdateFrame(e)
        let input = this.KeyboardState        
        if (input.IsKeyDown Keys.Escape) then            
            this.Close()
            
    override this.OnResize(e: ResizeEventArgs) =
        base.OnResize(e)
        GL.Viewport(0, 0, e.Width, e.Height)
    