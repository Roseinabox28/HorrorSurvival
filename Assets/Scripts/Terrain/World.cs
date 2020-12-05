using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public class World : MonoBehaviour
{
    public Settings settings;

    [Header("World Generation Values")]
    public BiomeAttributes biome;


    
    
    

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
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();
   
    bool applyingModifications = false;

    
    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    private bool _inUI = false;


    Thread chunkUpdateThread;
    public object chunkUpdateThreadLock = new object();


    private void Start() 
    {

        //string jsonExport = JsonUtility.ToJson(settings);

        //File.WriteAllText(Application.dataPath + "/Data/Settings/settings.cfg", jsonExport);

        //string jsonImport = File.ReadAllText(Application.dataPath + "/Data/Settings/settings.cfg");
        //settings = JsonUtility.FromJson<Settings>(jsonImport);

        Random.InitState(settings.seed);
        if(settings.enableThreading)
        {
            chunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            chunkUpdateThread.Start();
        }
        

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

        
        

        // Only update the chunks if the player has moved from the chunk they were previously on.
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        

        if (chunksToCreate.Count > 0)
            CreateChunk();

      

        if (chunksToDraw.Count > 0)
        {
            if (chunksToDraw.Peek().isEditable)
                chunksToDraw.Dequeue().CreateMesh();
        }
            
        if(!settings.enableThreading)
        {
            if (!applyingModifications)
                ApplyModifications();
            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }


        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
        



    }

    void GenerateWorld () {

        for (int x = (VoxelData.worldSizeInChunks / 2) - settings.viewDistance; x < (VoxelData.worldSizeInChunks / 2) + settings.viewDistance; x++) {
            for (int z = (VoxelData.worldSizeInChunks / 2) - settings.viewDistance; z < (VoxelData.worldSizeInChunks / 2) + settings.viewDistance; z++) {

                ChunkCoord newChunk = new ChunkCoord(x, z);

                chunks[x, z] = new Chunk(newChunk, this);
                chunksToCreate.Add(newChunk);

            }
        }

        player.position = spawnPosition;
        CheckViewDistance();

    }

    void CreateChunk() 
    {

        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        
        chunks[c.x, c.z].Init();

    }

    void UpdateChunks() 
    {

        bool updated = false;
        int index = 0;

        lock(chunkUpdateThreadLock)
        {
            while (!updated && index < chunksToUpdate.Count - 1) 
            {

                if (chunksToUpdate[index].isEditable)
                {
                    chunksToUpdate[index].UpdateChunk();
                    if(!activeChunks.Contains(chunksToUpdate[index].coord))
                        activeChunks.Add(chunksToUpdate[index].coord);
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                } 
                else
                    index++;

            }
        }
        

    }

    void ThreadedUpdate()
    {
        while(true)
        {
            if (!applyingModifications)
                ApplyModifications();
            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }
    }

    private void OnDisable()
    {
        if(settings.enableThreading)
        {
            chunkUpdateThread.Abort();
        }
    }

    void ApplyModifications () {

        applyingModifications = true;

        while (modifications.Count > 0) {

            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0) {

                VoxelMod v = queue.Dequeue();

                ChunkCoord c = GetChunkCoordFromVector3(v.position);

                if (chunks[c.x, c.z] == null) 
                {
                    chunks[c.x, c.z] = new Chunk(c, this);
                    chunksToCreate.Add(c);
                }

                chunks[c.x, c.z].modifications.Enqueue(v);

               

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
        activeChunks.Clear();

        for(int x = coord.x - settings.viewDistance; x < coord.x + settings.viewDistance; x++ )
        {
            for(int z = coord.z - settings.viewDistance; z < coord.z + settings.viewDistance; z++ )
            {
                ChunkCoord thisChunkCoord = new ChunkCoord(x,z);
                if(IsChunkInWorld(thisChunkCoord))
                {
                    if(chunks[x, z] == null)
                    {
                        chunks[x,z] = new Chunk(thisChunkCoord, this);
                        chunksToCreate.Add(thisChunkCoord);
                    }
                        
                    else if(!chunks[x,z].isActive)
                    {
                        chunks[x,z].isActive = true;
                        
                    }
                    activeChunks.Add(thisChunkCoord);
                        
                    
                }

                for(int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if(previouslyActiveChunks[i].Equals(thisChunkCoord))
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
                Cursor.visible = true;
                inventoryWindow.SetActive(true);
                cursorSlot.SetActive(true);
            }
                
            else if(lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                inventoryWindow.SetActive(false);
                cursorSlot.SetActive(false);
                
            }
            else
            {
                Cursor.visible = false;
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

[System.Serializable]
public class Settings
{
    [Header("Game Data")]
    public string version;

    [Header("Performance")]
    public int viewDistance;
    public bool enableThreading;


    [Header("Controls")]
    [Range(10f, 500f)]
    public float mouseSensitivity;

    [Header("World Gen")]
    public int seed;
}
