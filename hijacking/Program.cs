using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace hijacking
{
    internal static class Program
    {
        private static CameraDescriptor cameraDescriptor = new();

        private static ArrangementModel arrangementModel = new();

        private static IWindow window;

        private static IInputContext inputContext;

        private static GL Gl;

        private static ImGuiController controller;

        private static uint program;
        
        private static GlObject airbus;
        
        private static GlObject[] fighterJets = new GlObject[4];
        
        private static GlObject road;
        
        private static GlCube skyBox;

        private static float Shininess = 50;
        
        private static float movementSpeed = 2.0f;
        
        private static Dictionary<Key, bool> keyStates = new Dictionary<Key, bool>
        {
            { Key.Left, false },
            { Key.Right, false },
            { Key.Down, false },
            { Key.Up, false },
            { Key.A, false },
            { Key.D, false },
            { Key.Space, false }
        };


        private static float viewerPositionSpeed = 0.09f;

        private static float rotationAngle = 0f;
        
        private static bool isColidingWithRoad = false;
        
        private static bool isColidingWithFighterJets = false;
        private static bool planeMiss = false;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string TextureUniformVariableName = "uTexture";

        private const string LightColorVariableName = "lightColor";
        private const string LightPositionVariableName = "lightPos";
        private const string ViewPosVariableName = "viewPos";
        private const string ShininessVariableName = "shininess";

        private static bool pov = false;
        
        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Levi naon meno projektje";
            windowOptions.Size = new Vector2D<int>(1000, 1000);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();
        }

        private static void Window_Load()
        {
            //Console.WriteLine("Load");

            // set up input handling
            inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
                keyboard.KeyUp += Keyboard_KeyUp;

            }

            Gl = window.CreateOpenGL();

            controller = new ImGuiController(Gl, window, inputContext);

            // Handle resizes
            window.FramebufferResize += s =>
            {
                // Adjust the viewport to the new window size
                Gl.Viewport(s);
            };


            Gl.ClearColor(System.Drawing.Color.Black);

            SetUpObjects();

            LinkProgram();

            cameraDescriptor.setMovementSpeed(arrangementModel.getMovementSpeed());
            cameraDescriptor.setTurningSpeed(arrangementModel.getTurningSpeed());
           //Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);
        }

        private static void LinkProgram()
        {
            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, ReadShader("VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, ReadShader("FragmentShader.frag"));
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
        }

        private static string ReadShader(string shaderFileName)
        {
            using (Stream shaderStream = typeof(Program).Assembly.GetManifestResourceStream("hijacking.Shaders." + shaderFileName))
            using (StreamReader shaderReader = new StreamReader(shaderStream))
                return shaderReader.ReadToEnd();
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            if (keyStates.ContainsKey(key))
            {
                keyStates[key] = true;
            }
        }

        private static void Keyboard_KeyUp(IKeyboard keyboard, Key key, int arg3)
        {
            if (keyStates.ContainsKey(key))
            {
                keyStates[key] = false;
            }
        }
        

        private static void Window_Update(double deltaTime)
        {
            // multithreaded
            // make sure it is threadsafe
            // NO GL calls
            PilotControl();
            
            arrangementModel.AdvanceTime();
            cameraDescriptor.MoveForward();

            Vector3D<float> airbusPosition = arrangementModel.airplaneTranslation;
            Hitbox airbusHitbox = airbus.Hitbox.Translated(airbusPosition);
            
            if (arrangementModel.getColifingWithFighterJet() == -1)
            {
                Vector3D<float> roadPosition = arrangementModel.roadPosition;
                Hitbox roadHitbox = road.Hitbox.Translated(roadPosition);
                if (airbusHitbox.IsColliding(roadHitbox))
                {
                    isColidingWithRoad = true;
                    arrangementModel.setColidingWithRoad();
                    cameraDescriptor.SetColidingWithRoad();
                }
            }
            for (int i = 0; i < 4; i++)
            {
                Vector3D<float> jetPosition = arrangementModel.aircraftPosition[i];
                Hitbox jetHitbox = fighterJets[i].Hitbox.Translated(jetPosition);

                if (airbusHitbox.IsColliding(jetHitbox))
                {
                    isColidingWithFighterJets = true;
                    arrangementModel.setColidingWithFighterJet(i);
                    cameraDescriptor.SetColidingWithFighterJets();
                }
            }

            if (airbusPosition.Y < arrangementModel.roadPosition.Y && !planeMiss)
            {
                planeMiss = true;
            }

            controller.Update((float)deltaTime);
        }



        private static unsafe void Window_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s].");

            // GL here
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);


            Gl.UseProgram(program);

            SetViewMatrix();
            SetProjectionMatrix();

            SetLightColor();
            SetLightPosition();
            SetViewerPosition();
            SetShininess();

            DrawAirbus();
            DrawSkyBox();
            DrawRoad();
            DrawFighter();

            if (isColidingWithFighterJets)
            {
                ImGui.SetNextWindowPos(new Vector2(window.Size.X / 2 - 100, window.Size.Y / 2 - 50));
                ImGui.Begin("Game Over");
                ImGui.Text("You have crashed into a fighter jet");
                ImGui.End();
            }

            if (isColidingWithRoad)
            {
                ImGui.SetNextWindowPos(new Vector2(window.Size.X / 2 - 100, window.Size.Y / 2 - 50));
                ImGui.Begin("Game Over");
                ImGui.Text("Congratulations you have landed safely!");
                ImGui.End();
            }
            
            if (planeMiss)
            {
                ImGui.SetNextWindowPos(new Vector2(window.Size.X / 2 - 100, window.Size.Y / 2 - 50));
                ImGui.Begin("Game Over");
                ImGui.Text("You have missed the road");
                ImGui.End();
            }

            //ImGuiNET.ImGui.ShowDemoWindow();
            ImGui.SetNextWindowPos(new Vector2(0,0));
            ImGuiNET.ImGui.Begin("Pilot Position",
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            if (ImGui.RadioButton("FPV", pov))
            {
                if (!pov)
                {
                    cameraDescriptor.SetFPV();
                }
                pov = true;
                
                Console.WriteLine("POV selected");
            }
            ImGui.SameLine();
            if (ImGui.RadioButton("TPV", !pov))
            {
                if (pov)
                {
                    cameraDescriptor.SetTPV();
                }
                pov = false;
                Console.WriteLine("TPV selected");
            }
            ImGuiNET.ImGui.End();
            
            ImGui.SetNextWindowPos(new Vector2(0, 50));
            // set a bar for setting the movementSpeed min speed is 1.5 and max speed is 6.0
            ImGui.Begin("Movement Speed");
            ImGui.SliderFloat("Movement Speed", ref movementSpeed, 1.5f, 6.0f);
            arrangementModel.setMovementSpeed(movementSpeed);
            cameraDescriptor.setMovementSpeed(movementSpeed);

            controller.Render();
        }

        private static unsafe void SetLightColor()
        {
            int location = Gl.GetUniformLocation(program, LightColorVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightColorVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 1f, 1f, 1f);
            CheckError();
        }

        private static unsafe void SetLightPosition()
        {
            int location = Gl.GetUniformLocation(program, LightPositionVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightPositionVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 0f, 200f, 0f);
            CheckError();
        }

        private static unsafe void SetViewerPosition()
        {
            int location = Gl.GetUniformLocation(program, ViewPosVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewPosVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, cameraDescriptor.Position.X, cameraDescriptor.Position.Y, cameraDescriptor.Position.Z);
            CheckError();
        }

        private static unsafe void SetShininess()
        {
            int location = Gl.GetUniformLocation(program, ShininessVariableName);

            if (location == -1)
            {
                throw new Exception($"{ShininessVariableName} uniform not found on shader.");
            }

            Gl.Uniform1(location, Shininess);
            CheckError();
        }

        private static unsafe void DrawSkyBox()
        {
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(40000f);
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(skyBox.Vao);

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, skyBox.Texture.Value);

            Gl.DrawElements(GLEnum.Triangles, skyBox.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }
        
        private static unsafe void DrawRoad()
        {
            var translationMatrix = Matrix4X4.CreateTranslation(arrangementModel.roadPosition);
            var modelMatrix =  translationMatrix;
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(road.Vao);

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, road.Texture.Value);

            Gl.DrawElements(GLEnum.Triangles, road.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }
        
        private static unsafe void DrawAirbus()
        {
            if (arrangementModel.getColifingWithFighterJet() == -1)
            {
                var translationMatrix = Matrix4X4.CreateTranslation(arrangementModel.airplaneTranslation);
                //var rotationMatrix = Matrix4X4.CreateRotationX((float)-Math.PI/2);
                var modelMatrixForCenterCube = translationMatrix;
                //var modelMatrixForCenterCube = translationMatrix;
                SetModelMatrix(modelMatrixForCenterCube);
                Gl.BindVertexArray(airbus.Vao);
            }
            else if (arrangementModel.getColifingWithFighterJet() != -1)
            {   
                var lookDownRotationMatrix = Matrix4X4.CreateRotationX((float)-Math.PI / 2);
                var translationMatrix = Matrix4X4.CreateTranslation(arrangementModel.airplaneTranslation);
                var rotationMatrix = Matrix4X4.CreateRotationZ(rotationAngle);
                var modelMatrixForCenterCube = rotationMatrix * lookDownRotationMatrix * translationMatrix;
                SetModelMatrix(modelMatrixForCenterCube);
                Gl.BindVertexArray(airbus.Vao);
            }

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, airbus.Texture.Value);

            Gl.DrawElements(GLEnum.Triangles, airbus.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }
        
        private static unsafe void DrawFighter()
        {
            rotationAngle += 0.02f;
            // set material uniform to rubber
            for (int i = 0; i < fighterJets.Length; i++)
            {
                var translationMatrix = Matrix4X4.CreateTranslation(arrangementModel.aircraftPosition[i]);

                if (i == arrangementModel.getColifingWithFighterJet())
                {
                    // Apply rotation to the specific fighter jet
                    var lookDownRotationMatrix = Matrix4X4.CreateRotationX((float)-Math.PI / 2);
                    var rotationMatrix = Matrix4X4.CreateRotationZ(rotationAngle);  
                    var modelMatrixForCenterCube = rotationMatrix * lookDownRotationMatrix * translationMatrix;
                    SetModelMatrix(modelMatrixForCenterCube);
                    Gl.BindVertexArray(fighterJets[i].Vao);
                }
                else
                {
                    var modelMatrixForCenterCube = translationMatrix;
                    SetModelMatrix(modelMatrixForCenterCube);
                    Gl.BindVertexArray(fighterJets[i].Vao);
                }
                
                int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
                if (textureLocation == -1)
                {
                    throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
                }
                // set texture 0
                Gl.Uniform1(textureLocation, 0);

                Gl.ActiveTexture(TextureUnit.Texture0);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
                Gl.BindTexture(TextureTarget.Texture2D, fighterJets[i].Texture.Value);

                Gl.DrawElements(GLEnum.Triangles, fighterJets[i].IndexArrayLength, GLEnum.UnsignedInt, null);
                Gl.BindVertexArray(0);

                CheckError();
                Gl.BindTexture(TextureTarget.Texture2D, 0);
                CheckError();
            }
        }
        
        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            int location = Gl.GetUniformLocation(program, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError();

            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
            location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetUpObjects()
        {
            airbus = ObjResourceReader.CreateAirbus(Gl);
            skyBox = GlCube.CreateInteriorCube(Gl);
            road = ObjResourceReader.CreateRoad(Gl);
            arrangementModel = new();
            for (int i = 0; i < fighterJets.Length; i++)
            {
                fighterJets[i] = ObjResourceReader.CreateFighterJet(Gl);
            }
        }
        private static void Window_Closing()
        {
            airbus.ReleaseGlObject();
            skyBox.ReleaseGlObject();
            road.ReleaseGlObject();
            for (int i = 0; i < fighterJets.Length; i++)
            {
                fighterJets[i].ReleaseGlObject();
            }
        }

        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 100000);
            int location = Gl.GetUniformLocation(program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError();
        }

        private static unsafe void SetViewMatrix()
        {
            var viewMatrix = Matrix4X4.CreateLookAt(cameraDescriptor.Position, cameraDescriptor.Target, cameraDescriptor.UpVector);
            int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }

        private static void PilotControl()
        {
            if (keyStates[Key.Left])
            {
                cameraDescriptor.RotateLeft();
            }
            if (keyStates[Key.Right])
            {
                cameraDescriptor.RotateRight();
            }
            if (keyStates[Key.Down])
            {
                cameraDescriptor.RotateDown();
            }
            if (keyStates[Key.Up])
            {
                cameraDescriptor.RotateUp();
            }
            if (keyStates[Key.A])
            {
                arrangementModel.TurnLeft();
                cameraDescriptor.MoveLeft();
            }
            if (keyStates[Key.D])
            {
                arrangementModel.TurnRight();
                cameraDescriptor.MoveRight();
            }
            if (keyStates[Key.Space])
            {
                cameraDescriptor.SetLanding();
                arrangementModel.setLand();
            }
        }

    }
}