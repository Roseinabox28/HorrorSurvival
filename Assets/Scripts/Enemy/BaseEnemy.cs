using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class BaseEnemy : MonoBehaviour
{
    World world;

    [Header("Stats")]
    [Range(0,500)]
    public int damage = 1;
    public int health = 30;

    public EnemyHealth enemyHealth;
    
    public float speed = 5f;
    public bool isGrounded;

    public float enemyWidth;
    public float enemyHeight;

    private float verticalMomentum = 0f;
    private Vector3 velocity;
    private Vector3 move;
    private float horizontal = 0f;
    private float vertical = 0f;

     float gravity;

    // Start is called before the first frame update
    void Start()
    {
        
        enemyWidth = GetComponent<CapsuleCollider>().radius;
        enemyHeight = GetComponent<CapsuleCollider>().height;
        world = GameObject.Find("World").GetComponent<World>();
        gravity = world.gravity;
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void FixedUpdate()
    {
        
        CalculateVelocity();
        transform.Translate(velocity, Space.World);
    }


     void OnTriggerEnter(Collider other)
     {

        
        PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
        if(playerHealth != null)
        {
            playerHealth.DoDamage(damage);
        }
     }

    private void CalculateVelocity()
    {
        //Affect verticalmomentum with gravity
        if(verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        move = transform.right * horizontal + transform.forward * vertical;
        
        
        velocity = move * speed * Time.fixedDeltaTime;
        
        //apply vertical momentum (falling/jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;


        if((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;
        if(velocity.y < 0)
            velocity.y = CheckDownSpeed(velocity.y);
        else if(velocity.y > 0)
            velocity.y = CheckUpSpeed(velocity.y);




    }

    

     private float CheckDownSpeed(float downSpeed)
    {
        if(
            world.CheckForVoxel(new Vector3(transform.position.x - enemyWidth,transform.position.y + downSpeed, transform.position.z - enemyWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + enemyWidth,transform.position.y + downSpeed, transform.position.z - enemyWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + enemyWidth,transform.position.y + downSpeed, transform.position.z + enemyWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - enemyWidth,transform.position.y + downSpeed, transform.position.z + enemyWidth)) 
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
            world.CheckForVoxel(new Vector3(transform.position.x - enemyWidth,transform.position.y + enemyHeight + upSpeed, transform.position.z - enemyWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + enemyWidth,transform.position.y + enemyHeight + upSpeed, transform.position.z - enemyWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + enemyWidth,transform.position.y + enemyHeight + upSpeed, transform.position.z + enemyWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - enemyWidth,transform.position.y + enemyHeight + upSpeed, transform.position.z + enemyWidth)) 
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
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + enemyWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + enemyWidth))
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
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - enemyWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - enemyWidth))
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
                world.CheckForVoxel(new Vector3(transform.position.x - enemyWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - enemyWidth, transform.position.y + 1f, transform.position.z))
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
                world.CheckForVoxel(new Vector3(transform.position.x + enemyWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + enemyWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }
    }
}
