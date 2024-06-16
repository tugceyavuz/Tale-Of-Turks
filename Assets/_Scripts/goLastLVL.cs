using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class goLastLVL : MonoBehaviour
{
    public string sceneToLoad; // The name of the scene to load if the boss is not found
    public string bossName;

    void Update()
    {
       CheckForBoss();
    }

    void CheckForBoss()
    {
        // Try to find the FinalBoss(Clone) in the scene
        GameObject boss = GameObject.Find(bossName);

        // If the boss is not found, load the specified scene
        if (boss == null)
        {
            Debug.Log("FinalBoss(Clone) not found. Loading scene: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
