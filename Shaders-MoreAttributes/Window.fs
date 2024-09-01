module Window

open OpenTK.Graphics.OpenGL4
open LearnOpenTK.Common
open OpenTK.Windowing.Desktop
open OpenTK.Windowing.GraphicsLibraryFramework

// In this project, we will be assigning 3 colors to the triangle, one for each vertex.
// The output will be an interpolated value based on the distance from each vertex.
// If you want to look more into it, the in-between step is called a Rasterizer.
type Window(gameWindowSettings, nativeWindowSettings) =
    inherit GameWindow(gameWindowSettings, nativeWindowSettings)
    
    // We're assigning three different colors at the associated vertex position:
    // blue for the top, green for the bottom left and red for the bottom right.
    let _vertices = [|
        // positions        // colors
         0.5f; -0.5f; 0.0f;  1.0f; 0.0f; 0.0f;   // bottom right
        -0.5f; -0.5f; 0.0f;  0.0f; 1.0f; 0.0f;   // bottom left
         0.0f;  0.5f; 0.0f;  0.0f; 0.0f; 1.0f    // top 
    |]
    let mutable _vertexBufferObject = 0
    let mutable _vertexArrayObject = 0    
    [<DefaultValue>]
    val mutable private _shader: Shader            
        
    // Now, we start initializing OpenGL.
    override this.OnLoad() =
        base.OnLoad()
        
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f)
        
        _vertexBufferObject <- GL.GenBuffer()
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject)        
        GL.BufferData(BufferTarget.ArrayBuffer, nativeint(_vertices.Length * sizeof<float32>), _vertices, BufferUsageHint.StaticDraw)
        
        _vertexArrayObject <- GL.GenVertexArray()
        GL.BindVertexArray _vertexArrayObject
        
        // Just like before, we create a pointer for the 3 position components of our vertices.
        // The only difference here is that we need to account for the 3 color values in the stride variable.
        // Therefore, the stride contains the size of 6 floats instead of 3.
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof<float32>, 0)
        GL.EnableVertexAttribArray(0)
        
        // We create a new pointer for the color values.
        // Much like the previous pointer, we assign 6 in the stride value.
        // We also need to correctly set the offset to get the color values.
        // The color data starts after the position data, so the offset is the size of 3 floats.
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof<float32>, 3 * sizeof<float32>)
        GL.EnableVertexAttribArray(1)
        
        let mutable maxAttributeCount = 0
        GL.GetInteger(GetPName.MaxVertexAttribs, ref maxAttributeCount)
        System.Diagnostics.Debug.Print $"Maximum number of vertex attributes supported: {maxAttributeCount}"
        
        this._shader <- Shader("Shaders/shader.vert", "Shaders/shader.frag")        
        this._shader.Use()
        
    override this.OnRenderFrame(e) =
        base.OnRenderFrame e                
        GL.Clear(ClearBufferMask.ColorBufferBit)
        this._shader.Use()
        GL.BindVertexArray _vertexArrayObject
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3)
        this.SwapBuffers()
        
    override this.OnUpdateFrame(e) =
        base.OnUpdateFrame(e)
        let input = this.KeyboardState        
        if (input.IsKeyDown Keys.Escape) then            
            this.Close()
            
    override this.OnResize(e) =
        base.OnResize e        
        
        GL.Viewport(0, 0, e.Width, e.Height)