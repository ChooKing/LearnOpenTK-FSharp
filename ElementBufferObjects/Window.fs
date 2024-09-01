module Window
open LearnOpenTK.Common
open OpenTK.Graphics.OpenGL4
open OpenTK.Windowing.Common
open OpenTK.Windowing.GraphicsLibraryFramework
open OpenTK.Windowing.Desktop

// So you've drawn the first triangle. But what about drawing multiple?
// You may consider just adding more vertices to the array, and that would technically work, but say you're drawing a rectangle.
// It only needs four vertices, but since OpenGL works in triangles, you'd need to define 6.
// Not a huge deal, but it quickly adds up when you get to more complex models. For example, a cube only needs 8 vertices, but
// doing it that way would need 36 vertices!

// OpenGL provides a way to reuse vertices, which can heavily reduce memory usage on complex objects.
// This is called an Element Buffer Object. This tutorial will be all about how to set one up.
type Window(gameWindowSettings: GameWindowSettings, nativeWindowSettings: NativeWindowSettings) as this =
    inherit GameWindow(gameWindowSettings, nativeWindowSettings)
    
    // We modify the vertex array to include four vertices for our rectangle.
    let _vertices = [|
         0.5f;  0.5f; 0.0f; // top right
         0.5f; -0.5f; 0.0f; // bottom right
        -0.5f; -0.5f; 0.0f; // bottom left
        -0.5f;  0.5f; 0.0f // top left
    |]
    
    // Then, we create a new array: indices.
    // This array controls how the EBO will use those vertices to create triangles
    let _indices = [|
        // Note that indices start at 0!
        0; 1; 3; // The first triangle will be the top-right half of the triangle
        1; 2; 3  // Then the second will be the bottom-left half of the triangle
    |]
    let mutable _vertexBufferObject = 0
    let mutable _vertexArrayObject = 0
    [<DefaultValue>]
    val mutable public _shader: Shader    
    
    // Add a handle for the EBO
    let mutable _elementBufferObject = 0
    
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
        
        // We create/bind the Element Buffer Object EBO the same way as the VBO, except there is a major difference here which can be REALLY confusing.
        // The binding spot for ElementArrayBuffer is not actually a global binding spot like ArrayBuffer is. 
        // Instead it's actually a property of the currently bound VertexArrayObject, and binding an EBO with no VAO is undefined behaviour.
        // This also means that if you bind another VAO, the current ElementArrayBuffer is going to change with it.
        // Another sneaky part is that you don't need to unbind the buffer in ElementArrayBuffer as unbinding the VAO is going to do this,
        // and unbinding the EBO will remove it from the VAO instead of unbinding it like you would for VBOs or VAOs.
        _elementBufferObject <- GL.GenBuffer()
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject)
        
        // We also upload data to the EBO the same way as we did with VBOs.
        GL.BufferData(BufferTarget.ElementArrayBuffer, nativeint(_indices.Length * sizeof<float32>), _indices, BufferUsageHint.StaticDraw)
        // The EBO has now been properly setup. Go to the Render function to see how we draw our rectangle now!
        this._shader <- new Shader("Shaders/shader.vert", "Shaders/shader.frag")
        this._shader.Use()

    override this.OnRenderFrame(e: FrameEventArgs) =
        base.OnRenderFrame(e)
        GL.Clear(ClearBufferMask.ColorBufferBit)
        this._shader.Use()
        
        // Because ElementArrayObject is a property of the currently bound VAO,
        // the buffer you will find in the ElementArrayBuffer will change with the currently bound VAO.
        GL.BindVertexArray(_vertexArrayObject)
        
        // Then replace your call to DrawTriangles with one to DrawElements
        // Arguments:
        //   Primitive type to draw. Triangles in this case.
        //   How many indices should be drawn. Six in this case.
        //   Data type of the indices. The indices are an unsigned int, so we want that here too.
        //   Offset in the EBO. Set this to 0 because we want to draw the whole thing.
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0)
        
        this.SwapBuffers()
        
    override this.OnUpdateFrame(e: FrameEventArgs) =
        base.OnUpdateFrame(e)
        let input = this.KeyboardState        
        if (input.IsKeyDown Keys.Escape) then            
            this.Close()
            
    override this.OnResize(e: ResizeEventArgs) =
        base.OnResize(e)
        GL.Viewport(0, 0, e.Width, e.Height)