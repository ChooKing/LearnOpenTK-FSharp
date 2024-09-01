module Window

open System
open System.Diagnostics
open OpenTK.Graphics.OpenGL4
open LearnOpenTK.Common
open OpenTK.Windowing.Desktop
open OpenTK.Windowing.Common
open OpenTK.Windowing.GraphicsLibraryFramework

// This project will explore how to use uniform variable type which allows you to assign values
// to shaders at any point during the project.
type Window(gameWindowSettings, nativeWindowSettings) as this =
    inherit GameWindow(gameWindowSettings, nativeWindowSettings)
    let _vertices = [|
        -0.5f; -0.5f; 0.0f // Bottom-left vertex
        0.5f; -0.5f; 0.0f // Bottom-right vertex
        0.0f;  0.5f; 0.0f // Top vertex
    |]
    // So we're going make the triangle pulsate between a color range.
    // In order to do that, we'll need a constantly changing value.
    // The stopwatch is perfect for this as it is constantly going up.
    [<DefaultValue>]
    val mutable _timer: Stopwatch
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
        let mutable maxAttributeCount = 0
        GL.GetInteger(GetPName.MaxVertexAttribs, ref maxAttributeCount)
        System.Diagnostics.Debug.Print $"Maximum number of vertex attributes supported: {maxAttributeCount}"
        this._shader <- Shader("Shaders/shader.vert", "Shaders/shader.frag")
        this._shader.Use()
        
        // We start the stopwatch here as this method is only called once.
        this._timer <- Stopwatch()
        this._timer.Start()
        
    override this.OnRenderFrame(e) =
        base.OnRenderFrame e
        GL.Clear(ClearBufferMask.ColorBufferBit)
        this._shader.Use()
        
        // Here, we get the total seconds that have elapsed since the last time this method has reset
        // and we assign it to the timeValue variable so it can be used for the pulsating color.
        let timeValue = this._timer.Elapsed.TotalSeconds
        let greenValue = (float32 (Math.Sin timeValue)) / 2.0f + 0.5f
        let vertexColorLocation = GL.GetUniformLocation(this._shader.Handle, "ourColor")
        GL.Uniform4(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f)
        GL.BindVertexArray(_vertexArrayObject)
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
        