using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace RayTracing
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameWindowSettings = GameWindowSettings.Default;

            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(1200, 1200),
                Title = "Ray Tracing with Reflections and Refractions",
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
        private Vector3 cameraPosition = new Vector3(0.0f, 2.0f, -8.0f);
        private Vector3 lightPosition = new Vector3(0.0f, 5.0f, -5.0f);
        private float dx = 0.0f, dy = 0.0f, dz = -2.5f;

        private float MaxDepth = 5;

        // Свойства материала
        public class Material
        {
            public float Reflectivity { get; set; }
            public float RefractionIndex { get; set; }
            public Vector3 Color { get; set; }

            public Material(float reflectivity, float refractionIndex, Vector3 color)
            {
                Reflectivity = reflectivity;
                RefractionIndex = refractionIndex;
                Color = color;
            }
        }

        public class Sphere
        {
            public Vector3 Center { get; set; }
            public float Radius { get; set; }
            public Material Material { get; set; }

            public Sphere(Vector3 center, float radius, Material material)
            {
                Center = center;
                Radius = radius;
                Material = material;
            }
        }

        public class Plane
        {
            public Vector3 Point { get; set; }
            public Vector3 Normal { get; set; }
            public Material Material { get; set; }

            public Plane(Vector3 point, Vector3 normal, Material material)
            {
                Point = point;
                Normal = normal;
                Material = material;
            }
        }

        private Sphere[] spheres;
        private Plane plane;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.06f, 0.3f, 0.5f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)1200 / 1200, 0.1f, 500.0f);
            Matrix4 view = Matrix4.LookAt(cameraPosition, Vector3.Zero, Vector3.UnitY);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref view);

            Material reflectiveMaterial = new Material(0.8f, 1.0f, new Vector3(0.8f, 0.8f, 0.8f));
            Material refractiveMaterial = new Material(0.3f, 1.5f, new Vector3(0.5f, 0.5f, 1.0f));
            Material matteMaterial = new Material(0.0f, 1.0f, new Vector3(0.2f, 0.5f, 0.0f));

            // Инициализация сфер и плоскости
            spheres = new Sphere[]
            {
                new Sphere(new Vector3(0, 1.5f, 0), 1.5f, reflectiveMaterial),
                new Sphere(new Vector3(-3.5f, 1.5f, 0), 1.5f, refractiveMaterial),
                new Sphere(new Vector3(3.5f, 1.5f, 0), 1.5f, matteMaterial)
            };

            plane = new Plane(new Vector3(0, 0.0f, 0.0f), new Vector3(0, 1, 0), matteMaterial);
        }

        private bool IntersectSphere(Vector3 rayOrigin, Vector3 rayDir, Sphere sphere, out float t)
        {
            t = 0.0f;
            Vector3 oc = rayOrigin - sphere.Center;
            float a = Vector3.Dot(rayDir, rayDir);
            float b = 2.0f * Vector3.Dot(oc, rayDir);
            float c = Vector3.Dot(oc, oc) - sphere.Radius * sphere.Radius;
            float discriminant = b * b - 4.0f * a * c;

            if (discriminant > 0)
            {
                t = (-b - (float)Math.Sqrt(discriminant)) / (2.0f * a);
                if (t > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IntersectPlane(Vector3 rayOrigin, Vector3 rayDir, Plane plane, out float t)
        {
            t = 0.0f;
            float denom = Vector3.Dot(plane.Normal, rayDir);
            if (Math.Abs(denom) > 1e-6f)
            {
                Vector3 p0l0 = plane.Point - rayOrigin;
                t = Vector3.Dot(p0l0, plane.Normal) / denom;
                return (t > 0);
            }
            return false;
        }

        private bool IsInShadow(Vector3 hitPoint, Vector3 lightPosition)
        {
            // Направление от точки попадания до источника света
            Vector3 lightDir = Vector3.Normalize(lightPosition - hitPoint);
            float t; 

            // Проходим по всем сферам и проверяем, есть ли что-то между точкой попадания и источником света
            foreach (var sphere in spheres)
            {
                if (IntersectSphere(hitPoint + lightDir * 0.001f, lightDir, sphere, out t))  // Проверка на пересечение луча с шаром
                {
                    return true;
                }
            }

            // Проверка на пересечение с плоскостью
            if (IntersectPlane(hitPoint + lightDir * 0.001f, lightDir, plane, out t))
            {
                return true;
            }

            return false;
        }

        private Vector3 RayTrace(Vector3 rayOrigin, Vector3 rayDir, int depth)
        {
            if (depth > MaxDepth) return new Vector3(0.06f, 0.3f, 0.5f);

            // Проверка на пересечение с шарами
            foreach (var sphere in spheres)
            {
                if (IntersectSphere(rayOrigin, rayDir, sphere, out float t))
                {
                    Vector3 hitPoint = rayOrigin + t * rayDir;
                    Vector3 normal = Vector3.Normalize(hitPoint - sphere.Center);

                    Vector3 lightDir = Vector3.Normalize(lightPosition - hitPoint);
                    float diffuseIntensity = Math.Max(Vector3.Dot(normal, lightDir), 0.0f);

                    // Если объект в тени, уменьшить интенсивность
                    if (IsInShadow(hitPoint, lightPosition))
                    {
                        diffuseIntensity *= 0.3f;
                    }

                    Vector3 diffuseColor = sphere.Material.Color * diffuseIntensity;

                    // Отражение
                    Vector3 reflectionDir = Reflect(rayDir, normal);
                    Vector3 reflectionColor = RayTrace(hitPoint + normal * 0.001f, reflectionDir, depth + 1);

                    // Преломление (закон Снелла)
                    Vector3 refractionColor = Vector3.Zero;
                    if (sphere.Material.RefractionIndex > 1.0f)
                    {
                        refractionColor = Refract(rayDir, normal, sphere.Material.RefractionIndex);
                        refractionColor = RayTrace(hitPoint + normal * 0.001f, refractionColor, depth + 1);
                    }

                    return sphere.Material.Reflectivity * reflectionColor + (1 - sphere.Material.Reflectivity) * refractionColor + diffuseColor;
                }
            }

            // Проверка на пересечение с плоскостью
            if (IntersectPlane(rayOrigin, rayDir, plane, out float tPlane))
            {
                Vector3 hitPoint = rayOrigin + tPlane * rayDir;
                Vector3 normal = plane.Normal;

                Vector3 lightDir = Vector3.Normalize(lightPosition - hitPoint);
                float diffuseIntensity = Math.Max(Vector3.Dot(normal, lightDir), 0.0f);

                // Если объект в тени, уменьшить интенсивность
                if (IsInShadow(hitPoint, lightPosition))
                {
                    diffuseIntensity *= 0.3f;
                }

                return plane.Material.Color * diffuseIntensity;
            }

            return new Vector3(0.06f, 0.3f, 0.5f); 
        }

        // Отражение
        private Vector3 Reflect(Vector3 incident, Vector3 normal)
        {
            return incident - 2 * Vector3.Dot(incident, normal) * normal;
        }

        // Преломление
        private Vector3 Refract(Vector3 incident, Vector3 normal, float refractionIndex)
        {
            float cosi = MathF.Max(-1, MathF.Min(1, Vector3.Dot(incident, normal)));
            float etai = 1, etat = refractionIndex;

            Vector3 n = normal;
            if (cosi < 0)
            {
                cosi = -cosi;
            }
            else
            {
                float temp = etai;
                etai = etat;
                etat = temp;
                n = -normal;
            }

            float eta = etai / etat;
            float k = 1 - eta * eta * (1 - cosi * cosi);
            return k < 0 ? Vector3.Zero : eta * incident + (eta * cosi - MathF.Sqrt(k)) * n;
        }


        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.LoadIdentity();
            GL.Translate(dx, dy, dz);

            // Создаем лучи для каждого пикселя на экране
            for (int x = 0; x < 1200; x++)
            {
                for (int y = 0; y < 1200; y++)
                {
                    float u = (2.0f * x) / 1200 - 1.0f;;
                    float v = 1.0f - (2.0f * y) / 1200;

                    // Направление луча от камеры через пиксель
                    Vector3 rayDir = Vector3.Normalize(new Vector3(u, v, 1.0f));

                    // Отображаем результат трассировки луча
                    Vector3 color = RayTrace(cameraPosition, rayDir, 0);;

                    GL.Color3(color);
                    GL.Begin(PrimitiveType.Points);
                    GL.Vertex2(u, v);
                    GL.End();
                }
            }

            Context.SwapBuffers();
        }
    }
}
