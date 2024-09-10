module Window

open LearnOpenTK.Common
open OpenTK.Graphics.OpenGL4
open OpenTK.Windowing.Common
open OpenTK.Windowing.GraphicsLibraryFramework
open OpenTK.Windowing.Desktop

type Window(gameWindowSettings, nativeWindowSettings) =
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
    
    // For documentation on this, check Texture.cs.
    [<DefaultValue>]
    val mutable private _texture: Texture
    
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

        // The shaders have been modified to include the texture coordinates, check them out after finishing the OnLoad function.
        this._shader <- Shader("Shaders/shader.vert", "Shaders/shader.frag");
        this._shader.Use()

        // Because there's now 5 floats between the start of the first vertex and the start of the second,
        // we modify the stride from 3 * sizeof(float) to 5 * sizeof(float).
        // This will now pass the new vertex array to the buffer.
        let vertexLocation = this._shader.GetAttribLocation("aPosition")
        GL.EnableVertexAttribArray(vertexLocation);
        GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 0)

        // Next, we also setup texture coordinates. It works in much the same way.
        // We add an offset of 3, since the texture coordinates comes after the position data.
        // We also change the amount of data to 2 because there's only 2 floats for texture coordinates.
        let texCoordLocation = this._shader.GetAttribLocation("aTexCoord");
        GL.EnableVertexAttribArray(texCoordLocation);
        GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 3 * sizeof<float32>)

        this._texture <- Texture.LoadFromFile("Resources/container.png");
        this._texture.Use(TextureUnit.Texture0)

    override this.OnRenderFrame(e) =
        base.OnRenderFrame e
        GL.Clear(ClearBufferMask.ColorBufferBit)
        GL.BindVertexArray(_vertexArrayObject);
        this._texture.Use(TextureUnit.Texture0)
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