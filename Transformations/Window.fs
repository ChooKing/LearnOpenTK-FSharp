module Window
open LearnOpenTK.Common
open OpenTK.Graphics.OpenGL4
open OpenTK.Mathematics
open OpenTK.Windowing.Common
open OpenTK.Windowing.GraphicsLibraryFramework
open OpenTK.Windowing.Desktop

// So you can setup OpenGL, you can draw basic shapes without wasting vertices, and you can texture them.
// There's one big thing left, though: moving the shapes.
// To do this, we use linear algebra to move the vertices in the vertex shader.

// Just as a disclaimer: this tutorial will NOT explain linear algebra or matrices; those topics are wayyyyy too complex to do with comments.
// If you want a more detailed understanding of what's going on here, look at the web version of this tutorial instead.
// A deep understanding of linear algebra won't be necessary for this tutorial as OpenTK includes built-in matrix types that abstract over the actual math.

// Head down to RenderFrame to see how we can apply transformations to our shape.
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

        // shader.vert has been modified, take a look at it as well.
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
                
        this._shader.SetInt("texture0", 0);
        this._shader.SetInt("texture1", 1)
    override this.OnRenderFrame(e) =
        base.OnRenderFrame e
        GL.Clear(ClearBufferMask.ColorBufferBit)
        GL.BindVertexArray(_vertexArrayObject)
        
        // Note: The matrices we'll use for transformations are all 4x4.
        // We start with an identity matrix. This is just a simple matrix that doesn't move the vertices at all.
        let mutable transform = Matrix4.Identity
        
        // The next few steps just show how to use OpenTK's matrix functions, and aren't necessary for the transform matrix to actually work.
        // If you want, you can just pass the identity matrix to the shader, though it won't affect the vertices at all.

        // A fact to note about matrices is that the order of multiplications matter. "matrixA * matrixB" and "matrixB * matrixA" mean different things.
        // A VERY important thing to know is that OpenTK matrices are so called row-major. We won't go into the full details here, but here is a good place to read more about it:
        // https://www.scratchapixel.com/lessons/mathematics-physics-for-computer-graphics/geometry/row-major-vs-column-major-vector
        // What it means for us is that we can think of matrix multiplication as going left to right.
        // So "rotate * translate" means rotate (around the origin) first and then translate, as opposed to "translate * rotate" which means translate and then rotate (around the origin).

        // To combine two matrices, you multiply them. Here, we combine the transform matrix with another one created by OpenTK to rotate it by 20 degrees.
        // Note that all Matrix4.CreateRotation functions take radians, not degrees. Use MathHelper.DegreesToRadians() to convert to radians, if you want to use degrees.
        transform <- transform * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(20f))
        
        // Next, we scale the matrix. This will make the rectangle slightly larger.
        transform <- transform * Matrix4.CreateScale(1.1f)
        
        this._texture.Use(TextureUnit.Texture0)
        this._texture2.Use(TextureUnit.Texture1)
        this._shader.Use()
        
        // Now that the matrix is finished, pass it to the vertex shader.
        // Go over to shader.vert to see how we finally apply this to the vertices.
        this._shader.SetMatrix4("transform", transform)
        
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