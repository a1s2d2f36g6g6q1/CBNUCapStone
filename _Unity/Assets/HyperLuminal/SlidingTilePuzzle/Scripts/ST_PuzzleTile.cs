using System.Collections;
using UnityEngine;

public class ST_PuzzleTile : MonoBehaviour
{
    // the target position for this tile.
    public Vector3 TargetPosition;

    // is this an active tile?  usually one per game is inactive.
    public bool Active = true;

    // is this tile in the correct location?
    public bool CorrectLocation;

    // store this tiles array location.
    public Vector2 ArrayLocation;
    public Vector2 GridLocation;

    private void Awake()
    {
        // assign the new target position.
        TargetPosition = transform.localPosition;

        // start the movement coroutine to always move the objects to the new target position.
        StartCoroutine(UpdatePosition());
    }

    private void OnMouseDown()
    {
        // get the puzzle display and return the new target location from this tile. 
        LaunchPositionCoroutine(transform.parent.GetComponent<ST_PuzzleDisplay>()
            .GetTargetLocation(GetComponent<ST_PuzzleTile>()));
    }

    public void LaunchPositionCoroutine(Vector3 newPosition)
    {
        // assign the new target position.
        TargetPosition = newPosition;

        // start the movement coroutine to always move the objects to the new target position.
        StartCoroutine(UpdatePosition());
    }

    public IEnumerator UpdatePosition()
    {
        // whilst we are not at our target position.
        while (TargetPosition != transform.localPosition)
        {
            // lerp towards our target.
            transform.localPosition = Vector3.Lerp(transform.localPosition, TargetPosition, 10.0f * Time.deltaTime);
            yield return null;
        }

        // after each move check if we are now in the correct location.
        if (ArrayLocation == GridLocation)
            CorrectLocation = true;
        else
            CorrectLocation = false;

        // if we are not an active tile then hide our renderer and collider.
        if (Active == false)
        {
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
        }

        yield return null;
    }

    public void ExecuteAdditionalMove()
    {
        // get the puzzle display and return the new target location from this tile. 
        LaunchPositionCoroutine(transform.parent.GetComponent<ST_PuzzleDisplay>()
            .GetTargetLocation(GetComponent<ST_PuzzleTile>()));
    }
}