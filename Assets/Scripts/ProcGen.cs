using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ProcGen : MonoBehaviour
{
    [MenuItem("Tools/Generate New Level")] // Use tools from top bar to generate level
    public static void GenerateNewLevel() // Main function to handle level generation
    {
        float x = 6;    // Keep track of edge x
        float y = -2;   // Keep track of edge y
        float miny = -2;    // Value for deathzone
        int counter = 0;

        while (counter < 12)    // Generate 12 chunks
        {
            Vector2 newCords = CreateSafe(x, y);    // Create flat platform
            x = newCords.x;     // Update x
            y = newCords.y;     // Update y
            newCords = CreateDanger(x, y);      // Choose a dangerous chunk to create
            x = newCords.x;     // Update x
            y = newCords.y;     // Update y
            counter++;          
            if (y < miny)
            {
                miny = y;   // Update min y if a new minimun has been reached
            }
        }
        CreateEnd(x,y);     // Add flag to the end
        CreateDeathZone(x, miny);   // Update deathzone size/location based on level size
    }

    private static Vector2 CreateSafe(float x, float y) // Generate a flat chunk
    {
        float currentx = x;
        float currenty = y;
        int counter = 0;
        GameObject groundPrefab = Resources.Load<GameObject>("Ground");
        
        int randomValue = Random.Range(4, 6);   // Platform will be 4-5 in length
        while (counter < randomValue)   // Loop places each ground object to create the platform
        {
            GameObject ground = PrefabUtility.InstantiatePrefab(groundPrefab) as GameObject;
            ground.transform.position = new Vector3(currentx, currenty, 0);
            currentx += 2;
            counter++;
        }
        return new Vector2(currentx, currenty); // Return new edge x/y cords
    }

    private static Vector2 CreateDanger(float x, float y)   // Choose a dangerous chunk to create
    {
        float currentx = x;
        float currenty = y;
        int randomValue = Random.Range(0, 4);
        if (randomValue == 0)
        {
            return CreateGap(currentx, currenty);
        }
        else if (randomValue == 1)
        {
            return CreateJump(currentx, currenty);
        }
        else if (randomValue == 2)
        {
            return CreateShortJump(currentx, currenty);
        }
        else
        {
            return CreateBackJump(currentx, currenty);
        }
    }

    private static Vector2 CreateGap(float x, float y)  // Create a simple gap to jump over
    {
        float currentx = x;
        float currenty = y;
        int counter = 0;
        GameObject groundPrefab = Resources.Load<GameObject>("Ground");
        currentx += Random.Range(5, 8);

        int randomValue = Random.Range(4, 6);   // Gap length will be from 4-5
        while (counter < randomValue)   // Loop creates platform at other side of the gap 
        {
            GameObject ground = PrefabUtility.InstantiatePrefab(groundPrefab) as GameObject;
            ground.transform.position = new Vector3(currentx, currenty, 0);
            currentx += 2;
            counter++;
        }
        return new Vector2(currentx, currenty); // Return new edge x/y cords
    }

    private static Vector2 CreateJump(float x, float y) // Create a jump at a new elevation
    {
        float currentx = x;
        float currenty = y;
        int counter = 0;
        currentx += Random.Range(4,6);  // Gap length will be from 4-5
        int randomValue = Random.Range(0, 2); // Choose whether to increase elevation or drop elevation
        if (randomValue == 0)
        {
            currenty += 2;
        }
        else
        {
            currenty -= 2;
        }
        GameObject groundPrefab = Resources.Load<GameObject>("Ground");

        while (counter < 5) // Loop creates the platform at end of the jump
        {
            GameObject ground = PrefabUtility.InstantiatePrefab(groundPrefab) as GameObject;
            ground.transform.position = new Vector3(currentx, currenty, 0);
            currentx += 2;
            counter++;
        }
        return new Vector2(currentx, currenty); // Return new edge x/y cords
    }

    private static Vector2 CreateShortJump(float x, float y) // Create small jumps with small platforms to jump to, and can change elevation
    {
        float currentx = x;
        float currenty = y;
        int counter = 0;
        currentx += 2;

        GameObject groundPrefab = Resources.Load<GameObject>("Ground");
        int randomValue = Random.Range(2, 4);   // Amount of platforms will be 2-3
        int randomValue2 = Random.Range(-1, 2); // Change in elevation will be -1, 0, or 1
        while (counter < randomValue)   // Loop creates the small platforms with small gaps
        {
            GameObject ground = PrefabUtility.InstantiatePrefab(groundPrefab) as GameObject;
            ground.transform.position = new Vector3(currentx, currenty, 0);
            currentx += 4;
            currenty += randomValue2 * 2;
            counter++;
        }
        return new Vector2(currentx, currenty); // Return new edge x/y cords
    }

    private static Vector2 CreateBackJump(float x, float y) // Create a platform that will require player to jump back and up to progress
    {
        float currentx = x;
        float currenty = y;
        int counter = 0;
        currentx -= 6;
        currenty += 4;
        float edgex = currentx + 6;
        float edgey = currenty + 2;


        GameObject groundPrefab = Resources.Load<GameObject>("Ground");
        int randomValue = Random.Range(2, 4);   // Platform length will be 2-3
        while (counter < randomValue)   // Loop creates the platform
        {
            GameObject ground = PrefabUtility.InstantiatePrefab(groundPrefab) as GameObject;
            ground.transform.position = new Vector3(currentx, currenty, 0);
            currentx -= 2;
            counter++;
        }
        return new Vector2(edgex, edgey); // Return new edge x/y cords
    }

    private static void CreateEnd(float x, float y) // Create flat ground with flag at the end
    {
        float currentx = x;
        float currenty = y;
        int counter = 0;
        GameObject groundPrefab = Resources.Load<GameObject>("Ground");
        while (counter < 5) // Create the flat ground
        {
            GameObject ground = PrefabUtility.InstantiatePrefab(groundPrefab) as GameObject;
            ground.transform.position = new Vector3(currentx, currenty, 0);
            currentx += 2;
            counter++;
        }
        GameObject flagPrefab = Resources.Load<GameObject>("Flag");
        GameObject flag = PrefabUtility.InstantiatePrefab(flagPrefab) as GameObject;
        flag.transform.position = new Vector3(currentx-6, currenty+6);  // Place flag roughly in the middle
        return;
    }
    private static void CreateDeathZone(float x, float miny) // Edit the deathzone size/location at the end of level generation
    {
        GameObject deathZone = GameObject.Find("DeathZone");
        deathZone.transform.localScale = new Vector3(x + 30, 2, 1);
        deathZone.transform.position = new Vector3((x + 10) / 2, miny - 4, 0);
    }
}