module Window
open LearnOpenTK.Common
open OpenTK.Graphics.OpenGL4
open OpenTK.Windowing.Common
open OpenTK.Windowing.GraphicsLibraryFramework
open OpenTK.Windowing.Desktop

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
    override this.OnLoad() =
        base.OnLoad()

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f)

        _vertexArrayObject <- GL.GenVertexArray()
        GL.BindVertexArray _vertexArrayObject

        _vertexBufferObject <- GL.GenBuffer()
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject)
        GL.BufferData(BufferTarget.ArrayBuffer, nativeint(_vertices.Length * sizeof<float32>), _vertices, BufferUsageHint.StaticDraw)

        _elementBufferObject <- GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof<uint32>, _indices, BufferUsageHint.StaticDraw)

        // shader.frag has been modified yet again, take a look at it as well.
        this._shader <- Shader("Shaders/shader.vert", "Shaders/shader.frag");
        this._shader.Use()

        let vertexLocation = this._shader.GetAttribLocation("aPosition")
        GL.EnableVertexAttribArray(vertexLocation);
        GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 0)
        
        let texCoordLocation = this._shader.GetAttribLocation("aTexCoord");
        GL.EnableVertexAttribArray(texCoordLocation);
        GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 3 * sizeof<float32>)

        this._texture <- Texture.LoadFromFile("Resources/container.png")
        // Texture units are explained in Texture.cs, at the Use function.
        // First texture goes in texture unit 0.
        this._texture.Use(TextureUnit.Texture0)
        
        // This is helpful because System.Drawing reads the pixels differently than OpenGL expects.
        this._texture2 <- Texture.LoadFromFile("Resources/awesomeface.png")
        // Then, the second goes in texture unit 1.
        this._texture2.Use(TextureUnit.Texture1)
        
        // Next, we must setup the samplers in the shaders to use the right textures.
        // The int we send to the uniform indicates which texture unit the sampler should use.
        this._shader.SetInt("texture0", 0);
        this._shader.SetInt("texture1", 1)
    override this.OnRenderFrame(e) =
        base.OnRenderFrame e
        GL.Clear(ClearBufferMask.ColorBufferBit)
        GL.BindVertexArray(_vertexArrayObject);
        this._texture.Use(TextureUnit.Texture0)
        this._texture2.Use(TextureUnit.Texture1)
        this._shader.Use()
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