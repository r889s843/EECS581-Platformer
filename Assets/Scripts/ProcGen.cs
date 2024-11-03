using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ProcGen : MonoBehaviour
{
    private static List<GameObject> generatedObjects = new List<GameObject>();

    [MenuItem("Tools/Generate New Level")]
    public static void GenerateNewLevel()
    {
        float x = 6;
        float y = -2;
        int counter = 0;

        while (counter < 7)
        {
            Vector2 newCords = CreateSafe(x, y);
            x = newCords.x;
            y = newCords.y;
            newCords = CreateDanger(x, y);
            x = newCords.x;
            y = newCords.y;
            counter++;
        }
        CreateEnd(x,y);
    }

    [MenuItem("Tools/Clear Generated Level")]
    public static void ClearGeneratedLevel()
    {
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj); 
            }
        }
        generatedObjects.Clear();
    }

    private static Vector2 CreateSafe(float x, float y)
    {
        float currentx = x;
        float currenty = y;
        int counter = 0;
        GameObject groundPrefab = Resources.Load<GameObject>("Ground");

        while (counter < 5)
        {
            GameObject ground = PrefabUtility.InstantiatePrefab(groundPrefab) as GameObject;
            ground.transform.position = new Vector3(currentx, currenty, 0);
            generatedObjects.Add(ground);
            currentx += 2;
            counter++;
        }
        return new Vector2(currentx, currenty);
    }

    private static Vector2 CreateDanger(float x, float y)
    {
        float currentx = x;
        float currenty = y;
        int randomValue = Random.Range(0, 2);
        if (randomValue == 0) {
            return CreateGap(currentx, currenty);
        }
        else {
            return CreateJump(currentx, currenty);
        }
    }

    private static Vector2 CreateGap(float x, float y)
    {
        float currentx = x;
        float currenty = y;
        int counter = 0;
        GameObject groundPrefab = Resources.Load<GameObject>("Ground");
        currentx += 7;
        while (counter < 5)
        {
            GameObject ground = PrefabUtility.InstantiatePrefab(groundPrefab) as GameObject;
            ground.transform.position = new Vector3(currentx, currenty, 0);
            generatedObjects.Add(ground);
            currentx += 2;
            counter++;
        }
        return new Vector2(currentx, currenty);
    }

    private static Vector2 CreateJump(float x, float y)
    {
        float currentx = x;
        float currenty = y;
        int counter = 0;
        currentx += 5;
        int randomValue = Random.Range(0, 2);
        if (randomValue == 0)
        {
            currenty += 2;
        }
        else
        {
            currenty -= 2;
        }
        GameObject groundPrefab = Resources.Load<GameObject>("Ground");

        while (counter < 5)
        {
            GameObject ground = PrefabUtility.InstantiatePrefab(groundPrefab) as GameObject;
            ground.transform.position = new Vector3(currentx, currenty, 0);
            generatedObjects.Add(ground);
            currentx += 2;
            counter++;
        }
        return new Vector2(currentx, currenty);
    }

    private static void CreateEnd(float x, float y)
    {
        float currentx = x;
        float currenty = y;
        int counter = 0;
        GameObject groundPrefab = Resources.Load<GameObject>("Ground");
        while (counter < 5)
        {
            GameObject ground = PrefabUtility.InstantiatePrefab(groundPrefab) as GameObject;
            ground.transform.position = new Vector3(currentx, currenty, 0);
            generatedObjects.Add(ground);
            currentx += 2;
            counter++;
        }
        GameObject flagPrefab = Resources.Load<GameObject>("Flag");
        GameObject flag = PrefabUtility.InstantiatePrefab(flagPrefab) as GameObject;
        flag.transform.position = new Vector3(currentx-6, currenty+6);
        generatedObjects.Add(flag);
        return;
    }
}