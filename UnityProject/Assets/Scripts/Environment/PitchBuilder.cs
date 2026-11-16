using UnityEngine;

namespace Zakareya.DreamFootball.Environment
{
    /// <summary>
    /// Generates a correctly proportioned placeholder pitch for early gameplay tests.
    /// Replace its simple materials and line geometry with authored stadium assets later.
    /// </summary>
    public sealed class PitchBuilder : MonoBehaviour
    {
        [SerializeField, Min(30f)] private float length = 105f;
        [SerializeField, Min(20f)] private float width = 68f;
        [SerializeField, Min(0.01f)] private float lineWidth = 0.12f;
        [SerializeField] private Material grassMaterial;
        [SerializeField] private Material lineMaterial;

        private void Start()
        {
            Build();
        }

        [ContextMenu("Build Pitch")]
        public void Build()
        {
            Transform old = transform.Find("GeneratedPitch");
            if (old != null)
            {
                if (Application.isPlaying) Destroy(old.gameObject);
                else DestroyImmediate(old.gameObject);
            }

            GameObject rootObject = new GameObject("GeneratedPitch");
            rootObject.transform.SetParent(transform, false);
            Transform root = rootObject.transform;

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "GrassSurface";
            ground.transform.SetParent(root, false);
            ground.transform.localPosition = Vector3.zero;
            ground.transform.localScale = new Vector3(length / 10f, 1f, width / 10f);
            if (grassMaterial != null)
                ground.GetComponent<Renderer>().sharedMaterial = grassMaterial;

            Material markings = lineMaterial != null ? lineMaterial : CreateFallbackLineMaterial();
            float x = length * 0.5f;
            float z = width * 0.5f;

            DrawRectangle(root, "TouchlinesAndGoalLines",
                new Vector3(-x, 0.035f, -z), new Vector3(x, 0.035f, z), markings);
            DrawLine(root, "HalfwayLine", new Vector3(0, 0.035f, -z), new Vector3(0, 0.035f, z), markings);
            DrawCircle(root, "CenterCircle", Vector3.up * 0.035f, 9.15f, markings);

            float penaltyDepth = 16.5f;
            float penaltyHalfWidth = 20.16f;
            DrawRectangle(root, "LeftPenaltyArea",
                new Vector3(-x, 0.04f, -penaltyHalfWidth),
                new Vector3(-x + penaltyDepth, 0.04f, penaltyHalfWidth), markings);
            DrawRectangle(root, "RightPenaltyArea",
                new Vector3(x - penaltyDepth, 0.04f, -penaltyHalfWidth),
                new Vector3(x, 0.04f, penaltyHalfWidth), markings);

            float goalAreaDepth = 5.5f;
            float goalAreaHalfWidth = 9.16f;
            DrawRectangle(root, "LeftGoalArea",
                new Vector3(-x, 0.045f, -goalAreaHalfWidth),
                new Vector3(-x + goalAreaDepth, 0.045f, goalAreaHalfWidth), markings);
            DrawRectangle(root, "RightGoalArea",
                new Vector3(x - goalAreaDepth, 0.045f, -goalAreaHalfWidth),
                new Vector3(x, 0.045f, goalAreaHalfWidth), markings);
        }

        private void DrawRectangle(Transform root, string objectName, Vector3 min, Vector3 max, Material material)
        {
            Vector3[] points =
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, max.y, max.z),
                new Vector3(min.x, max.y, max.z)
            };
            DrawLoop(root, objectName, points, material);
        }

        private void DrawLine(Transform root, string objectName, Vector3 a, Vector3 b, Material material)
        {
            DrawLoop(root, objectName, new[] { a, b }, material, false);
        }

        private void DrawCircle(Transform root, string objectName, Vector3 center, float radius, Material material)
        {
            const int segments = 64;
            Vector3[] points = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                points[i] = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            }
            DrawLoop(root, objectName, points, material);
        }

        private void DrawLoop(Transform root, string objectName, Vector3[] points, Material material, bool loop = true)
        {
            GameObject lineObject = new GameObject(objectName);
            lineObject.transform.SetParent(root, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = points.Length;
            line.SetPositions(points);
            line.loop = loop;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.sharedMaterial = material;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;
        }

        private static Material CreateFallbackLineMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                Debug.LogError("PitchBuilder: no supported shader was found. Assign a line material in the Inspector.");
                return null;
            }

            Material material = new Material(shader)
            {
                color = Color.white
            };
            return material;
        }
    }
}
