using UnityEngine;
using Unity.Cinemachine;
using System.Collections;


public class MapTransition : MonoBehaviour
{
    //Defines the camera we're using
    [SerializeField] CinemachineCamera vcam;

    //The boundary we want the camera to move to
    [SerializeField] PolygonCollider2D mapBoundary;
    //will dictate which way the player is moved when passing through a doorway
    [SerializeField] Direction direction;

    //sets up the confiner variable and gives it the confiner of the current room
    [SerializeField] CinemachineConfiner2D confiner;

    
    enum Direction {Up, Down, Left, Right}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //in case the tag doesn't match the player
        if (!collision.CompareTag("Player"))
            return;

        //stores the current location of the player
        Vector3 oldPos = collision.transform.position;

        // Change confiner
        confiner.BoundingShape2D = mapBoundary;
        confiner.InvalidateBoundingShapeCache();

        // Move player
        UpdatePlayerPosition(collision.gameObject, oldPos);

        // Force one-frame cut
        StartCoroutine(ForceCameraCut());
    }


    private IEnumerator ForceCameraCut()
    {
        //disables dampening
        vcam.PreviousStateIsValid = false;
        yield return null; // wait one frame
        //re-enables dampening
        vcam.PreviousStateIsValid = true;
    }

    //moves the player forward in the direction they're facing to prevent
    //them from running into the trigger collider that goes in the opposite direction
    private void UpdatePlayerPosition(GameObject player, Vector3 oldPos)
    {
        Vector3 newPos = oldPos;

        switch(direction)
        {
            case Direction.Right:
                newPos.x += 3;
                break;
            case Direction.Left:
                newPos.x -= 3;
                break;
            case Direction.Up:
                newPos.y += 3;
                break;
            case Direction.Down:
                newPos.y -= 3;
                break;
        }

        //moves the player
        player.transform.position = newPos;
        //informs the camera that the object it was tracking just warped
        vcam.OnTargetObjectWarped(player.transform, newPos - oldPos);
    }
}
