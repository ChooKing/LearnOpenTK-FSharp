namespace LearnOpenTK.Common

open System.IO
open OpenTK.Graphics.OpenGL4
open OpenTK.Mathematics

// A simple class meant to help create shaders.

// This is how you create a simple shader.
// Shaders are written in GLSL, which is a language very similar to C in its semantics.
// The GLSL source is compiled *at runtime*, so it can optimize itself for the graphics card it's currently being used on.
// A commented example of GLSL can be found in shader.vert.
type Shader(vertPath:string, fragPath:string) =
    //These values are only declared and not initialized yet in the original C# version
    let mutable Handle = 0         
    let mutable _uniformLocations = Map.empty<string, int> 
    do
        // There are several different types of shaders, but the only two you need for basic rendering are the vertex and fragment shaders.
        // The vertex shader is responsible for moving around vertices, and uploading that data to the fragment shader.
        // The vertex shader won't be too important here, but they'll be more important later.
        // The fragment shader is responsible for then converting the vertices to "fragments", which represent all the data OpenGL needs to draw a pixel.
        // The fragment shader is what we'll be using the most here.

        // Load vertex shader and compile
        let mutable shaderSource = File.ReadAllText(vertPath)        
        
        // GL.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
        let vertexShader = GL.CreateShader(ShaderType.VertexShader)
        
        // Now, bind the GLSL source code
        GL.ShaderSource(vertexShader, shaderSource)
        
        // And then compile
        GL.CompileShader(vertexShader)
        
        // We do the same for the fragment shader.
        shaderSource <- File.ReadAllText(fragPath)        
        let fragmentShader = GL.CreateShader(ShaderType.FragmentShader)
        GL.ShaderSource(fragmentShader, shaderSource)
        GL.CompileShader(fragmentShader)
        
        // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
        // To do this, create a program...
        Handle <- GL.CreateProgram()
        
        // Attach both shaders...
        GL.AttachShader(Handle, vertexShader)
        GL.AttachShader(Handle, fragmentShader)
        
        // And then link them together.
        GL.LinkProgram(Handle)
        
        // When the shader program is linked, it no longer needs the individual shaders attached to it; the compiled code is copied into the shader program.
        // Detach them, and then delete them.
        GL.DetachShader(Handle, vertexShader)
        GL.DetachShader(Handle, fragmentShader)
        GL.DeleteShader(fragmentShader)
        GL.DeleteShader(vertexShader)
        
        // The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
        // Querying this from the shader is very slow, so we do it once on initialization and reuse those values
        // later.

        // First, we have to get the number of active uniforms in the shader.
        let mutable numberOfUniforms = 0
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, ref numberOfUniforms)
        
        // The original C# initializes the _uniformLocations Dictionary here
        // Omitted in the F# because it was already initialized above        

        // The variable _i and _aut do not exist in the original C#
        // These values are discarded via use of underscores when calling GL.GetActiveUniform
        let mutable _i = 0 // Discarded value
        let mutable _aut:ActiveUniformType = ActiveUniformType.Float // Discarded value
        for i in 0..numberOfUniforms - 1 do
            // Original C# code discards two out parameters GL.GetActiveUniform(Handle, i, out _, out _)
            // get the name of this uniform,
            let key = GL.GetActiveUniform(Handle, i, ref _i, ref _aut)
            
            // get the location,
            let location = GL.GetUniformLocation(Handle, key)
            
            // and then add it to the dictionary.
            _uniformLocations <- _uniformLocations.Add(key, location)
            
    member this.compileShader(shader:int) =
        // Try to compile the shader
        GL.CompileShader(shader)
        
        // Check for compilation errors
        let mutable code = 0
        GL.GetShader(shader, ShaderParameter.CompileStatus, ref code)
        if code <> int All.True then
            // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
            let infoLog = GL.GetShaderInfoLog shader
            raise (System.Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}"))
            
    member this.linkProgram(program:int) =
        // We link the program
        GL.LinkProgram(program)
        
        // Check for linking errors
        let mutable code = 0
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, ref code)
        if code <> int All.True then
            // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
            raise (System.Exception($"Error occurred whilst linking Program({program})"))
            
    // A wrapper function that enables the shader program.
    member this.Use() =
        GL.UseProgram(Handle)
        
    // The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
    // you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
    member this.GetAttribLocation(attribName:string) =
        GL.GetAttribLocation(Handle, attribName)
        
    // Uniform setters
    // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
    // You use VBOs for vertex-related data, and uniforms for almost everything else.

    // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
    //     1. Bind the program you want to set the uniform on
    //     2. Get a handle to the location of the uniform with GL.GetUniformLocation.
    //     3. Use the appropriate GL.Uniform* function to set the uniform.

    /// <summary>
    /// Set a uniform int on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    member this.SetInt(name:string, data:int) =
        GL.UseProgram(Handle)
        GL.Uniform1(_uniformLocations[name], data)
        
    /// <summary>
    /// Set a uniform float on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    member this.SetFloat(name:string, data:float) =
        GL.UseProgram(Handle)
        GL.Uniform1(_uniformLocations[name], data)
        
    /// <summary>
    /// Set a uniform Matrix4 on this shader
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    /// <remarks>
    ///   <para>
    ///   The matrix is transposed before being sent to the shader.
    ///   </para>
    /// </remarks>
    member this.SetMatrix4(name:string, data:Matrix4) =
        GL.UseProgram(Handle)
        GL.UniformMatrix4(_uniformLocations[name], true, ref data)
        
    /// <summary>
    /// Set a uniform Vector3 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    member this.SetVector3(name:string, data:Vector3) =
        GL.UseProgram(Handle)
        GL.Uniform3(_uniformLocations[name], data)
            
            
    
    
    
    
        
        
        
        