using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public class World : MonoBehaviour
{
    public Settings settings;

    [Header("World Generation Values")]
    public BiomeAttributes[] biomes;


    
    
    

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
    public object chunkListThreadLock = new object();

    private static World _instance;
    public static World instance
    {
        get{return _instance;}
    }

    public WorldData worldData;

    public string appPath;

    private void Awake()
    {
        if(_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;

        appPath = Application.persistentDataPath;
    }

    private void Start() 
    {
        Debug.Log("Seed: " + VoxelData.seed);
        
        worldData = SaveSystem.LoadWorld("Prototype");

        string jsonImport = File.ReadAllText(Application.dataPath + "/Data/Settings/settings.cfg");
        settings = JsonUtility.FromJson<Settings>(jsonImport);

        Random.InitState(VoxelData.seed);
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

        if(Input.GetKeyDown(KeyCode.F1))
            SaveSystem.SaveWorld(worldData);
        



    }

    void LoadWorld () {

        for (int x = (VoxelData.worldSizeInChunks / 2) - settings.loadDistance; x < (VoxelData.worldSizeInChunks / 2) + settings.loadDistance; x++) {
            for (int z = (VoxelData.worldSizeInChunks / 2) - settings.loadDistance; z < (VoxelData.worldSizeInChunks / 2) + settings.loadDistance; z++) {

                worldData.LoadChunk(new Vector2Int(x, z));

            }
        }

     

    }

    void GenerateWorld () {

        for (int x = (VoxelData.worldSizeInChunks / 2) - settings.viewDistance; x < (VoxelData.worldSizeInChunks / 2) + settings.viewDistance; x++) {
            for (int z = (VoxelData.worldSizeInChunks / 2) - settings.viewDistance; z < (VoxelData.worldSizeInChunks / 2) + settings.viewDistance; z++) {

                ChunkCoord newChunk = new ChunkCoord(x, z);

                chunks[x, z] = new Chunk(newChunk);
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
        lock(chunkUpdateThreadLock)
        {
          chunksToUpdate[0].UpdateChunk();
            if(!activeChunks.Contains(chunksToUpdate[0].coord))
                activeChunks.Add(chunksToUpdate[0].coord);
            chunksToUpdate.RemoveAt(0);
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

                worldData.SetVoxel(v.position, v.id);

               

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
                        chunks[x,z] = new Chunk(thisChunkCoord);
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
        VoxelState voxel = worldData.GetVoxel(pos);

        if(blockTypes[voxel.id].isSolid)
            return true;
        else
            return false;


    }

    public VoxelState GetVoxelState(Vector3 pos)
    {
        return worldData.GetVoxel(pos);
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

        /*BIOME SELECTION PASS*/

        int solidGroundHeight = 42;
        float sumOfHeights = 0f;
        int count = 0;
        float strongestWeight = 0f;
        int strongestBiomeIndex = 0;
        
        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);

            //Track Weight
            if(weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            //Get the height of the terrain (for the current biome) and multiply it by its weight
            float height = biomes[i].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x,pos.z), 0, biomes[i].terrainScale) * weight;

            //If the height value is greater than 0 add it to the sum of heights

            if(height > 0)
            {
                sumOfHeights += height;
                count++;
            }

            
        }

        //Set biome to the one with the strongest weight
        BiomeAttributes biome = biomes[strongestBiomeIndex];

        //get the average of the heights
        sumOfHeights /= count;

        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);



        

        /*BASIC TERRAIN PASS*/

        
        byte voxelValue = 0;

        if(yPos == terrainHeight)
            voxelValue = biome.surfaceBlock;
        else if(yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = biome.subsurfaceBlock;
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

        if(yPos == terrainHeight && biome.placeMajorFlora) 
        {
            if(Noise.Get2DPerlin(new Vector2(pos.x,pos.z),0,biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                if(Noise.Get2DPerlin(new Vector2(pos.x,pos.z),0,biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {
                    voxelValue = biome.subsurfaceBlock;
                    modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex, pos, biome.minHeight, biome.maxHeight));
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
    public bool renderNeighborFaces;
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
    public string version = "pre-alpha-0.1";

    [Header("Performance")]
    public int loadDistance = 10;
    public int viewDistance = 5;
    public bool enableThreading = true;


    [Header("Controls")]
    [Range(10f, 500f)]
    public float mouseSensitivity = 100f;

    
}
