using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.IO;


namespace lab4
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Spotlight on Pyramid",
            };

            using (var game = new Game(gameWindowSettings, nativeWindowSettings))
            {
                game.Run();
            }
        }
    }

    public class Game : GameWindow
    {

        // Загрузка шейдеров из файла
        private string LoadShaderSource(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке шейдера из файла {filePath}: {ex.Message}");
                throw;
            }
        }
        private int _shaderProgram;
        private int _vao;
        private Vector3 _lightDir = new Vector3(0.0f, -1.0f, -1.0f);
        private float _cutoffAngle = MathF.Cos(MathHelper.DegreesToRadians(30));

        private string VertexShaderSource => LoadShaderSource("vertexShader.glsl");

        private string FragmentShaderSource => LoadShaderSource("fragmentShader.glsl");

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            _shaderProgram = CreateShaderProgram(VertexShaderSource, FragmentShaderSource);

            float[] vertices = {
                 0.0f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f,
                -0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
                 0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,

                 0.0f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f,
                 0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
                 0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,

                 0.0f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f,
                 0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
                -0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,

                 0.0f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f,
                -0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
                -0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f
            };

            int vbo = GL.GenBuffer();
            _vao = GL.GenVertexArray();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        // Создание программы шейдеров
        private int CreateShaderProgram(string vertexShaderSource, string fragmentShaderSource)
        {
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            int shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shaderProgram);

            Matrix4 model = Matrix4.CreateRotationY((float)args.Time);
            Matrix4 view = Matrix4.LookAt(new Vector3(0, 1, 3), Vector3.Zero, Vector3.UnitY);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), Size.X / (float)Size.Y, 0.1f, 100.0f);

            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref projection);

            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "lightPos"), new Vector3(0, 1, 2));
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "lightDir"), _lightDir);
            GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "cutoffAngle"), _cutoffAngle);
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "lightColor"), new Vector3(1.0f, 1.0f, 1.0f));

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 12);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            float rotationSpeed = 2.0f;
            float cutoffSpeed = 1.0f;

            if (KeyboardState.IsKeyDown(Keys.W))
                _lightDir = Vector3.TransformNormal(_lightDir, Matrix4.CreateRotationX(rotationSpeed * (float)args.Time));
            if (KeyboardState.IsKeyDown(Keys.S))
                _lightDir = Vector3.TransformNormal(_lightDir, Matrix4.CreateRotationX(-rotationSpeed * (float)args.Time));
            if (KeyboardState.IsKeyDown(Keys.A))
                _lightDir = Vector3.TransformNormal(_lightDir, Matrix4.CreateRotationY(rotationSpeed * (float)args.Time));
            if (KeyboardState.IsKeyDown(Keys.D))
                _lightDir = Vector3.TransformNormal(_lightDir, Matrix4.CreateRotationY(-rotationSpeed * (float)args.Time));

            _lightDir = Vector3.Normalize(_lightDir);

            if (KeyboardState.IsKeyDown(Keys.Q))
                _cutoffAngle = MathF.Min(_cutoffAngle + cutoffSpeed * (float)args.Time, 1.0f);
            if (KeyboardState.IsKeyDown(Keys.E))
                _cutoffAngle = MathF.Max(_cutoffAngle - cutoffSpeed * (float)args.Time, 0.0f);
        }
    }
}
