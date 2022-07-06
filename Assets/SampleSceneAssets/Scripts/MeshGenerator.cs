using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace SampleSceneAssets.Scripts
{
    /// <summary>
    /// <c>MeshGenerator</c> is an abstract class which can be extended to implement the generation
    /// of parametric surface. A concrete Mesh Generator class needs to implement the
    /// <c>GenerateVertex</c>, <c>getUMinMax</c>, and <c>getVMinMax</c> methods
    /// This class handles the generation of points, forming them into triangles,
    /// generating a collider, and generating a mesh to assign to the
    /// given mesh filter. 
    /// </summary>
    public abstract class MeshGenerator : MonoBehaviour
    {
        [SerializeField] [Range(1, 256)] private int mSubDivisions = 33;

        [SerializeField] [Range(1, 256)] private int nSubDivisions = 33;
        /// <summary>
        /// <c>UMinMax</c> scales the coordinates from 0 and 1 to the given scale
        /// determined by the x and y value of the vector 
        /// <example>
        /// generating a simple sphere:
        /// <code>
        /// return new Vector2(0, 2 * Mathf.PI)
        /// </code>
        /// </example>
        /// </summary>
        protected abstract Vector2 UMinMax { get; }
        /// <summary>
        /// <c>VMinMax</c> scales the coordinates from 0 and 1 to the given scale
        /// determined by the x and y value of the vector 
        /// <example>
        /// generating a simple sphere:
        /// <code>
        /// return new Vector2(0, Mathf.PI)
        /// </code>
        /// </example>
        /// </summary>
        protected abstract Vector2 VMinMax { get; }
        /// <summary>
        /// <c>GenerateVertex</c> generates a vertex from a given u and v coordinate 
        /// <example>
        /// generating a simple sphere:
        /// <code>
        /// float x = radius * Mathf.Cos(u) * Mathf.Sin(v);
        /// float y = radius * Mathf.Sin(u) * Mathf.Sin(v);
        /// float z = radius * Mathf.Cos(v);
        /// return  new Vector3(x, y, z);
        /// </code>
        /// </example>
        /// </summary>
        protected abstract Vector3 GenerateVertex(float u, float v);
        // Start is called before the first frame update
        void Start()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null)
            {
                Debug.LogError("did not find mesh filter assigned to this object cant generate mesh");
                return;
            }

            Mesh mesh = GenerateMesh();
            mf.mesh = mesh;
            AssignCollider(mesh);
        }
        /// <summary>
        /// Method <c>GenerateMesh</c> is used to iterate over the amount of subdivisions selected in the editor
        /// using this method coordinates in the uv system are generated by iterating which are all in the range
        /// of 0 to 1 and the implemented method <c>UMinMax</c> returns the a scaled version of these values
        /// which matches the needed surface to plug into the <c>GenerateVertex</c> method to generate vertices
        /// afterwards unity functions are used to generate normals and tangents.
        /// </summary>
        protected Mesh GenerateMesh()
        {
            Mesh mesh = new Mesh();
            Vector2Int subdivisions = new Vector2Int(mSubDivisions, nSubDivisions);
            Vector2Int vertexSize = subdivisions + new Vector2Int(1, 1);
            Vector3[] vertices = new Vector3[vertexSize.x * vertexSize.y];
            Vector2[] uvs = new Vector2[vertices.Length];
            float uDelta = UMinMax.y - UMinMax.x;
            float vDelta = VMinMax.y - VMinMax.x;
            for (int y = 0; y < vertexSize.y; y++)
            {
                float v = (1f / subdivisions.y) * y;
                for (int x = 0; x < vertexSize.x; x++)
                {
                    float u = (1f / subdivisions.x) * x;
                    Vector2 scaledUV = new Vector2(u * uDelta + UMinMax.x, v * vDelta + VMinMax.x);
                    Vector3 vertex = GenerateVertex(scaledUV.x, scaledUV.y);
                    Vector2 uv = new Vector2(u, v);
                    vertices[x + vertexSize.x * y] = vertex;
                    uvs[x + vertexSize.x * y] = uv;
                }
            }

            int[] triangles = GenerateTriangles(subdivisions, vertexSize);

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
        /// <summary>
        /// Method <c>GenerateTriangles</c> is used in <c>GenerateMesh</c> to generate triangles from the vertices.
        /// unity and most other graphic pipelines hold a separate triangles array for the mesh which
        /// holds indices to the point array where 3 subsequent vertex indices form a triangle to eliminate duplicate
        /// points form adjacent triangles. The methods generates the triangles in counter clockwise winding order
        /// from the given input array, because the order of vertices is always the same
        /// </summary>
        private static int[] GenerateTriangles(Vector2Int subdivisions, Vector2Int vertexSize)
        {
            int[] triangles = new int[3 * 2 * subdivisions.x * subdivisions.y];
            for (int i = 0; i < subdivisions.x * subdivisions.y; i++)
            {
                int triangleIndex = (i % subdivisions.x) + (i / subdivisions.x) * vertexSize.x;
                int indexer = i * 6;

                triangles[indexer] = triangleIndex;
                triangles[indexer + 1] = triangleIndex + subdivisions.x + 1;
                triangles[indexer + 2] = triangleIndex + 1;

                triangles[indexer + 3] = triangleIndex + 1;
                triangles[indexer + 4] = triangleIndex + subdivisions.x + 1;
                triangles[indexer + 5] = triangleIndex + subdivisions.x + 2;
            }

            return triangles;
        }
        /// <summary>
        /// Method <c>AssignCollider</c> assigns the mesh to a mesh collider if the GameObject has one to
        /// ensure correct collision with a generated surface
        /// </summary>
        private void AssignCollider(Mesh mesh)
        {
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null) return;
            meshCollider.sharedMesh = mesh;
        }
    }
}