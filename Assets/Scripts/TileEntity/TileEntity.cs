using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEntity : MonoBehaviour
{

    private float verticalMomentum = 0f;

    float gravity;
    private World world;
    float entitySize = 0.2f;
    public byte id;

    Vector3 velocity;
    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        gravity = world.gravity;
    }

    
    void FixedUpdate()
    {
        CalculateVelocity();
        transform.Translate(velocity, Space.World);
        transform.RotateAround(transform.position, Vector3.up, 20 * Time.fixedDeltaTime);
        
    }

    void OnTriggerEnter(Collider other)
     {

        Debug.Log("TileEntity Colliding with something");
        Player player = other.gameObject.GetComponent<Player>();
        if(player != null)
        {
            Debug.Log("Player is trying to pick up item");

            ItemSlot slot = player.GetFirstAvailableSlot(id);
            Debug.Log("slot: " + slot);
            if(slot != null)
            {
                ItemStack stack = new ItemStack(id, 1);
                slot.AddStack(stack);
                Destroy(gameObject);
            }
        }
     }

    private void CalculateVelocity()
    {
        //Affect verticalmomentum with gravity
        if(verticalMomentum > gravity)
        {
            
            verticalMomentum += Time.fixedDeltaTime * gravity;
        }
            

        
        
        //apply vertical momentum (falling/jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;


        
        if(velocity.y < 0)
            velocity.y = CheckDownSpeed(velocity.y);
        




    }

    private float CheckDownSpeed(float downSpeed)
    {
        if(
            world.CheckForVoxel(new Vector3(transform.position.x - entitySize,transform.position.y - entitySize/2 + downSpeed, transform.position.z - entitySize)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + entitySize,transform.position.y - entitySize/2 + downSpeed, transform.position.z - entitySize)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + entitySize,transform.position.y - entitySize/2 + downSpeed, transform.position.z + entitySize)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - entitySize,transform.position.y - entitySize/2 + downSpeed, transform.position.z + entitySize)) 
            )
            {
                
                return 0;
            }
        else
        {
            
            return downSpeed;
        }
    }
}
