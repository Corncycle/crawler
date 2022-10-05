using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    const int P_UP = 0; 
    const int P_RIGHT = 1; 
    const int P_DOWN = 2; 
    const int P_LEFT = 3;

    static Vector2[] P_VECTORS = {
        Vector2.up,
        Vector2.right,
        Vector2.down,
        Vector2.left
    };

    const int P_VERTICAL = 0;
    const int P_HORIZONTAL = 1;

    // overrideDirection is the direction the player first held, and should take precedent over future inputs while held
    int overrideDirection = -1;
    int movementAxis = 0;

    Vector2Int moveInput;
    public float speed = 1;
    [SerializeField] float internalSpeedMultiplier = 0.5f;
    
    // centerAlignmentTolerance dictates how close to the middle of a tile we must be to be considered centered within it
    const float centerAlignmentTolerance = 0.04f;

    Rigidbody2D rigidBody;
    DungeonGeneration dungeonGeneration;

    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        dungeonGeneration = FindObjectOfType<DungeonGeneration>();
        transform.position = dungeonGeneration.FindRandomOpenTile() + new Vector2(0.5f, 0.5f);
    }

    void FixedUpdate() {
        HandleMovement();
        /*if (overrideDirection == -1) {
            rigidBody.velocity = Vector2.zero;
        } else {
            Vector2 playerVelocity = P_VECTORS[overrideDirection] * speed * internalSpeedMultiplier;
            rigidBody.velocity = playerVelocity;
        }
        print(overrideDirection);*/    
    }

    void Update()
    {

    }

    // OnMove updates moveInput to one of the 9 directions with integer components
    // OnMove also updates overrideDirection with basic logic to only allow cardinals
    void OnMove(InputValue value) {
        Vector2 rawInput = value.Get<Vector2>();
        moveInput = new Vector2Int(System.Convert.ToInt32(rawInput.x), System.Convert.ToInt32(rawInput.y));
        overrideDirection = ReduceVectorToDirection(moveInput);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        print("hiii");    
    }

    private void OnCollisionEnter2D(Collision2D other) {
        print("blah");
    }

    // reduces an input vector to an integer direction (representing a cardinal) based on prior input
    int ReduceVectorToDirection(Vector2Int input) {
        if (overrideDirection == -1) {
            if (input != new Vector2Int(0, 0)) {
                return CastVectorToDirection(input);
            } else {
                return -1;
            }
        } else {
            // This code forces maintaining the first input from the player
            // if (!VecHasComponent(input, overrideDirection)) {
            //     return CastVectorToDirection(input);
            // }
            if (input == new Vector2Int(0, 0)) {
                return -1;
            }
            int dir = VecHasPerpComponent(input, overrideDirection);
            if (dir != -1) {
                return dir;
            }
            return overrideDirection;
        }
    }

    // Returns true if a vector contains a given direction as a component
    // ie (1, 1) would return true for P_UP
    bool VecHasComponent(Vector2Int v, int dir) {
        if (dir % 2 == 0) {
            return (mod(v.y - 1, 4) == dir);
        } else {
            return (mod(v.x, 4) == dir);
        }
    }

    // Returns -1 if the given vector has no component perpendicular to the given direction
    // Returns an integer representing that direction otherwise
    int VecHasPerpComponent(Vector2Int v, int dir) {
        if (dir % 2 == 0) {
            if (v.x != 0) {
                return mod(v.x, 4);
            }
        } else {
            if (v.y != 0) {
                return mod(v.y - 1, 4);
            }
        }
        return -1;
    }

    int CastVectorToDirection(Vector2Int v) {
        if (v.x != 0) {
            return mod(v.x, 4);
        } else if (v.y != 0) {
            return mod(v.y - 1, 4);
        } else {
            return -1;
        }
    }

    void ForceMovePlayer(int direction) {
        rigidBody.velocity = P_VECTORS[direction] * speed * internalSpeedMultiplier;
    }

    void HandleMovement() {
        if (overrideDirection != -1) {
            // If input is not on movement axis, readjust
            if (overrideDirection % 2 != movementAxis) {
                if (movementAxis == 0) {
                    float offset = transform.position.y - (float) System.Math.Floor(transform.position.y);
                    if (offset > 0.5 + centerAlignmentTolerance) {
                        ForceMovePlayer(P_DOWN);
                    } else if (offset < 0.5 - centerAlignmentTolerance) {
                        ForceMovePlayer(P_UP);
                    } else {
                        transform.position = new Vector2(transform.position.x, (float) System.Math.Floor(transform.position.y) + 0.5f);
                        movementAxis = P_HORIZONTAL;
                        // Should we do this?
                        ForceMovePlayer(overrideDirection);
                    }
                } else {
                    float offset = transform.position.x - (float) System.Math.Floor(transform.position.x);
                    if (offset > 0.5 + centerAlignmentTolerance) {
                        ForceMovePlayer(P_LEFT);
                    } else if (offset < 0.5 - centerAlignmentTolerance) {
                        ForceMovePlayer(P_RIGHT);
                    } else {
                        transform.position = new Vector2((float) System.Math.Floor(transform.position.x) + 0.5f, transform.position.y);
                        movementAxis = P_VERTICAL;
                        // Should we do this?
                        ForceMovePlayer(overrideDirection);
                    }
                }
            } else {
                ForceMovePlayer(overrideDirection);
            }
            // else, move along axis
        } else {
            rigidBody.velocity = Vector2.zero;
        }

    }

    int mod(int x, int m) {
        return (x % m + m) % m;
    }
}
