using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField] private  UIItemSlot cursorSlot = null;
    private ItemSlot cursorItemSlot;

    [SerializeField] private GraphicRaycaster m_Raycaster = null;
    private PointerEventData m_PointerEventData;
    [SerializeField] private EventSystem m_EventSysten = null;

    World world;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update()
    {
        if(!world.inUI)
            return;
        
        cursorSlot.transform.position = Input.mousePosition;

        if(Input.GetMouseButtonDown(0))
        {
            HandleSlotClick(CheckForSlot(), 0);
        }
        if(Input.GetMouseButtonDown(1))
        {
            HandleSlotClick(CheckForSlot(), 1);
        }
    }

    private void HandleSlotClick(UIItemSlot clickedSlot, int mode)
    {
        if(clickedSlot == null)
            return;
        
        if(mode == 0)
        {
            if(!cursorSlot.hasItem && !clickedSlot.hasItem)
            return;

            if(!cursorSlot.hasItem && clickedSlot.hasItem)
            {
                cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
                return;
            }

            if(cursorSlot.hasItem && !clickedSlot.hasItem)
            {
                clickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
                return;
            }


            if(cursorSlot.hasItem && clickedSlot.hasItem)
            {
                if(cursorSlot.itemSlot.stack.id != clickedSlot.itemSlot.stack.id)
                {
                    ItemStack oldCursorSlot = cursorSlot.itemSlot.TakeAll();
                    ItemStack oldSlot = clickedSlot.itemSlot.TakeAll();

                    clickedSlot.itemSlot.InsertStack(oldCursorSlot);
                    cursorSlot.itemSlot.InsertStack(oldSlot);


                }
                if(cursorSlot.itemSlot.stack.id == clickedSlot.itemSlot.stack.id)
                {
                    
                    if(cursorSlot.isStackable && clickedSlot.isStackable)
                    {
                        int stackSize = world.stackSize;
                        int cursorAmt = cursorSlot.itemSlot.stack.amount;
                        int clickedAmt = clickedSlot.itemSlot.stack.amount;
                        int clickedLeft = stackSize - clickedAmt;
                        if(clickedLeft > 0)
                        {
                            if(cursorAmt >= clickedLeft)
                            {
                                cursorAmt -= clickedLeft;
                                clickedAmt += clickedLeft;
                                ItemStack newCursorStack = new ItemStack(cursorSlot.itemSlot.stack.id, cursorAmt);
                                ItemStack newClickedStack = new ItemStack(clickedSlot.itemSlot.stack.id, clickedAmt);
                                cursorSlot.itemSlot.InsertStack(newCursorStack);
                                clickedSlot.itemSlot.InsertStack(newClickedStack);
                            }
                            else
                            {
                                clickedAmt += cursorAmt;
                                cursorSlot.itemSlot.TakeAll();
                                ItemStack newClickedStack = new ItemStack(clickedSlot.itemSlot.stack.id, clickedAmt);
                                clickedSlot.itemSlot.InsertStack(newClickedStack);
                            }
                        }
                    }
                }
            }
        }
        if(mode == 1)
        {
            if(!cursorSlot.hasItem && !clickedSlot.hasItem)
            return;

            if(!cursorSlot.hasItem && clickedSlot.hasItem)
            {
                int clickedAmt = clickedSlot.itemSlot.stack.amount;
                int halfAmt = clickedAmt/2;
                Debug.Log("Half: " + halfAmt);
                clickedAmt -= halfAmt;
                Debug.Log("left: " + clickedAmt);
                ItemStack newCursorStack = new ItemStack(clickedSlot.itemSlot.stack.id, halfAmt);
                ItemStack newClickedStack = new ItemStack(clickedSlot.itemSlot.stack.id, clickedAmt);
                clickedSlot.itemSlot.InsertStack(newClickedStack);
                cursorSlot.itemSlot.InsertStack(newCursorStack);


                return;
            }

            if(cursorSlot.hasItem && !clickedSlot.hasItem)
            {
                cursorItemSlot.Take(1);
                ItemStack newStack = new ItemStack(cursorItemSlot.stack.id, 1);
                clickedSlot.itemSlot.InsertStack(newStack);
                return;
            }
             if(cursorSlot.hasItem && clickedSlot.hasItem)
            {
                if(cursorSlot.itemSlot.stack.id != clickedSlot.itemSlot.stack.id)
                {
                   return;

                }
                if(cursorSlot.itemSlot.stack.id == clickedSlot.itemSlot.stack.id)
                {
                    

                    if(cursorSlot.isStackable && clickedSlot.isStackable)
                    {
                        int stackSize = world.stackSize;
                        int cursorAmt = cursorSlot.itemSlot.stack.amount;
                        int clickedAmt = clickedSlot.itemSlot.stack.amount;
                        int clickedLeft = stackSize - clickedAmt;
                        if(clickedLeft > 0)
                        {
                            cursorAmt -= 1;
                            clickedAmt += 1;
                            ItemStack newCursorStack = new ItemStack(cursorSlot.itemSlot.stack.id, cursorAmt);
                            ItemStack newClickedStack = new ItemStack(clickedSlot.itemSlot.stack.id, clickedAmt);
                            cursorSlot.itemSlot.InsertStack(newCursorStack);
                            clickedSlot.itemSlot.InsertStack(newClickedStack);
                        }
                    }
                }
            }
            
        }
        
        
    }

    private UIItemSlot CheckForSlot()
    {
        m_PointerEventData = new PointerEventData(m_EventSysten);
        m_PointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if(result.gameObject.tag == "UIItemSlot")
                return result.gameObject.GetComponent<UIItemSlot>();
        }

        return null;
    }

}
