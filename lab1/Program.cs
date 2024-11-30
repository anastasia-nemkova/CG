using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace lab1
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameWindowSettings = GameWindowSettings.Default;

            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(1280, 1280),
                Title = "Ellipse Drawing and Transformation",
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
        private float baseSemiMajorAxis = 0.6f; // Базовая длина большой полуоси
        private float baseSemiMinorAxis = 0.4f; // Базовая длина малой полуоси
        private float semiMajorAxis; // Текущая длина большой полуоси
        private float semiMinorAxis; // Текущая длина малой полуоси

        private int segments = 100; // Количество сегментов для рисования эллипса

        private float scale = 1.0f; // Масштаб эллипса
        private float rotation = 0.0f; // Угол поворота эллипса

        private float time = 0.0f; // Время для динамического изменения

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        // Установка начальных параметров
        protected override void OnLoad() 
        {
            base.OnLoad();
            GL.ClearColor(0.1f, 0.1f, 0.2f, 1.0f); 

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, -1.0, 1.0);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        // Обновление полуосей эллипса по времени
        private void UpdateSemiAxes(float deltaTime)
        {
            time += deltaTime;

            // Применяем гармоническое колебание для изменения полуосей
            semiMajorAxis = baseSemiMajorAxis + 0.2f * MathF.Sin(time);
            semiMinorAxis = baseSemiMinorAxis + 0.1f * MathF.Cos(time);
        }

        private void DrawEllipse(float centerX, float centerY, float semiMajor, float semiMinor, int segments)
        {
            GL.Begin(PrimitiveType.LineLoop);
            GL.Color3(1.0f, 1.0f, 1.0f);

            for (int i = 0; i < segments; i++)
            {
                float angle = 2.0f * MathF.PI * i / segments;
                float x = semiMajor * MathF.Cos(angle); 
                float y = semiMinor * MathF.Sin(angle); 

                // Применяем масштаб и поворот
                float rotatedX = x * MathF.Cos(rotation) - y * MathF.Sin(rotation);
                float rotatedY = x * MathF.Sin(rotation) + y * MathF.Cos(rotation);

                // Применяем центр и масштаб
                GL.Vertex2(centerX + rotatedX * scale, centerY + rotatedY * scale);
            }

            GL.End();
        }

        // Рендеринг
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Обновляем параметры полуосей
            UpdateSemiAxes((float)args.Time);

            DrawEllipse(0.0f, 0.0f, semiMajorAxis, semiMinorAxis, segments);

            SwapBuffers();
        }

        // Обновление состояния
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            var input = KeyboardState;

            // Управление масштабом
            if (input.IsKeyDown(Keys.W))
                scale += 0.01f;
            if (input.IsKeyDown(Keys.S))
                scale = MathF.Max(0.1f, scale - 0.01f);

            // Управление поворотом
            if (input.IsKeyDown(Keys.A))
                rotation += 0.005f;
            if (input.IsKeyDown(Keys.D))
                rotation -= 0.005f;
        }
    }
}