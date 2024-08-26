module Window

open OpenTK.Graphics.OpenGL4
open OpenTK.Windowing.Common
open OpenTK.Windowing.GraphicsLibraryFramework
open OpenTK.Windowing.Desktop

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
    let mutable vertexShader = 0
    let mutable fragmentShader = 0
    let mutable program = 0
    let mutable _shader = 0
    //Vertex shader source string
    let vertexSource = """
        #version 330

        layout(location = 0) in vec3 aPosition;

        void main(void)
        {
            gl_Position = vec4(aPosition, 1.0);
        }
        """
//Fragment shader source string        
    let fragmentSource = """
        #version 330

        out vec4 outputColor;

        void main(void)
        {
	        outputColor = vec4(1.0, 1.0, 0.0, 1.0);
        }
        """
            
        
    override this.OnLoad() =
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f)
        _vertexBufferObject <- GL.GenBuffer()
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject)
        GL.BufferData(BufferTarget.ArrayBuffer, nativeint(_vertices.Length * sizeof<float32>), _vertices, BufferUsageHint.StaticDraw)
        _vertexArrayObject <- GL.GenVertexArray()
        GL.BindVertexArray _vertexArrayObject
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof<float32>, 0)
        GL.EnableVertexAttribArray 0
        vertexShader <-
            let shader = GL.CreateShader(ShaderType.VertexShader)
            GL.ShaderSource(shader, vertexSource)
            GL.CompileShader(shader)
            shader
        
        fragmentShader <-
            let shader = GL.CreateShader(ShaderType.FragmentShader)
            GL.ShaderSource(shader, fragmentSource)
            GL.CompileShader(shader)
            shader
                
        program <- 
            let program = GL.CreateProgram()
            GL.AttachShader(program, vertexShader)
            GL.AttachShader(program, fragmentShader)
            GL.LinkProgram(program)
            program
            
        GL.UseProgram program            
            
    override this.OnRenderFrame(e) =
        base.OnRenderFrame e
        GL.Clear(ClearBufferMask.ColorBufferBit)
        GL.UseProgram program
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
                  