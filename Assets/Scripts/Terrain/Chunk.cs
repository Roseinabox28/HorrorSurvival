using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Chunk {

    public ChunkCoord coord;

    GameObject chunkObject;
	MeshRenderer meshRenderer;
	MeshFilter meshFilter;

	int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3> ();
	List<int> triangles = new List<int> ();
    List<int> transparentTriangles = new List<int>();
    Material[] materials = new Material[2];
	List<Vector2> uvs = new List<Vector2> ();
    List<Color> colors = new List<Color>();
    List<Vector3> normals = new List<Vector3>();

    public Vector3 position;

	

    
    
    
    private bool _isActive;

    ChunkData chunkData;
    

    public Chunk (ChunkCoord _coord) {

        coord = _coord;
    
        
        

        

    }

    public void Init () {

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = World.instance.material;
        materials[1] = World.instance.transparentMaterial;
        meshRenderer.materials = materials;

        chunkObject.transform.SetParent(World.instance.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkWidth, 0f, coord.z * VoxelData.chunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;
        position = chunkObject.transform.position;

        chunkData = World.instance.worldData.RequestChunk(new Vector2Int((int)position.x, (int)position.z), true);

        lock(World.instance.chunkUpdateThreadLock)
            World.instance.chunksToUpdate.Add(this);

        chunkObject.AddComponent<ChunkLoadAnimation>();
        
    }

	

   

	public void UpdateChunk () {

        


        ClearMeshData();

		for (int y = 0; y < VoxelData.chunkHeight; y++) {
			for (int x = 0; x < VoxelData.chunkWidth; x++) {
				for (int z = 0; z < VoxelData.chunkWidth; z++) {

                    if (World.instance.blockTypes[chunkData.map[x,y,z].id].isSolid)
					    UpdateMeshData (new Vector3(x, y, z));

				}
			}
		}

        
        World.instance.chunksToDraw.Enqueue(this);
        


}

    void ClearMeshData () {

        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
        colors.Clear();
        normals.Clear();

    }

    public bool isActive {

        get { return _isActive; }
        set {

            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);

        }

    }

  

    bool IsVoxelInChunk (int x, int y, int z) {

        if (x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1)
            return false;
        else
            return true;

    }

    public void EditVoxel (Vector3 pos, byte newID) {

        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        chunkData.map[xCheck, yCheck, zCheck].id = newID;
        World.instance.worldData.AddToModifiedChunkList(chunkData);

        lock (World.instance.chunkUpdateThreadLock)
        {
            World.instance.chunksToUpdate.Insert(0,this);
            UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
        }

        

        

    }

    void UpdateSurroundingVoxels (int x, int y, int z) {

        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++) {

            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z)) {

                World.instance.chunksToUpdate.Insert(0,World.instance.GetChunkFromVector3(currentVoxel + position));

            }

        }

    }

	VoxelState CheckVoxel (Vector3 pos) {

		int x = Mathf.FloorToInt (pos.x);
		int y = Mathf.FloorToInt (pos.y);
		int z = Mathf.FloorToInt (pos.z);

        if (!IsVoxelInChunk(x, y, z))
            return World.instance.GetVoxelState(pos + position);

		return chunkData.map[x, y, z];

	}

    public VoxelState GetVoxelFromGlobalVector3 (Vector3 pos) {

        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);

        return chunkData.map[xCheck, yCheck, zCheck];

    }

	void UpdateMeshData (Vector3 pos) 
    {

        int x = Mathf.FloorToInt (pos.x);
		int y = Mathf.FloorToInt (pos.y);
		int z = Mathf.FloorToInt (pos.z);

        byte blockID = chunkData.map[x, y, z].id;

        //bool isTransparent = World.instance.blockTypes[blockID].isTransparent;

		for (int p = 0; p < 6; p++) { 
            VoxelState neighbor = CheckVoxel(pos + VoxelData.faceChecks[p]);

			if (neighbor != null && World.instance.blockTypes[neighbor.id].renderNeighborFaces) {

				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 0]]);
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 1]]);
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 2]]);
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 3]]);

                for(int i = 0; i < 4; i++)
                    normals.Add(VoxelData.faceChecks[p]);

                AddTexture(World.instance.blockTypes[blockID].GetTextureID(p));

                float lightLevel;

                int yPos = (int)pos.y + 1;
                bool inShade = false;

                
                

                if (inShade)
                    lightLevel = 0.5f;
                else
                    lightLevel = 0f;

                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));

                if (!World.instance.blockTypes[neighbor.id].renderNeighborFaces) {
				    triangles.Add (vertexIndex);
				    triangles.Add (vertexIndex + 1);
				    triangles.Add (vertexIndex + 2);
				    triangles.Add (vertexIndex + 2);
				    triangles.Add (vertexIndex + 1);
				    triangles.Add (vertexIndex + 3);
                } else {
                    transparentTriangles.Add (vertexIndex);
				    transparentTriangles.Add (vertexIndex + 1);
				    transparentTriangles.Add (vertexIndex + 2);
				    transparentTriangles.Add (vertexIndex + 2);
				    transparentTriangles.Add (vertexIndex + 1);
				    transparentTriangles.Add (vertexIndex + 3);
                }

                vertexIndex += 4;

			}
		}

	}

	public void CreateMesh () {

		Mesh mesh = new Mesh ();
		mesh.vertices = vertices.ToArray ();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        //mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray ();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();

		//mesh.RecalculateNormals ();

		meshFilter.mesh = mesh;

	}

    void AddTexture (int textureID) {

        float y = textureID / VoxelData.textureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.textureAtlasSizeInBlocks);

        x *= VoxelData.normalizedBlockTextureSize;
        y *= VoxelData.normalizedBlockTextureSize;

        y = 1f - y - VoxelData.normalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.normalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.normalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.normalizedBlockTextureSize, y + VoxelData.normalizedBlockTextureSize));


    }

}

public class ChunkCoord
{

    public int x;
    public int z;

    public ChunkCoord () {

        x = 0;
        z = 0;

    }

    public ChunkCoord (int _x, int _z) {

        x = _x;
        z = _z;

    }

    public ChunkCoord (Vector3 pos) {

        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.chunkWidth;
        z = zCheck / VoxelData.chunkWidth;

    }

    public bool Equals (ChunkCoord other) {

        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;

    }

}

[System.Serializable]
public class VoxelState
{
    public byte id;

    public VoxelState () {

        id = 0;
        

    }

    public VoxelState (byte _id) {

        id = _id;

    }
}
