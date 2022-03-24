using UnityEngine;

/* Overall game state management at a very global scale. */
public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        DataHandler.LoadGameData();
    }
}
