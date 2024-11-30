using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace lab2
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameWindowSettings = GameWindowSettings.Default;

            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 800),
                Title = "3D Cube",
                APIVersion = new Version(3, 3),
                Profile = ContextProfile.Compatability
            };

            using (var game = new Game(gameWindowSettings, nativeWindowSettings))
            {
                game.Run();
            }
        }
    }

    public class Game : GameWindow
    {
        private float angleX = 0.0f; // Угол вращения вокруг оси X
        private float angleY = 0.0f; // Угол вращения вокруг оси Y
        private float angleZ = 0.0f; // Угол вращения вокруг оси Z

        private float rotationSpeed = 15.0f; // Текущая скорость вращения
        private float rotationDirectionX = 0.0f; // Направление вращения по оси X
        private float rotationDirectionY = 0.0f; // Направление вращения по оси Y
        private float rotationDirectionZ = 0.0f; // Направление вращения по оси Z

        private bool isPerspective = false; // Текущий режим проекции (по умолчанию ортографический)

        private float maxRotationSpeed = 70.0f; // Максимальная скорость вращения
        private float minRotationSpeed = 5.0f;  // Минимальная скорость вращения

        private float[] cubeVertices =
        {
            // Передняя сторона
            -0.5f, -0.5f,  0.5f, 1.0f, 0.0f, 0.0f,
             0.5f, -0.5f,  0.5f, 1.0f, 0.0f, 0.0f,
             0.5f,  0.5f,  0.5f, 1.0f, 0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f, 1.0f, 0.0f, 0.0f,

            // Задняя сторона
            -0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
             0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
             0.5f,  0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f, 0.0f, 1.0f, 0.0f,

            // Верхняя сторона
            -0.5f,  0.5f,  0.5f, 0.0f, 0.0f, 1.0f,
             0.5f,  0.5f,  0.5f, 0.0f, 0.0f, 1.0f,
             0.5f,  0.5f, -0.5f, 0.0f, 0.0f, 1.0f,
            -0.5f,  0.5f, -0.5f, 0.0f, 0.0f, 1.0f,

            // Нижняя сторона
            -0.5f, -0.5f,  0.5f, 0.0f, 1.0f, 1.0f,
             0.5f, -0.5f,  0.5f, 0.0f, 1.0f, 1.0f,
             0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 1.0f,

            // Левая сторона
            -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 0.0f,

            // Правая сторона
             0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f,
             0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 1.0f,
             0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 1.0f,
             0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f
        };

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        // Начальные параметры
        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.1f, 0.1f, 0.1f, 0.1f);
            GL.Enable(EnableCap.DepthTest);
            SetProjection();
        }

        // Установка проекции
        private void SetProjection()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            if (isPerspective)
            {
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(45.0f), 
                    (float)Size.X / Size.Y, 
                    0.1f, 
                    100.0f);
                GL.LoadMatrix(ref projection);
            }
            else
            {
                Matrix4 projection = Matrix4.CreateOrthographic(2.0f, 2.0f, 0.1f, 100.0f);
                GL.LoadMatrix(ref projection);
            }

            GL.MatrixMode(MatrixMode.Modelview);
        }

        private void DrawCube()
        {
            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < cubeVertices.Length; i += 6)
            {
                GL.Color3(cubeVertices[i + 3], cubeVertices[i + 4], cubeVertices[i + 5]);
                GL.Vertex3(cubeVertices[i], cubeVertices[i + 1], cubeVertices[i + 2]);
            }
            GL.End();
        }

        // Рендеринг
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.LoadIdentity();
            GL.Translate(0.0f, 0.0f, -3.0f); // Отдаляем объект от камеры

            // Применяем вращение
            angleX += rotationSpeed * rotationDirectionX * (float)args.Time;
            angleY += rotationSpeed * rotationDirectionY * (float)args.Time;
            angleZ += rotationSpeed * rotationDirectionZ * (float)args.Time;

            GL.Rotate(angleX, 1.0f, 0.0f, 0.0f);
            GL.Rotate(angleY, 0.0f, 1.0f, 0.0f);
            GL.Rotate(angleZ, 0.0f, 0.0f, 1.0f);

            DrawCube();

            SwapBuffers();
        }

        // Обновление состояния
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            // Изменение скорости вращения
            if (KeyboardState.IsKeyDown(Keys.Up)) rotationSpeed = Math.Min(rotationSpeed + 0.5f, maxRotationSpeed);
            if (KeyboardState.IsKeyDown(Keys.Down)) rotationSpeed = Math.Max(rotationSpeed - 0.1f, minRotationSpeed);

            // Переключение проекции
            if (KeyboardState.IsKeyDown(Keys.P))
            {
                isPerspective = !isPerspective;
                SetProjection();
            }

            // Управление вращением с клавиш
            if (KeyboardState.IsKeyDown(Keys.W)) rotationDirectionX = 1.0f;
            if (KeyboardState.IsKeyDown(Keys.S)) rotationDirectionX = -1.0f;
            if (KeyboardState.IsKeyDown(Keys.A)) rotationDirectionY = 1.0f;
            if (KeyboardState.IsKeyDown(Keys.D)) rotationDirectionY = -1.0f;
            if (KeyboardState.IsKeyDown(Keys.Q)) rotationDirectionZ = 1.0f;
            if (KeyboardState.IsKeyDown(Keys.E)) rotationDirectionZ = -1.0f;
        }

    }
}
