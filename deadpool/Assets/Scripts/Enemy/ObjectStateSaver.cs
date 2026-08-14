using UnityEngine;

public class ObjectStateSaver : MonoBehaviour
{
    [Header("Save Settings")]
    [Tooltip("Type a completely unique name here! e.g., 'Level1_Secret_Shotgun' or 'Room2_RedEnemy'")]
    public string uniqueID;

    void Start()
    {
        // 1. Safety check! Did you forget to name it?
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogWarning("WARNING: " + gameObject.name + " has an empty Unique ID!");
            return;
        }

        // 2. Ask PlayerPrefs if this object was destroyed in the past
        // (0 means alive/untouched, 1 means dead/collected)
        if (PlayerPrefs.GetInt(uniqueID, 0) == 1)
        {
            // The object is dead! Erase it immediately before the player sees it.
            gameObject.SetActive(false);
        }
    }

    // Call this function the exact moment the enemy dies or the secret is picked up!
    public void MarkAsDestroyed()
    {
        if (!string.IsNullOrEmpty(uniqueID))
        {
            PlayerPrefs.SetInt(uniqueID, 1);
            PlayerPrefs.Save();
            Debug.Log("Saved state: " + uniqueID + " is now permanently gone.");
        }
    }
}