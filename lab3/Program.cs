using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace lab3
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(1200, 900),
                Title = "3D Scene with Camera Transition",
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
        private Vector3 cameraPosition = new Vector3(0.0f, 0.0f, 4.2f); // Текущая позиция камеры
        private Vector3 cameraTarget = Vector3.Zero; // Цель, на которую смотрит камера

        // Список точек перехода камеры
        private Vector3[] cameraPositions = new Vector3[]
        {
            new Vector3(0.0f, 0.0f, 4.2f),
            new Vector3(3.0f, 3.0f, 4.2f),
            new Vector3(-3.0f, 2.0f, 4.2f),
            new Vector3(0.0f, -3.0f, -4.2f)
        };
        
        private int currentCameraPositionIndex = 0;
        private Vector3 startCameraPosition;
        private Vector3 endCameraPosition;
        private float transitionProgress = 0.0f; // Прогресс анимации (от 0.0 до 1.0)
        private bool isTransitioning = false;   // Флаг, указывающий на активность анимации
        private float transitionSpeed = 0.5f;

        private float pyramidRotationAngle = 0.0f;
        private float cylinderRotationAngle = 0.0f;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            // Установка матрицы проекции
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f),
                (float)Size.X / Size.Y,
                0.1f,
                100f
            );
            GL.LoadMatrix(ref perspective);

            GL.MatrixMode(MatrixMode.Modelview);
            UpdateCamera();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            // Обработка ввода для запуска анимации
            if (KeyboardState.IsKeyDown(Keys.Space) && !isTransitioning)
            {
                // Переключение направления перехода камеры
                currentCameraPositionIndex = (currentCameraPositionIndex + 1) % cameraPositions.Length; // Переход к следующей точке
                isTransitioning = true;
                transitionProgress = 0.0f; // Сброс прогресса анимации

                startCameraPosition = cameraPosition;
                endCameraPosition = cameraPositions[currentCameraPositionIndex];
            }

            // Управление скоростью перехода
            if (KeyboardState.IsKeyDown(Keys.W))
            {
                transitionSpeed += 0.05f;
            }
            if (KeyboardState.IsKeyDown(Keys.S))
            {
                transitionSpeed = MathHelper.Max(0.1f, transitionSpeed - 0.1f);
            }

            // Обновление состояния анимации
            if (isTransitioning)
            {
                transitionProgress += (float)args.Time * transitionSpeed;
                if (transitionProgress >= 1.0f)
                {
                    transitionProgress = 1.0f;
                    isTransitioning = false;
                }

                // Линейная интерполяция позиции камеры
                cameraPosition = Vector3.Lerp(startCameraPosition, endCameraPosition, transitionProgress);
                UpdateCamera();
            }

            pyramidRotationAngle += 30.0f * (float)args.Time;
            cylinderRotationAngle += 20.0f * (float)args.Time;
        }

        // Обновление матрицы вида камеры
        private void UpdateCamera()
        {
            GL.LoadIdentity();
            Matrix4 view = Matrix4.LookAt(cameraPosition, cameraTarget, Vector3.UnitY);
            GL.LoadMatrix(ref view);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            DrawScene();

            SwapBuffers();
        }

        // Отрисовка сцены
        private void DrawScene()
        {
            GL.PushMatrix();
            GL.Translate(-1.0f, 0.0f, 0.0f);
            GL.Rotate(pyramidRotationAngle, 0.0f, 1.0f, 0.0f); // Вращение пирамиды вокруг своей оси
            DrawPyramid();
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Translate(1.0f, 0.0f, 0.0f);
            GL.Rotate(cylinderRotationAngle, 0.0f, 1.0f, 0.0f); // Вращение цилиндра вокруг своей оси
            DrawCylinder();
            GL.PopMatrix();
        }

        private void DrawPyramid()
        {
            GL.Begin(PrimitiveType.Triangles);

            GL.Color3(1.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.5f, 0.0f);
            GL.Vertex3(-0.5f, -0.5f, 0.5f);
            GL.Vertex3(0.5f, -0.5f, 0.5f);

            GL.Color3(0.0f, 1.0f, 0.0f);
            GL.Vertex3(0.0f, 0.5f, 0.0f);
            GL.Vertex3(0.5f, -0.5f, 0.5f);
            GL.Vertex3(0.5f, -0.5f, -0.5f);

            GL.Color3(0.0f, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 0.5f, 0.0f);
            GL.Vertex3(0.5f, -0.5f, -0.5f);
            GL.Vertex3(-0.5f, -0.5f, -0.5f);

            GL.Color3(0.5f, 0.5f, 0.5f);
            GL.Vertex3(0.0f, 0.5f, 0.0f);
            GL.Vertex3(-0.5f, -0.5f, -0.5f);
            GL.Vertex3(-0.5f, -0.5f, 0.5f);

            GL.End();

            GL.Begin(PrimitiveType.Quads);
            GL.Color3(1.0f, 1.0f, 1.0f);

            GL.Vertex3(-0.5f, -0.5f, 0.5f);
            GL.Vertex3(0.5f, -0.5f, 0.5f);
            GL.Vertex3(0.5f, -0.5f, -0.5f);
            GL.Vertex3(-0.5f, -0.5f, -0.5f);

            GL.End();
        }

        private void DrawCylinder()
        {
            const int segments = 32;
            const float radius = 0.5f;
            const float height = 1.0f;

            // Нижнее основание цилиндра
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Color3(1.0f, 1.0f, 1.0f);
            GL.Vertex3(0.0f, -height / 2, 0.0f);
            for (int i = 0; i <= segments; i++)
            {
                double angle = i * 2.0 * Math.PI / segments;
                GL.Vertex3(Math.Cos(angle) * radius, -height / 2, Math.Sin(angle) * radius);
            }
            GL.End();

            // Верхнее основание цилиндра
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Color3(1.0f, 1.0f, 1.0f);
            GL.Vertex3(0.0f, height / 2, 0.0f);
            for (int i = 0; i <= segments; i++)
            {
                double angle = i * 2.0 * Math.PI / segments;
                GL.Vertex3(Math.Cos(angle) * radius, height / 2, Math.Sin(angle) * radius);
            }
            GL.End();

            // Боковая поверхность цилиндра с полосами
            GL.Begin(PrimitiveType.QuadStrip);
            for (int i = 0; i <= segments; i++)
            {
                double angle = i * 2.0 * Math.PI / segments;
                double x = Math.Cos(angle) * radius;
                double z = Math.Sin(angle) * radius;

                if (i % 2 == 0)
                    GL.Color3(0.5f, 0.5f, 0.5f); 
                else
                    GL.Color3(1.0f, 1.0f, 0.5f);

                GL.Vertex3(x, -height / 2, z);
                GL.Vertex3(x, height / 2, z);
            }
            GL.End();
        }
    }
}
