using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{

    public static readonly int chunkWidth = 16;
    public static readonly int chunkHeight = 128;
    public static readonly int worldSizeInChunks = 100;

    //Lighting Values
    //public static float minLightLevel = 0.15f;
    //public static float maxLightLevel = 0.8f;
    //public static float lightFallOff = 0.08f;

    public static int worldSizeInVoxels
    {
        get {return worldSizeInChunks * chunkWidth;}
    }

    public static readonly int viewDistanceInChunks = 5;

    public static readonly int textureAtlasSizeInBlocks = 4; //Across
    public static float normalizedBlockTextureSize 
    {
        get {return 1f/((float)textureAtlasSizeInBlocks);}
    }

    public static float voxelSize = 1f;




    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0f,0f,0f),
        new Vector3(voxelSize,0f,0f),
        new Vector3(voxelSize,voxelSize,0f),
        new Vector3(0f,voxelSize,0f),
        new Vector3(0f,0f,voxelSize),
        new Vector3(voxelSize,0f,voxelSize),
        new Vector3(voxelSize,voxelSize,voxelSize),
        new Vector3(0f,voxelSize,voxelSize),

    };

    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0f,0f,-voxelSize),
        new Vector3(0f,0f,voxelSize),
        new Vector3(0f,voxelSize,0f),
        new Vector3(0f,-voxelSize,0f),
        new Vector3(-voxelSize,0f,0f),
        new Vector3(voxelSize,0f,0f)
    };

    public static readonly int[,] voxelTris = new int[6,4]
    {
        {0, 3, 1, 2}, //Back Face
        {5, 6, 4, 7}, //Front Face
        {3, 7, 2, 6}, //Top Face
        {1, 5, 0, 4}, //Bottom Face
        {4, 7, 0, 3}, //Left Face
        {1, 2, 5, 6} //Right Face
    };

    public static readonly Vector2[] voxelUVs = new Vector2[4]
    {
        new Vector2(0f,0f),
        new Vector2(0f,voxelSize),
        new Vector2(voxelSize,0f),
        new Vector2(voxelSize,voxelSize)
    };
}
