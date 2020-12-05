using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    [Header("Stats")]
    public int health = 30;
    public int maxHealth = 30;
   
    public int maxHunger = 30;
    public int hunger = 30;
    public float exhaustion = 0f;
    public float saturation = 5f;

    public PlayerHealth playerHealth;

    [Header("Physics")]
    public bool isGrounded;
    public bool isSprinting;

    public CharacterController controller;



    public float speed = 5f;
    public float sprintSpeed = 10f;

    

    
    public float jumpForce = 5f;

    public float playerWidth;
    public float playerHeight;

    private World world;
    private Transform cam;
    private float verticalMomentum = 0f;
    private bool jumpRequest;
    private float horizontal;
    private float vertical;
    private Vector3 velocity;
    private Vector3 move;

    float gravity;


    [Header("Block Placement")]
    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 8f;
    public bool isEntityInWay = false;
   
    public Toolbar toolbar;
    public Inventory inventory;

    float isSprintingOffset = 0f;
    private bool _isInventoryButtonInUse = false;
    private MouseLook mouselook;
    
    void Start()
    {
        hunger = maxHunger;
        playerWidth = controller.radius;
        playerHeight = controller.height;
        world = GameObject.Find("World").GetComponent<World>();
        cam = GameObject.Find("Camera").transform;
        mouselook = cam.GetComponent<MouseLook>();
        world.inUI = false;
        mouselook.inUI = world.inUI;
        playerHealth = GetComponent<PlayerHealth>();
        gravity = world.gravity;

        
        
    }

    private void FixedUpdate()
    {
        float timer = 0f;
        //fixedtimestep = .02 
        if(!world.inUI)
        {
            CalculateVelocity();
            if(jumpRequest)
            {
                Jump();
                if(isSprinting)
                    exhaustion += 0.3f;
                else
                    exhaustion += 0.075f;
            }
            if(isSprinting)
            {
               HungerCalculations(); 
            }
        
            if(exhaustion >= 4f)
            {
                exhaustion = 0f;
                if(saturation > 0f)
                    saturation -= 1f;
                else
                {
                    if(hunger > 0)
                        hunger -= 1;
                }
                    

            }
            if(hunger == 30 && saturation > 0f )
            {
                if( ((timer / 0.5f) % 1) == 0)
                    if(playerHealth.GetPlayerHealth() != maxHealth)
                        playerHealth.DoHeal(1);
            }
            else if(hunger >= 27)
            {
                if(((timer / 4) % 1) == 0)
                    if(playerHealth.GetPlayerHealth() != maxHealth)
                        playerHealth.DoHeal(1);
            }
            if(hunger >= 9)
            {
                isSprinting = false;
            }
            if(hunger == 0)
            {
                if(((timer / 4) % 1) == 0)
                    playerHealth.DoDamage(1);
            }

            
            
            transform.Translate(velocity, Space.World);
            if(timer > 4)
                timer -= timer;
            timer += Time.fixedDeltaTime;
            
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetAxis("Inventory") != 0)
        {
            if(!_isInventoryButtonInUse)
            {
                world.inUI = !world.inUI;
                mouselook.inUI = world.inUI;
                _isInventoryButtonInUse = true;
            }
        }
        if(Input.GetAxis("Inventory") == 0)
        {
            _isInventoryButtonInUse = false;
        }
        

        if(!world.inUI)
        {
            GetPlayerInputs();
            HungerCalculations();
            PlaceCursorBlocks();
        }
        


        
    }

    public ItemSlot GetFirstAvailableSlot(byte _id)
    {
        if(_id >= world.blockTypes.Length)
        {
            //is item
            _id -= (byte)world.itemIDOffset;
            foreach(UIItemSlot slot in toolbar.slots)
            {
                ItemSlot itemSlot = slot.itemSlot;
                if(world.itemTypes[_id].isStackable && itemSlot.stack.id == _id && itemSlot.stack.amount < world.stackSize)
                {
                    return itemSlot;
                }
                else if(!world.itemTypes[_id].isStackable)
                {
                    if(!itemSlot.hasItem)
                    {
                        return itemSlot;
                    }
                }
                
            }
            foreach(UIItemSlot slot in toolbar.slots)
            {
                //if we get here all matching slots are full so check if any are empty
                ItemSlot itemSlot = slot.itemSlot;
                if(!itemSlot.hasItem)
                {
                    return itemSlot;
                }

            }
            foreach(ItemSlot itemSlot in inventory.slots)
            {
                if(world.itemTypes[_id].isStackable && itemSlot.stack.id == _id && itemSlot.stack.amount < world.stackSize)
                {
                    return itemSlot;
                }
                else if(!world.itemTypes[_id].isStackable)
                {
                    if(!itemSlot.hasItem)
                    {
                        return itemSlot;
                    }
                }
            }
            foreach(ItemSlot itemSlot in inventory.slots)
            {
                //if we get here all matching slots are full so check if any are empty
                
                if(!itemSlot.hasItem)
                {
                    return itemSlot;
                }

            }
            //if we get here the entire inventory is full so return null
            return null;
        }
        else
        {
            foreach(UIItemSlot slot in toolbar.slots)
            {
                ItemSlot itemSlot = slot.itemSlot;

                if(itemSlot.hasItem && itemSlot.stack.id == _id  && itemSlot.stack.amount < world.stackSize)
                {
                    return itemSlot;
                }
            }
            foreach(UIItemSlot slot in toolbar.slots)
            {
                //if we get here all matching slots are full so check if any are empty
                ItemSlot itemSlot = slot.itemSlot;
                if(!itemSlot.hasItem)
                {
                    return itemSlot;
                }

            }
            foreach (ItemSlot itemSlot in inventory.slots)
            {
                if(itemSlot.hasItem && itemSlot.stack.id == _id  && itemSlot.stack.amount < world.stackSize)
                {
                    return itemSlot;
                }
            }
            foreach(ItemSlot itemSlot in inventory.slots)
            {
                //if we get here all matching slots are full so check if any are empty
                
                if(!itemSlot.hasItem)
                {
                    return itemSlot;
                }

            }
            return null;
        }
    }

    void HungerCalculations()
    {
        exhaustion+= isSprintingOffset * Mathf.Abs(move.x) * Mathf.Abs(move.z) * 0.15f * Time.deltaTime;
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void CalculateVelocity()
    {
        //Affect verticalmomentum with gravity
        if(verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        move = transform.right * horizontal + transform.forward * vertical;
        //sprinting
        if(isSprinting)
        {
            isSprintingOffset = 1f;
            velocity = move * sprintSpeed * Time.fixedDeltaTime;
        }
            
        else
        {
            isSprintingOffset = 0f;
            velocity = move * speed * Time.fixedDeltaTime;
        }
            
        
        //apply vertical momentum (falling/jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;


        if((velocity.z > 0 && front) || (velocity.z < 0 && back))
        {
            velocity.z = 0;
            isSprintingOffset = 0f;
        }
        if((velocity.x > 0 && right) || (velocity.x < 0 && left))
        {
            velocity.x = 0;
            isSprintingOffset = 0f;
        }
        if(velocity.y < 0)
        {
            velocity.y = CheckDownSpeed(velocity.y);
        }
        else if(velocity.y > 0)
        {
            velocity.y = CheckUpSpeed(velocity.y);
        }
            




    }

    private void GetPlayerInputs()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if(Input.GetAxis("Sprint") != 0)
        {
            
            isSprinting = true;
            
            
            
        }
        else if (Input.GetAxis("Sprint") == 0)
        {
            isSprinting = false;
        }
        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequest = true;
        }

        

        if(highlightBlock.gameObject.activeSelf)
        {
            //destroy
            if(Input.GetMouseButtonDown(0))
            {
               float timer = 0f;
               Vector3 itemDropPosition = new Vector3(highlightBlock.position.x + 0.5f, highlightBlock.position.y + 0.5f, highlightBlock.position.z + 0.5f);

                if(toolbar.slots[toolbar.slotIndex].hasItem)
                {
                    if(toolbar.slots[toolbar.slotIndex].itemSlot.stack.id >= world.blockTypes.Length)
                    {
                        Debug.Log("Player is using item");
                        Debug.Log("Item type: " + world.itemTypes[(toolbar.slots[toolbar.slotIndex].itemSlot.stack.id - world.itemIDOffset)].toolType);
                        Debug.Log("Item Level: " + world.itemTypes[(toolbar.slots[toolbar.slotIndex].itemSlot.stack.id - world.itemIDOffset)].level);


                        Debug.Log("Tile name: " + world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].blockName);
                        Debug.Log("Tile designated Level: " + world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].minLevel);
                        Debug.Log("Tile designated tool: " + world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].designatedTool);
                        Debug.Log("Tile hardness: " + world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].hardness);

                        Debug.Log("Timer: " + timer);

                        

                        if(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].isToolRequired == true)
                        {
                            Debug.Log("Tool is required");
                            if((int)world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].designatedTool == (int)world.itemTypes[(toolbar.slots[toolbar.slotIndex].itemSlot.stack.id - world.itemIDOffset)].toolType)
                            {
                                Debug.Log("Types match");
                                if(world.itemTypes[(toolbar.slots[toolbar.slotIndex].itemSlot.stack.id - world.itemIDOffset)].level >= world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].minLevel)
                                {
                                    Debug.Log("Item will drop");
                                    if(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].itemEntity != null)
                                    {
                                        GameObject itemDrop = Instantiate(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].itemEntity, itemDropPosition, highlightBlock.rotation);
                                    }
                                    
                                }
                                else
                                {
                                    Debug.Log("Item will not drop");
                                }
                            }
                            else
                            {
                                Debug.Log("Types do not match");
                                Debug.Log("item will not drop");
                            }
                        }
                        else
                        {
                            Debug.Log("Tool is not required");
                            if((int)world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].designatedTool == (int)world.itemTypes[(toolbar.slots[toolbar.slotIndex].itemSlot.stack.id - world.itemIDOffset)].toolType)
                            {
                                Debug.Log("Types match");
                                Debug.Log("item will drop faster");
                                if(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].itemEntity != null)
                                {
                                    GameObject itemDrop = Instantiate(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].itemEntity, itemDropPosition, highlightBlock.rotation);
                                }
                            }
                            else
                            {
                                Debug.Log("Types do not match");
                                Debug.Log("item will not drop faster");
                                if(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].itemEntity != null)
                                {
                                    GameObject itemDrop = Instantiate(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].itemEntity, itemDropPosition, highlightBlock.rotation);
                                }
                            }
                        }

                        
                        
                        
                    }
                    else
                    {
                        Debug.Log("player is using block");
                        if(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].isToolRequired == true)
                        {
                            Debug.Log("Tool is required");
                            Debug.Log("Item will not drop");
                        }
                        else
                        {
                            Debug.Log("Tool is not required");
                            Debug.Log("Item will drop");
                            if(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].itemEntity != null)
                            {
                                GameObject itemDrop = Instantiate(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].itemEntity, itemDropPosition, highlightBlock.rotation);
                            }
                        }
                    }
                    
                }
                else
                {
                    Debug.Log("Player is using hand");
                    if(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].isToolRequired == true)
                    {
                        Debug.Log("Item will not drop");
                    }
                    else
                    {
                        Debug.Log("Item will drop");
                        if(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].itemEntity != null)
                        {
                            GameObject itemDrop = Instantiate(world.blockTypes[world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position)].itemEntity, itemDropPosition, highlightBlock.rotation);
                        }
                    }
                }

                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);

                timer += Time.deltaTime;
            }
               
            
            //place
            if(Input.GetMouseButtonDown(1))
            {
                if(!isEntityInWay)
                {
                    if(toolbar.slots[toolbar.slotIndex].hasItem)
                    {
                        if(toolbar.slots[toolbar.slotIndex].itemSlot.stack.id >= world.blockTypes.Length)
                        {
                            Debug.Log("Player is using item");
                            Debug.Log("Item type: " + world.itemTypes[(toolbar.slots[toolbar.slotIndex].itemSlot.stack.id - world.itemIDOffset)].toolType);
                            Debug.Log("Item Level: " + world.itemTypes[(toolbar.slots[toolbar.slotIndex].itemSlot.stack.id - world.itemIDOffset)].level);
                        }
                        else
                        {
                            world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, toolbar.slots[toolbar.slotIndex].itemSlot.stack.id);
                            toolbar.slots[toolbar.slotIndex].itemSlot.Take(1);
                        }
                        
                    }
                       
                }
                    
            }
                
            

        }

    }

    private void PlaceCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);

            if(world.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x),Mathf.FloorToInt(pos.y),Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;

            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x),Mathf.FloorToInt(pos.y),Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);

    }

    private float CheckDownSpeed(float downSpeed)
    {
        if(
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth,transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth,transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth,transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth,transform.position.y + downSpeed, transform.position.z + playerWidth)) 
            )
            {
                isGrounded = true;
                return 0;
            }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }
    private float CheckUpSpeed(float upSpeed)
    {
        if(
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth,transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth,transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth,transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth,transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth)) 
            )
            {
                
                return 0;
            }
        else
        {
            
            return upSpeed;
        }
    }

    public bool front
    {
        get
        {
            if(
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
                )
                return true;
            else
                return false;
        }
    }

     public bool back
    {
        get
        {
            if(
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
                )
                return true;
            else
                return false;
        }
    }

     public bool left
    {
        get
        {
            if(
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }
    }

     public bool right
    {
        get
        {
            if(
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }
    }

    
}
