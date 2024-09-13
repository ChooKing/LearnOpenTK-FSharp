module Window
open LearnOpenTK.Common
open OpenTK.Graphics.OpenGL4
open OpenTK.Mathematics
open OpenTK.Windowing.Common
open OpenTK.Windowing.GraphicsLibraryFramework
open OpenTK.Windowing.Desktop

// We can now move around objects. However, how can we move our "camera", or modify our perspective?
// In this tutorial, I'll show you how to setup a full projection/view/model (PVM) matrix.
// In addition, we'll make the rectangle rotate over time.
type Window(gameWindowSettings: GameWindowSettings, nativeWindowSettings: NativeWindowSettings) as this =
    inherit GameWindow(gameWindowSettings, nativeWindowSettings)
    let _vertices = [|
        // Position         Texture coordinates
         0.5f;  0.5f; 0.0f; 1.0f; 1.0f; // top right
         0.5f; -0.5f; 0.0f; 1.0f; 0.0f; // bottom right
        -0.5f; -0.5f; 0.0f; 0.0f; 0.0f; // bottom left
        -0.5f;  0.5f; 0.0f; 0.0f; 1.0f  // top left
    |]
    let _indices: uint32[] = [|
        0u; 1u; 3u;
        1u; 2u; 3u
    |]
    let mutable _elementBufferObject = 0
    let mutable _vertexBufferObject = 0
    let mutable _vertexArrayObject = 0
    [<DefaultValue>]
    val mutable private _shader: Shader    
    
    [<DefaultValue>]
    val mutable private _texture: Texture
    [<DefaultValue>]
    val mutable private _texture2: Texture
    
    // We create a double to hold how long has passed since the program was opened.
    [<DefaultValue>]
    val mutable private _time: double
    
    // Then, we create two matrices to hold our view and projection. They're initialized at the bottom of OnLoad.
    // The view matrix is what you might consider the "camera". It represents the current viewport in the window.
    [<DefaultValue>]
    val mutable private _view: Matrix4
    
    // This represents how the vertices will be projected. It's hard to explain through comments,
    // so check out the web version for a good demonstration of what this does.
    [<DefaultValue>]
    val mutable private _projection: Matrix4
    
    override this.OnLoad() =
        base.OnLoad()

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f)
        
        // We enable depth testing here. If you try to draw something more complex than one plane without this,
        // you'll notice that polygons further in the background will occasionally be drawn over the top of the ones in the foreground.
        // Obviously, we don't want this, so we enable depth testing. We also clear the depth buffer in GL.Clear over in OnRenderFrame.
        GL.Enable(EnableCap.DepthTest)        

        _vertexArrayObject <- GL.GenVertexArray()
        GL.BindVertexArray _vertexArrayObject

        _vertexBufferObject <- GL.GenBuffer()
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject)
        GL.BufferData(BufferTarget.ArrayBuffer, nativeint(_vertices.Length * sizeof<float32>), _vertices, BufferUsageHint.StaticDraw)

        _elementBufferObject <- GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof<uint32>, _indices, BufferUsageHint.StaticDraw)

        // shader.vert has been modified. Take a look at it after the explanation in OnRenderFrame.
        this._shader <- Shader("Shaders/shader.vert", "Shaders/shader.frag");
        this._shader.Use()

        let vertexLocation = this._shader.GetAttribLocation("aPosition")
        GL.EnableVertexAttribArray(vertexLocation);
        GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 0)
        
        let texCoordLocation = this._shader.GetAttribLocation("aTexCoord");
        GL.EnableVertexAttribArray(texCoordLocation);
        GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 3 * sizeof<float32>)

        this._texture <- Texture.LoadFromFile("Resources/container.png")        
        this._texture.Use(TextureUnit.Texture0)
        
        this._texture2 <- Texture.LoadFromFile("Resources/awesomeface.png")        
        this._texture2.Use(TextureUnit.Texture1)
                
        this._shader.SetInt("texture0", 0)
        this._shader.SetInt("texture1", 1)
        
        // For the view, we don't do too much here. Next tutorial will be all about a Camera class that will make it much easier to manipulate the view.
        // For now, we move it backwards three units on the Z axis.
        this._view <- Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f)
        
        // For the matrix, we use a few parameters.
        //   Field of view. This determines how much the viewport can see at once. 45 is considered the most "realistic" setting, but most video games nowadays use 90
        //   Aspect ratio. This should be set to Width / Height.
        //   Near-clipping. Any vertices closer to the camera than this value will be clipped.
        //   Far-clipping. Any vertices farther away from the camera than this value will be clipped.
        this._projection <- Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), float32 (this.Size.X / this.Size.Y), 0.1f, 100.0f)
        // Now, head over to OnRenderFrame to see how we setup the model matrix.
    override this.OnRenderFrame(e) =
        base.OnRenderFrame e
        
        // We add the time elapsed since last frame, times 4.0 to speed up animation, to the total amount of time passed.
        this._time <- this._time + (4.0 * e.Time)
        
        // We clear the depth buffer in addition to the color buffer.        
        GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        GL.BindVertexArray(_vertexArrayObject)
        
        this._texture.Use(TextureUnit.Texture0)
        this._texture2.Use(TextureUnit.Texture1)
        this._shader.Use()
        
        // Finally, we have the model matrix. This determines the position of the model.
        let model = Matrix4.Identity * Matrix4.CreateRotationX(float32 (MathHelper.DegreesToRadians this._time))
        
        // Then, we pass all of these matrices to the vertex shader.
        // You could also multiply them here and then pass, which is faster, but having the separate matrices available is used for some advanced effects.

        // IMPORTANT: OpenTK's matrix types are transposed from what OpenGL would expect - rows and columns are reversed.
        // They are then transposed properly when passed to the shader. 
        // This means that we retain the same multiplication order in both OpenTK c# code and GLSL shader code.
        // If you pass the individual matrices to the shader and multiply there, you have to do in the order "model * view * projection".
        // You can think like this: first apply the modelToWorld (aka model) matrix, then apply the worldToView (aka view) matrix, 
        // and finally apply the viewToProjectedSpace (aka projection) matrix.
        this._shader.SetMatrix4("model", model)
        this._shader.SetMatrix4("view", this._view);
        this._shader.SetMatrix4("projection", this._projection)
        
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0)
        this.SwapBuffers()
    override this.OnUpdateFrame(e) =
        base.OnUpdateFrame(e)
        let input = this.KeyboardState        
        if (input.IsKeyDown Keys.Escape) then            
            this.Close()
            
    override this.OnResize(e) =
        base.OnResize e
        GL.Viewport(0, 0, e.Width, e.Height)