using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Player player;
    Text text;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    

    // Start is called before the first frame update
    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();
        player = world.player.gameObject.GetComponent<Player>();
        halfWorldSizeInVoxels = VoxelData.worldSizeInVoxels / 2;
        halfWorldSizeInChunks = VoxelData.worldSizeInChunks / 2;
    }

    void Update()
    {
        string debugText = "Games in a Box's Untitled Horror Survival Game";
        debugText += "\n";
        debugText += frameRate + " FPS\n\n";
        debugText += "XYZ: " + (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels) + " / " + (Mathf.FloorToInt(world.player.transform.position.y)) + " / " + (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels) + "\n";
        debugText += "Chunk: " + (world.playerChunkCoord.x - halfWorldSizeInChunks) + " / " + (world.playerChunkCoord.z - halfWorldSizeInChunks);

        debugText += "\n Player Health: " + player.playerHealth.GetPlayerHealth();
        debugText += "\n Player Health Percent: " + (player.playerHealth.GetPlayerHealthPercent() * 100) + "%";
        debugText += "\n Player Hunger: " + player.hunger;
        debugText += "\n Player Hunger Percent: " + (player.hunger / player.maxHunger * 100) + "%";
        debugText += "\n Player Exhaustion: " + player.exhaustion;
        debugText += "\n Player Saturation: " + player.saturation;

        text.text = debugText;

        if(timer > 1f)
        {
            frameRate = (int)(1f/Time.unscaledDeltaTime);
            timer = 0;
        }    
        else
            timer += Time.deltaTime;
    }
}
