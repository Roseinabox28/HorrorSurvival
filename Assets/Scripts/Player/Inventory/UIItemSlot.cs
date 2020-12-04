using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour
{
    public bool isLinked = false;
    public ItemSlot itemSlot;
    public Image slotImage;
    public Image slotIcon;
    public Text slotAmount;
    public bool isStackable;

    World world;

    private void Awake()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }

    public bool hasItem
    {
        get
        {
            if(itemSlot == null)
                return false;
            else
                return itemSlot.hasItem;
        }

    }

    public void Link(ItemSlot _itemSlot)
    {
        itemSlot = _itemSlot;
        isLinked = true;
        itemSlot.LinkUISlot(this);
        UpdateSlot();
    }

    public void UnLink()
    {
        itemSlot.UnLinkUISlot();
        itemSlot = null;
        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if(itemSlot != null && itemSlot.hasItem)
        {
            
            if(itemSlot.stack.id < world.blockTypes.Length)
            {
                slotIcon.sprite = world.blockTypes[itemSlot.stack.id].icon;
                isStackable = true;
            }
            
            else if(itemSlot.stack.id >= (world.blockTypes.Length) && itemSlot.stack.id < (world.itemTypes.Length + world.itemIDOffset))
            {
                slotIcon.sprite = world.itemTypes[(itemSlot.stack.id - world.itemIDOffset)].icon;
                if(world.itemTypes[(itemSlot.stack.id - world.itemIDOffset)].isStackable)
                {
                    isStackable = true;
                }
                else
                    isStackable = false;
            }
            
            if(itemSlot.stack.amount == 1)
            {
                slotAmount.text = "";
            }
            else
            {
                slotAmount.text = itemSlot.stack.amount.ToString();
            }

            
            slotIcon.enabled = true;
            slotAmount.enabled = true;
        }
        else
            Clear();
    }

    public void Clear()
    {
        slotIcon.sprite = null;
        slotAmount.text = "";
        slotIcon.enabled = false;
        slotAmount.enabled = false;
    }

    private void OnDestroy()
    {
        if(isLinked)
            itemSlot.UnLinkUISlot();
    }
}

public class ItemSlot
{
    public ItemStack stack = null;
    private UIItemSlot uiItemSlot = null;

    public ItemSlot(UIItemSlot _uiItemSlot)
    {
        stack = null;
        uiItemSlot = _uiItemSlot;
        uiItemSlot.Link(this);


    }

    public ItemSlot(UIItemSlot _uiItemSlot, ItemStack _stack)
    {
        stack = _stack;
        uiItemSlot = _uiItemSlot;
        uiItemSlot.Link(this);
    }

    public void LinkUISlot(UIItemSlot uiSlot)
    {
        uiItemSlot = uiSlot;
    }

    public void UnLinkUISlot()
    {
        uiItemSlot = null;
    }

    public void EmptySlot()
    {
        stack = null;
        if(uiItemSlot != null)
            uiItemSlot.UpdateSlot();
    }

    public int Take(int amt)
    {
        if(amt > stack.amount)
        {
            int _amt = stack.amount;
            EmptySlot();
            return _amt;
        }
        else if(amt < stack.amount)
        {
            stack.amount -= amt;
            uiItemSlot.UpdateSlot();
            return amt;
        }
        else
        {
            EmptySlot();
            return amt;
        }
            
    }

    public ItemStack TakeAll()
    {
        ItemStack handOver = new ItemStack(stack.id, stack.amount);
        EmptySlot();
        return handOver;
    }

    public void InsertStack(ItemStack _stack)
    {
        stack = _stack;
        uiItemSlot.UpdateSlot();
    }
    public void AddStack(ItemStack _stack)
    {
        if(stack == null)
        {
            stack = new ItemStack(_stack.id,_stack.amount);
            
        }
        else
        {
            ItemStack oldStack = stack;
            if(_stack.id == stack.id)
            {
                oldStack.amount += _stack.amount;
            }
            
            stack = oldStack;
        }
        
        uiItemSlot.UpdateSlot();
        
    }

    public bool hasItem
    {
        get
        {
            if(stack != null)
                return true;
            else
                return false;
        }
    }

}
