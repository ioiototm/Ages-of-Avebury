using System;
using System.Collections.Generic;
using UnityEngine;

public class TinySave : MonoBehaviour
{

    //static instance for singleton pattern
    public static TinySave Instance { get; private set; }
    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    const string KEY_HAS_PLAYED = "HasPlayedBefore";
    const string KEY_OBJECTS    = "SavedObjects";   // JSON blob
    const string KEY_STONES     = "SavedStones";    // JSON blob of meshes


    [Tooltip("Drop any objects whose state you want to persist")]
    public List<GameObject> trackedObjects = new List<GameObject>();

  
    [Serializable]
    class ObjData
    {
        public string name;      // use object's name as identifier
        public bool   active;
        public float  x, y, z;   // position
    }

    [Serializable]
    class StoneData
    {
        public string base64;
    }

    public void Save()
    {
        // a) flag that the game has been opened once
        PlayerPrefs.SetInt(KEY_HAS_PLAYED, 1);

        // b) collect object data
        var objList = new List<ObjData>();
        foreach (var go in trackedObjects)
        {
            if (!go) continue;   // skip null slots

            objList.Add(new ObjData
            {
                name   = go.name,
                active = go.activeSelf,
                x = go.transform.position.x,
                y = go.transform.position.y,
                z = go.transform.position.z
            });
        }

        // c) dump to JSON and store in prefs
        string json = JsonUtility.ToJson(new Wrapper<ObjData>{ items = objList });
        PlayerPrefs.SetString(KEY_OBJECTS, json);

        // d) collect stone data
        var stoneList = new List<StoneData>();
        foreach (var stone in Compass.meshesAndOutlines)
        {
            if (stone.mesh == null || stone.outline == null) continue;

            stoneList.Add(new StoneData
            {
                base64 = MeshSerializer.ToBase64(stone.mesh, stone.outline)
            });
        }

        // e) dump to JSON and store in prefs
        string stonesJson = JsonUtility.ToJson(new Wrapper<StoneData> { items = stoneList });
        PlayerPrefs.SetString(KEY_STONES, stonesJson);


        // f) flush to disk
        PlayerPrefs.Save();
        Debug.Log("TinySave: Saved objects and stones.");
    }

   
    public void Load()
    {
        bool hasPlayed = PlayerPrefs.GetInt(KEY_HAS_PLAYED, 0) == 1;
        Debug.Log("Has played before? " + hasPlayed);

        // Load tracked objects
        if (PlayerPrefs.HasKey(KEY_OBJECTS))
        {
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
        }

        // Load stones
        if (PlayerPrefs.HasKey(KEY_STONES))
        {
            Compass.meshesAndOutlines.Clear(); // Avoid duplicates

            string json = PlayerPrefs.GetString(KEY_STONES);
            var wrapper = JsonUtility.FromJson<Wrapper<StoneData>>(json);

            foreach (var data in wrapper.items)
            {
                MeshSerializer.FromBase64(data.base64, out Mesh mesh, out List<Vector2> outline);
                if (mesh != null)
                {
                    Compass.meshesAndOutlines.Add(new Compass.MeshAndOutline { mesh = mesh, outline = outline });
                }
            }
            Debug.Log($"TinySave: Loaded {Compass.meshesAndOutlines.Count} stones from storage.");
        }

        if(PlayerPrefs.HasKey(KEY_OBJECTS) || PlayerPrefs.HasKey(KEY_STONES))
        {
            Debug.Log("TinySave -> loaded");
        }
    }

    public static bool HasPlayedBefore
    {
        get => PlayerPrefs.GetInt(KEY_HAS_PLAYED, 0) == 1;
    }

    public static bool HasSavedStones
    {
        get => PlayerPrefs.HasKey(KEY_STONES);
    }


    private void Start()
    {
        if(HasPlayedBefore)
        {
            Load();
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
