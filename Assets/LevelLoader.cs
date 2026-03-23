using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;

    public enum transitionDirection
    {
        goingRight = 1,
        goingLeft = -1
    }

    [SerializeField] 
    private transitionDirection indexDirection;

    //runs when the player runs into the trigger area
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //in case the tag doesn't match the player
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        LoadNextLevel();

        // Move player
        //UpdatePlayerPosition(collision.gameObject, oldPos);
    }

    public void LoadNextLevel()
    {
        //Should change
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + (int)indexDirection));
    }

    //a coroutine. used to handle timed events without freezing the entire game.
    //in this instance, it's being used to make sure the animation gets played
    //before the next scene loads
    IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelIndex);
    }
}

/*
public LoadDirection direction = LoadDirection.Next;
public enum LoadDirection
{
    Next,
    Previous
}
public LoadDirection direction;

In Unity, any public enum field automatically shows as a dropdown in the Inspector.

So when Unity sees:

public LoadDirection direction;

and LoadDirection is an enum, it renders:

Direction ▼
  Next
  Previous
*/