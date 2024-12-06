using System.Collections.Generic;
using UnityEngine;

public static class CatmullClarkSubdivision
{
    public static Mesh Subdivide(Mesh inputMesh)
    {
        // Extract mesh data
        Vector3[] vertices = inputMesh.vertices;
        int[] triangles = inputMesh.triangles;

        // Create edge and face structures
        Dictionary<Edge, EdgeData> edgeData = new Dictionary<Edge, EdgeData>();
        List<Face> faces = new List<Face>();
        
        // Process faces and edges
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int v0 = triangles[i];
            int v1 = triangles[i + 1];
            int v2 = triangles[i + 2];

            Face face = new Face(v0, v1, v2);
            faces.Add(face);

            // Add edges to edge data
            AddEdge(edgeData, new Edge(v0, v1), face);
            AddEdge(edgeData, new Edge(v1, v2), face);
            AddEdge(edgeData, new Edge(v2, v0), face);
        }

        // Create new vertices for faces
        Vector3[] facePoints = new Vector3[faces.Count];
        for (int i = 0; i < faces.Count; i++)
        {
            Face face = faces[i];
            facePoints[i] = (vertices[face.v0] + vertices[face.v1] + vertices[face.v2]) / 3;
        }

        // Create new vertices for edges
        Dictionary<Edge, Vector3> edgePoints = new Dictionary<Edge, Vector3>();
        foreach (var kvp in edgeData)
        {
            Edge edge = kvp.Key;
            EdgeData data = kvp.Value;

            Vector3 edgePoint = (vertices[edge.v0] + vertices[edge.v1]) / 2;
            if (data.Faces.Count == 2)
            {
                edgePoint = (edgePoint + facePoints[faces.IndexOf(data.Faces[0])] + facePoints[faces.IndexOf(data.Faces[1])]) / 4;
            }
            edgePoints[edge] = edgePoint;
        }

        // Create new vertices for original vertices
        Vector3[] newVertices = new Vector3[vertices.Length];
        int[] vertexFaceCount = new int[vertices.Length];
        Vector3[] vertexFaceSum = new Vector3[vertices.Length];
        Vector3[] vertexEdgeSum = new Vector3[vertices.Length];

        foreach (var kvp in edgeData)
        {
            Edge edge = kvp.Key;
            EdgeData data = kvp.Value;

            vertexEdgeSum[edge.v0] += edgePoints[edge];
            vertexEdgeSum[edge.v1] += edgePoints[edge];

            foreach (Face face in data.Faces)
            {
                vertexFaceSum[edge.v0] += facePoints[faces.IndexOf(face)];
                vertexFaceSum[edge.v1] += facePoints[faces.IndexOf(face)];
                vertexFaceCount[edge.v0]++;
                vertexFaceCount[edge.v1]++;
            }
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 avgFace = vertexFaceSum[i] / vertexFaceCount[i];
            Vector3 avgEdge = vertexEdgeSum[i] / vertexFaceCount[i];
            newVertices[i] = (avgFace + 2 * avgEdge + vertices[i]) / 4;
        }

        // Create new triangles
        List<Vector3> finalVertices = new List<Vector3>(newVertices);
        List<int> finalTriangles = new List<int>();

        foreach (var kvp in edgeData)
        {
            Edge edge = kvp.Key;
            Vector3 edgePoint = edgePoints[edge];

            finalVertices.Add(edgePoint);
        }

        foreach (Face face in faces)
        {
            int fIdx = finalVertices.Count;
            finalVertices.Add(facePoints[faces.IndexOf(face)]);

            int v0Idx = face.v0;
            int v1Idx = face.v1;
            int v2Idx = face.v2;

            Edge e0 = new Edge(v0Idx, v1Idx);
            Edge e1 = new Edge(v1Idx, v2Idx);
            Edge e2 = new Edge(v2Idx, v0Idx);

            int e0Idx = finalVertices.Count - 3;
            int e1Idx = finalVertices.Count - 2;
            int e2Idx = finalVertices.Count - 1;

            finalTriangles.Add(v0Idx);
            finalTriangles.Add(e0Idx);
            finalTriangles.Add(fIdx);

            finalTriangles.Add(e0Idx);
            finalTriangles.Add(v1Idx);
            finalTriangles.Add(fIdx);

            finalTriangles.Add(v1Idx);
            finalTriangles.Add(e1Idx);
            finalTriangles.Add(fIdx);

            finalTriangles.Add(e1Idx);
            finalTriangles.Add(v2Idx);
            finalTriangles.Add(fIdx);

            finalTriangles.Add(v2Idx);
            finalTriangles.Add(e2Idx);
            finalTriangles.Add(fIdx);

            finalTriangles.Add(e2Idx);
            finalTriangles.Add(v0Idx);
            finalTriangles.Add(fIdx);
        }

        // Create the new mesh
        Mesh newMesh = new Mesh
        {
            vertices = finalVertices.ToArray(),
            triangles = finalTriangles.ToArray()
        };
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
        return newMesh;
    }

    private static void AddEdge(Dictionary<Edge, EdgeData> edgeData, Edge edge, Face face)
    {
        if (!edgeData.ContainsKey(edge))
        {
            edgeData[edge] = new EdgeData();
        }
        edgeData[edge].Faces.Add(face);
    }

    private class Face
    {
        public int v0, v1, v2;

        public Face(int v0, int v1, int v2)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
        }
    }

    private struct Edge
    {
        public int v0, v1;

        public Edge(int v0, int v1)
        {
            if (v0 < v1)
            {
                this.v0 = v0;
                this.v1 = v1;
            }
            else
            {
                this.v0 = v1;
                this.v1 = v0;
            }
        }
    }

    private class EdgeData
    {
        public List<Face> Faces = new List<Face>();
    }
}
