using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Chunk
{
    public GameObject chunkObject;
}
[Serializable]
public class ChunkRow
{
    public List<Chunk> chunks = new List<Chunk>();
}
public class ChunkManager : MonoBehaviour
{
    public int ChunkSize = 64;
    [SerializeField]
    public List<ChunkRow> chunkRows = new List<ChunkRow>();
    public GameObject grid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        CreateChunks();
    }


    [ContextMenu("Gerar Mapa de Chunks")]
    void CreateChunks()
    {
        ClearChunks();
        for (int i = 0; i < chunkRows.Count; i++)
        {
            for (int j = 0; j < chunkRows[i].chunks.Count; j++)
            {
                Instantiate(chunkRows[i].chunks[j].chunkObject, new Vector3(i * ChunkSize, j * ChunkSize, 0), Quaternion.identity, grid.transform);
            }
        }
    }
    void ClearChunks()
    {
        for (int i = grid.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(grid.transform.GetChild(i).gameObject);
        }
    }
}
