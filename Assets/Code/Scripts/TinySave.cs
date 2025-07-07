using System;
using System.Collections.Generic;
using UnityEngine;

public class TinySave : MonoBehaviour
{
   
    const string KEY_HAS_PLAYED = "HasPlayedBefore";
    const string KEY_OBJECTS    = "SavedObjects";   // JSON blob


    [Tooltip("Drop any objects whose state you want to persist")]
    public List<GameObject> trackedObjects = new List<GameObject>();

  
    [Serializable]
    class ObjData
    {
        public string name;      // use object's name as identifier
        public bool   active;
        public float  x, y, z;   // position
    }

    public void Save()
    {
        // a) flag that the game has been opened once
        PlayerPrefs.SetInt(KEY_HAS_PLAYED, 1);

        // b) collect object data
        var list = new List<ObjData>();
        foreach (var go in trackedObjects)
        {
            if (!go) continue;   // skip null slots

            list.Add(new ObjData
            {
                name   = go.name,
                active = go.activeSelf,
                x = go.transform.position.x,
                y = go.transform.position.y,
                z = go.transform.position.z
            });
        }

        // c) dump to JSON and store in prefs
        string json = JsonUtility.ToJson(new Wrapper<ObjData>{ items = list });
        PlayerPrefs.SetString(KEY_OBJECTS, json);

        // d) flush to disk
        PlayerPrefs.Save();
        Debug.Log("TinySave");
    }

   
    public void Load()
    {
        bool hasPlayed = PlayerPrefs.GetInt(KEY_HAS_PLAYED, 0) == 1;
        Debug.Log("Has played before? " + hasPlayed);

        if (!PlayerPrefs.HasKey(KEY_OBJECTS)) return;

        string json = PlayerPrefs.GetString(KEY_OBJECTS);
        var wrapper = JsonUtility.FromJson<Wrapper<ObjData>>(json);

        foreach (var data in wrapper.items)
        {
            // find the matching object by name
            GameObject go = trackedObjects.Find(g => g && g.name == data.name);
            if (!go) continue;

            go.SetActive(data.active);
            go.transform.position = new Vector3(data.x, data.y, data.z);
        }
        Debug.Log("TinySave -> loaded");
    }

    public static bool HasPlayedBefore
    {
        get => PlayerPrefs.GetInt(KEY_HAS_PLAYED, 0) == 1;
    }


    private void Start()
    {
        if(HasPlayedBefore)
        {
            BasicFlowEngine basicFlowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
            //basicFlowEngine.ExecuteNode("Tutorial");
        }
        else
        {
            //get the basicflowengine
            BasicFlowEngine basicFlowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
            basicFlowEngine.ExecuteNode("FirstPlay");

            //save the game state
            Save();
        }
    }

    [Serializable]
    class Wrapper<T> { public List<T> items; }
}
