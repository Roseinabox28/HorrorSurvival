using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
public static class SaveSystem
{
    static bool _isSaving;
    public static bool isSaving
    {
        get
        {
            return _isSaving;
        }
        set
        {
            _isSaving = value;
        }
    }
    //public static bool isLoading = false;
    public static void SaveWorld(WorldData world)
    {
        _isSaving = true;
        string savePath = World.instance.appPath + "/saves/" + world.worldName + "/";

        if(!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        Debug.Log("Saving " + world.worldName + " into " + savePath);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "world.world", FileMode.Create);

        formatter.Serialize(stream, world);
        stream.Close();

        Thread thread = new Thread(() => SaveChunks(world));
        thread.Start();

    }

    public static void SaveChunks(WorldData world)
    {
        List<ChunkData> chunks = new List<ChunkData>(world.modifiedChunks);
        world.modifiedChunks.Clear();

        int count = 0;
        foreach (ChunkData chunk in chunks)
        {
            SaveSystem.SaveChunk(chunk, world.worldName);
            count++;
        }
        Debug.Log(count + " chunks saved to file.");
        _isSaving = false;
    }

    public static void SavePlayer(PlayerData player, string worldName)
    {
        string savePath = World.instance.appPath + "/saves/" + worldName + "/player/";

        if(!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);


        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "player.player", FileMode.Create);

        formatter.Serialize(stream, player);
        stream.Close();

    }

    public static PlayerData LoadPlayer(string worldName)
    {
        string loadPath = World.instance.appPath + "/saves/" + worldName + "/player/player.player";

        if(File.Exists(loadPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath, FileMode.Open);

            PlayerData playerData = formatter.Deserialize(stream) as PlayerData;
            stream.Close();
            return playerData;

        }
        else
        {
            PlayerData player = new PlayerData(World.instance.player.GetComponent<Player>());
            SavePlayer(player, worldName);

            return player;
        }
        


    }

    public static WorldData LoadWorld(string worldName, int seed = 0)
    {
        string loadPath = World.instance.appPath + "/saves/" + worldName + "/";

        if(File.Exists(loadPath + "world.world"))
        {
            Debug.Log("World file " + worldName + " found. Loading now...");
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "world.world", FileMode.Open);

            WorldData world = formatter.Deserialize(stream) as WorldData;
            stream.Close();
            
            return new WorldData(world);

        }
        else
        {
            Debug.Log("World file " + worldName + " not found. Creating new world");

            WorldData world = new WorldData(worldName, seed);
            SaveWorld(world);
            
            return world;
        }
        
    }

    public static void SaveChunk(ChunkData chunk, string worldName)
    {
        string chunkName = chunk.position.x + "-" + chunk.position.y;


        string savePath = World.instance.appPath + "/saves/" + worldName + "/chunks/";

        if(!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);


        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + chunkName + ".chunk", FileMode.Create);

        formatter.Serialize(stream, chunk);
        stream.Close();

    }

    public static ChunkData LoadChunk(string worldName, Vector2Int position)
    {
        string chunkName = position.x + "-" + position.y;

        string loadPath = World.instance.appPath + "/saves/" + worldName + "/chunks/" + chunkName + ".chunk";

        if(File.Exists(loadPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath, FileMode.Open);

            ChunkData chunkData = formatter.Deserialize(stream) as ChunkData;
            stream.Close();
            return chunkData;

        }
        
        return null;
    }
}
