using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    (int, int) lastVisitedTile;
    Vector2 rawInput;
    Vector2 moveInput;
    Vector2 processedDirection = Vector2.zero;
    DungeonGeneration dungeonGeneration;

    bool isMoving = false;
    float internalSpeedMultiplier = 5f;
    float speed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        dungeonGeneration = FindObjectOfType<DungeonGeneration>();
        Vector2 rand = dungeonGeneration.FindRandomOpenTile();
        transform.position = rand + new Vector2(0.5f, 0.5f);
        lastVisitedTile = ((int) rand.x, (int) rand.y);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate() {
        if (!isMoving) {
            if (rawInput != Vector2.zero) {
                moveInput = new Vector2Int(System.Convert.ToInt32(rawInput.x), System.Convert.ToInt32(rawInput.y));
                processedDirection = CleanInputContextual(moveInput);
                StartCoroutine(MovePlayer());
            }
        }
    }

    // OnMove updates moveInput to one of the 9 directions with integer components
    // OnMove also updates overrideDirection with basic logic to only allow cardinals
    void OnMove(InputValue value) {
        rawInput = value.Get<Vector2>();
    }

    float effectiveSpeed() {
        return speed * internalSpeedMultiplier;
    }

    IEnumerator MovePlayer() {
        float timeToMove = 1f / effectiveSpeed();
        isMoving = true;
        float elapsedTime = 0;
        Vector3 start = transform.position;
        Vector3 end = start + new Vector3(processedDirection.x, processedDirection.y);

        if (dungeonGeneration.HasSolidTileAt(CoordToTuple(end))) {
            isMoving = false;
            yield return null;
        } else {
            while (elapsedTime < timeToMove) {
                transform.position = Vector3.Lerp(start, end, (elapsedTime / timeToMove));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = end;

            isMoving = false;
        }
    }

    Vector2 CleanInputContextual(Vector2 input) {
        // If input is zero or a clear cardinal, then use input
        if (input == Vector2.zero || input.magnitude == 1) {
            return input;
        }
        // If the previous movement was not parallel to the new input, take the "new" direction
        Vector2 v = HasPerpendicularComponent(processedDirection, moveInput);
        if (v != Vector2.zero) {
            return v;
        } else {
            return processedDirection;
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

    Vector2 FlattenVectorToX(Vector2 input) {
        return new Vector2(input.x, 0);
    }

    (int, int) CoordToTuple(Vector2 coord) {
        return (System.Convert.ToInt32(coord.x - 0.5f), System.Convert.ToInt32(coord.y - 0.5f));
    }

    Vector2 TupleToCoord((int, int) tup) {
        return new Vector2(tup.Item1 + 0.5f, tup.Item2 + 0.5f);
    }
}
