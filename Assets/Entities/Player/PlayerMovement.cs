using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    (int, int) lastVisitedTile;
    const int PLAYER_HORIZONTAL = 0;
    const int PLAYER_VERTICAL = 1;
    // rawInput is the input received directly from Unity from OnMove
    Vector2 rawInput;
    // moveInput is a vector in the same direction as rawInput, with integer coordinates
    // E.g. (0.71, 0.71) is stored as (1.0, 1.0) in moveInput
    Vector2 moveInput;
    // processedDirection is the output of CleanInputContextual. It is always a cardinal vector
    // The computation behind it can be found in the comment above CleanInputContextual
    Vector2 processedDirection = Vector2.zero;
    // lastDirection is the direction the player last moved in
    Vector2 lastDirection = Vector2.zero;

    // movePoint is a point representing the destination the player wishes to move to
    public Transform movePoint;
    // movePointTolerance is the distance the player must be from the center of the destination
    // tile in order to be able to move the movePoint again
    [SerializeField] float movePointTolerance = 0.00002f;

    DungeonGeneration dungeonGeneration;

    Animator animator;

    bool isMoving = false;
    [SerializeField] float internalSpeedMultiplier = 10f;
    [SerializeField] float speed = 1f;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        dungeonGeneration = FindObjectOfType<DungeonGeneration>();
        Vector2 rand = dungeonGeneration.FindRandomOpenTile();
        
        //Camera cam = FindObjectOfType<Camera>();
        //cam.transform.position = (Vector3) rand + new Vector3(0, 0, -1);

        transform.position = rand + new Vector2(0.5f, 0.5f);
        movePoint.parent = null;
        lastVisitedTile = ((int) rand.x, (int) rand.y);

        // Remove this at some point
        if (isMoving) {
            print("blah");
        }
    }

    void Update() {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, effectiveSpeed() * Time.deltaTime);
        if (Vector3.Distance(transform.position, movePoint.position) <= movePointTolerance) {
            moveInput = new Vector2Int(System.Convert.ToInt32(rawInput.x), System.Convert.ToInt32(rawInput.y));
            processedDirection = CleanInputContextual(moveInput);
            if (processedDirection != Vector2.zero) {
                isMoving = true;
                animator.SetBool("moving", true);
                Vector2 attemptedDestination = (Vector2) movePoint.position + processedDirection;
                if (!dungeonGeneration.HasSolidTileAt(CoordToTuple(attemptedDestination))) {
                    lastVisitedTile = CoordToTuple((Vector2) transform.position);
                    lastDirection = processedDirection;
                    movePoint.position = (Vector3) attemptedDestination;
                    AlignPlayerAfterMove();
                }
            } else {
                isMoving = false;
                animator.SetBool("moving", false);
            }
        }
    }

    void OnMove(InputValue value) {
        rawInput = value.Get<Vector2>();
    }

    float effectiveSpeed() {
        return speed * internalSpeedMultiplier;
    }

    // CleanInputContextual always returns a cardinal vector or the zero vector
    // It returns a vector that feels "natural" based on previous movements
    // For example, running diagonally into a wall will return a cardinal vector parallel to the wall
    // Running diagonally in open space alternates between the two cardinals making up the diagonal
    Vector2 CleanInputContextual(Vector2 input) {
        // If input is zero or a clear cardinal, then use input
        if (input == Vector2.zero || input.magnitude == 1) {
            return input;
        }
        Vector2 xVec = new Vector2(input.x, 0);
        Vector2 yVec = new Vector2(0, input.y);
        // If we are running diagonally into a wall, try to find an open direction
        if (dungeonGeneration.HasSolidTileAt(CoordToTuple((Vector2) transform.position + xVec))) {
            if (dungeonGeneration.HasSolidTileAt(CoordToTuple((Vector2) transform.position + yVec))) {
                return xVec;
            } else {
                return yVec;
            }
        }
        if (dungeonGeneration.HasSolidTileAt(CoordToTuple((Vector2) transform.position + yVec))) {
            return xVec;
        }

        // If the new movement is not parallel to the last taken movement, move in the "new" direction
        Vector2 v = HasPerpendicularComponent(lastDirection, input);
        if (v != Vector2.zero && !dungeonGeneration.HasSolidTileAt(CoordToTuple((Vector2) transform.position + v))) {
            return v;
        } else {
            return lastDirection;
        }
    }

    // HasPerpendicularComponent checks if input is parallel to card (which must be cardinal).
    // If not, then return a cardinal vector perpendicular to card which shares a component with input
    Vector2 HasPerpendicularComponent(Vector2 card, Vector2 input) {
        float angle = Vector2.Angle(card, input);
        if (angle <= float.Epsilon || angle >= (180 - float.Epsilon)) {
            return Vector2.zero;
        } else {
            Vector2 proj = Vector2.Dot(card, input) * card;
            return (input - proj);
        }
    }

    (int, int) CoordToTuple(Vector2 coord) {
        return (System.Convert.ToInt32(coord.x - 0.5f), System.Convert.ToInt32(coord.y - 0.5f));
    }

    Vector2 TupleToCoord((int, int) tup) {
        return new Vector2(tup.Item1 + 0.5f, tup.Item2 + 0.5f);
    }

    // AlignPlayerAfterMove should only be called after initiating a player movement and
    // updating lastDirection. This method will realign the player to the axis after this is done
    void AlignPlayerAfterMove() {
        if (lastDirection.x == 0) {
            AlignPlayerToAxis(PLAYER_HORIZONTAL);
        } else {
            AlignPlayerToAxis(PLAYER_VERTICAL);
        }
    }

    void AlignPlayerToAxis(int axis) {
        if (axis == PLAYER_HORIZONTAL) {
            float newX = System.Convert.ToInt32(transform.position.x - 0.5f) + 0.5f;
            transform.position = new Vector3(newX, transform.position.y, 0);
        } else if (axis == PLAYER_VERTICAL) {
            float newY = System.Convert.ToInt32(transform.position.y - 0.5f) + 0.5f;
            transform.position = new Vector3(transform.position.x, newY, 0);
        } else {
            print("PlayerMovement.cs: AlignPlayerToAxis: I hope this never prints!");
        }
    }
}
