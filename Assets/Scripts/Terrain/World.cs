using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{

    [Header("World Generation Values")]
    public int seed;
    public BiomeAttributes biome;


    [Header("Performance")]
    public bool enableThreading;
    
    [Range(0.95f,0f)]
    public float globalLightLevel;
    public Color day;
    public Color night;

    public Transform player;
    public bool lockCursor = true;
    public Vector3 spawnPosition;
    public float gravity = -9.81f;
    public Material material;
    public Material transparentMaterial;

    public GameObject debugScreen; 
    public GameObject inventoryWindow;
    public GameObject cursorSlot;

    public int stackSize = 100;

    public BlockType[] blockTypes;

    public ItemType[] itemTypes;

    

    public int itemIDOffset = 0;

    Chunk[,] chunks = new Chunk[VoxelData.worldSizeInChunks,VoxelData.worldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public ChunkCoord playerChunkCoord; 
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();
   
    bool applyingModifications = false;

    
    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    private bool _inUI = false;


     private void Start() {

        Random.InitState(seed);

        spawnPosition = new Vector3((VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f, VoxelData.chunkHeight - 50f, (VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);

        if(lockCursor)
            Cursor.lockState = CursorLockMode.Locked;

    }


    private void OnValidate()
    {
        itemIDOffset = blockTypes.Length;
        
    }

    private void Update() {

        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        Shader.SetGlobalFloat("localLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(day, night, globalLightLevel);

        // Only update the chunks if the player has moved from the chunk they were previously on.
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (!applyingModifications)
            ApplyModifications();

        if (chunksToCreate.Count > 0)
            CreateChunk();

        if (chunksToUpdate.Count > 0)
            UpdateChunks();

        if (chunksToDraw.Count > 0)
            lock (chunksToDraw) {

                if (chunksToDraw.Peek().isEditable)
                    chunksToDraw.Dequeue().CreateMesh();

            }


        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
        



    }

    void GenerateWorld () {

        for (int x = (VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks; x < (VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; x++) {
            for (int z = (VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks; z < (VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; z++) {

                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));

            }
        }

        player.position = spawnPosition;

    }

    void CreateChunk () {

        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        activeChunks.Add(c);
        chunks[c.x, c.z].Init();

    }

    void UpdateChunks () {

        bool updated = false;
        int index = 0;

        while (!updated && index < chunksToUpdate.Count - 1) {

            if (chunksToUpdate[index].isEditable) {
                chunksToUpdate[index].UpdateChunk();
                chunksToUpdate.RemoveAt(index);
                updated = true;
            } else
                index++;

        }

    }

    void ApplyModifications () {

        applyingModifications = true;

        while (modifications.Count > 0) {

            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0) {

                VoxelMod v = queue.Dequeue();

                ChunkCoord c = GetChunkCoordFromVector3(v.position);

                if (chunks[c.x, c.z] == null) {
                    chunks[c.x, c.z] = new Chunk(c, this, true);
                    activeChunks.Add(c);
                }

                chunks[c.x, c.z].modifications.Enqueue(v);

                if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
                    chunksToUpdate.Add(chunks[c.x, c.z]);

            }
        }

        applyingModifications = false;

    }

    ChunkCoord GetChunkCoordFromVector3 (Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);
        return new ChunkCoord(x,z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);
        return chunks[x,z];
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for(int x = coord.x - VoxelData.viewDistanceInChunks; x < coord.x + VoxelData.viewDistanceInChunks; x++ )
        {
            for(int z = coord.z - VoxelData.viewDistanceInChunks; z < coord.z + VoxelData.viewDistanceInChunks; z++ )
            {
                if(IsChunkInWorld(new ChunkCoord(x,z)))
                {
                    if(chunks[x, z] == null)
                    {
                        chunks[x,z] = new Chunk(new ChunkCoord(x,z), this, false);
                        chunksToCreate.Add(new ChunkCoord (x,z));
                    }
                        
                    else if(!chunks[x,z].isActive)
                    {
                        chunks[x,z].isActive = true;
                        
                    }
                    activeChunks.Add(new ChunkCoord(x,z));
                        
                    
                }

                for(int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if(previouslyActiveChunks[i].Equals(new ChunkCoord(x,z)))
                        previouslyActiveChunks.RemoveAt(i);
                }
            }
        }

        foreach (ChunkCoord c in previouslyActiveChunks)
        {
            chunks[c.x,c.z].isActive = false;
        }

    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if(!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.chunkHeight)
            return false;
        
        if(chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid;

        return blockTypes[GetVoxel(pos)].isSolid;


    }

    public bool CheckIfVoxelTransparent(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if(!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.chunkHeight)
            return false;
        
        if(chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isTransparent
;

        return blockTypes[GetVoxel(pos)].isTransparent
;


    }

    public bool inUI
    {
        get {return _inUI;}

        set 
        {
            _inUI = value;
            if(_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                inventoryWindow.SetActive(true);
                cursorSlot.SetActive(true);
            }
                
            else if(lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                inventoryWindow.SetActive(false);
                cursorSlot.SetActive(false);
                
            }
            else
            {
                inventoryWindow.SetActive(false);
                cursorSlot.SetActive(false);
            }
               

        }
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        /*IMMUTABLE PASS*/
        //If outside world, return air
        if(!IsVoxelInWorld(pos))
            return 0;
        //if at bottom of world return BaseStone
        if(yPos == 0)
            return 1;

        /*BASIC TERRAIN PASS*/

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x,pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0;

        if(yPos == terrainHeight)
            voxelValue = 3;
        else if(yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = 2;
        else if(yPos > terrainHeight)
            return 0;
        else
            voxelValue = 5;

        /*SECOND PASS*/

        if(voxelValue == 2)
        {
            foreach(Lode lode in biome.lodes)
            {
                if(yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if(Noise.Get3DPerlin(pos,lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
                }
            }
        }

        /*TREE PASS*/

        if(yPos == terrainHeight) 
        {
            if(Noise.Get2DPerlin(new Vector2(pos.x,pos.z),0,biome.treeZoneScale) > biome.treeZoneThreshold)
            {
                if(Noise.Get2DPerlin(new Vector2(pos.x,pos.z),0,biome.treePlacementScale) > biome.treePlacementThreshold)
                {
                    voxelValue = 2;
                    modifications.Enqueue(Structure.MakeTree(pos, biome.minTreeHeight, biome.maxTreeHeight));
                }
                    

            }
        }
        
        return voxelValue;

    }

  

    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < (VoxelData.worldSizeInChunks - 1)  && coord.z > 0 && coord.z < VoxelData.worldSizeInChunks - 1)
            return true;
        else
            return false;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if(pos.x >= 0 && pos.x < (VoxelData.worldSizeInVoxels )&& pos.y >= 0 && pos.y < (VoxelData.chunkHeight )&& pos.z >= 0 && pos.z < (VoxelData.worldSizeInVoxels))
            return true;
        else 
            return false;
    }

}

[System.Serializable]
public class BlockType
{
    public enum ToolType{None,Axe,Shovel,Pick_axe, Food};
    public string blockName;
    public bool isSolid;
    public bool isTransparent;
    public float transparency;
    public Sprite icon;
    public ToolType designatedTool;
    public int minLevel;
    public float hardness;
    public bool isToolRequired;
    public GameObject itemEntity;
    

    [Header ("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;




    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;

            default:
                Debug.Log("Error in GetTextureID, Invalid face index");
                return 0;
        }
    }

}

[System.Serializable]
public class ItemType
{
    public enum ToolType{None,Axe,Shovel,Pick_axe, Food};

    public string itemName;
    public Sprite icon;
    public ToolType toolType;
    public int level;
    public bool isStackable;
    public GameObject itemEntity;
    //all items id's start after the last block id
}

public class VoxelMod
{
    public Vector3 position;
    public byte id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }

}
