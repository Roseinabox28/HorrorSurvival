using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject slotPrefab;
    World world;

    public List<ItemSlot> slots = new List<ItemSlot>();


    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();

        for (int i = 0; i < 27; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, transform);


            ItemSlot slot = new ItemSlot(newSlot.GetComponent<UIItemSlot>());
            slots.Add(slot);

            
        }
    }

    
}
