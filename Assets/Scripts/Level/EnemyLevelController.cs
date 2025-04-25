// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 4/25/2025
// Course: EECS 582
// Purpose: manages enemy level to detect when all enemies are defeated to reveal flag

using UnityEngine;

public class EnemyLevelController : MonoBehaviour
{
    [SerializeField] private GameObject enemies; //single game object that holds all enemies as children
    [SerializeField] private GameObject flag; //flag game object

    void Start()
    {
        flag.SetActive(false); //disable flag to start
    }

    void Update()
    {
        if (allEnemiesDead()) {
            flag.SetActive(true);
        }
    }

    //returns true if all enemies have been killed, false otherwise
    private bool allEnemiesDead()
    {
        foreach (Transform enemy in enemies.transform) {
            if(enemy.gameObject.activeSelf == true) {
                return false;
            }
        }

        return true;
    }
}
