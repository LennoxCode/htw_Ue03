using UnityEngine;

namespace SampleSceneAssets.Scripts.MaxScript
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshGenerator_Test : MonoBehaviour
    {
    
        Mesh mesh;
        Vector3[] vertices;
        int [] triangles;
        // Start is called before the first frame update
        void Start()
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;

            CreateShape();
            UpdateMesh();
        }

        // Update is called once per frame
        void CreateShape()
        {
            vertices = new Vector3[]
            {
                new Vector3 (0, 0, 0),
                new Vector3 (0, 0, 1),
                new Vector3 (1, 0, 0),
                new Vector3 (1, 0, 1)

            };
            triangles = new int[]
            {
                0, 1, 2,
                1,3,2

            };
        }

        void UpdateMesh()
        {
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
        
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        
        }
    }
}