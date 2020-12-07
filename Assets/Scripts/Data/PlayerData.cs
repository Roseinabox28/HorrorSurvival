using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [HideInInspector]
    public ItemSlot[] slots = new ItemSlot[World.instance.player.GetComponent<Player>().toolbar.slots.Length + World.instance.player.GetComponent<Player>().inventory.slots.Count];

    public int health;
    public int hunger;
    public float exhaustion;
    public float saturation;

    float x;
    float y;
    float z;

    
    
    public Vector3 position
    {
        get
        {
            return new Vector3(x, y, z);
        }
        set
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
    }

    float qx;
    float qy;
    float qz;
    float qw;

    public Quaternion rotation
    {
        get {return new Quaternion(qx, qy, qz, qw);}
        set
        {
            qx = value.x;
            qy = value.y;
            qz = value.z;
            qw = value.w;
        }
    }

    public PlayerData(int _health, int _hunger, float _exhaustion, float _saturation, Vector3 _position, Quaternion _rotation)
    {
        health = _health;
        hunger = _hunger;
        exhaustion = _exhaustion;
        saturation = _saturation;
        position = _position;
        rotation = _rotation;
    }

    public PlayerData(Player player)
    {
        health = player.playerHealth.GetPlayerHealth();
        hunger = player.hunger;
        exhaustion = player.exhaustion;
        saturation = player.saturation;
        position = player.gameObject.transform.position;
        rotation = player.gameObject.transform.rotation;
    }

    public ItemSlot RequestSlot(int index)
    {

        if(index < slots.Length)
            return slots[index];
        else
            return null;
    }

   

    public void PopulateSlots()
    {
        World.instance.player.GetComponent<Player>().SaveSlots();
    }

}
