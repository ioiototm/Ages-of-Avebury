using LoGaCulture.LUTE;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    const string KEY_VARIABLES = "SavedVariables"; // JSON blob
    const string KEY_LAST_NODE = "LastNodeSeen";
    const string KEY_MESSAGES = "SavedMessages"; // JSON blob of messages

    [SerializeField]
    private GameObject messagePrefab;


    [Tooltip("Drop any objects whose state you want to persist")]
    public List<GameObject> trackedObjects = new List<GameObject>();

    //public static string LastNodeSeen { get; set; }

  
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

    [Serializable]
    class VariableData
    {
        public string name;
        public string type;
        public string value;
    }

    [Serializable]
    class MessageData
    {
        public string from;
        public string subject;
        public string message;
    }


    public void SaveMessages()
    {
        //messages are objects, and they have a From, Subject, and Message property that we want to save
        //go thorugh each message in InitialiseEverything._inboxCanvas, and go into ModernInbox/Scroll View/Viewport/Content and get all the Message objects
        //each message object has a subobject called FromField/FromField, SubjectField/SubjectField, and Panel/ContentsField
        //save them one by one in a list of MessageData objects

        if (InitialiseEverything._inboxCanvas == null)
        {
            Debug.LogWarning("TinySave: Inbox content transform not assigned. Cannot save messages.");
            return;
        }

        var messageList = new List<MessageData>();

        //using the InitialiseEverything._inboxCanvas, get the subobject called "Scroll View/Viewport/Content"
        Transform inboxContent = InitialiseEverything._inboxCanvas.transform.Find("ModernInbox/Scroll View/Viewport/Content");

        foreach (Transform messageObject in inboxContent)
        {
            // Find the text components within the message object hierarchy
            var fromText = messageObject.Find("FromField/FromField")?.GetComponent<TMP_Text>();
            var subjectText = messageObject.Find("SubjectField/SubjectField")?.GetComponent<TMP_Text>();
            // The comment mentions Panel/ContentsField. Assuming it's a TMP_Text component.
            var messageContent = messageObject.Find("Panel/ContentsField")?.GetComponent<TMP_Text>();

            if (fromText != null && subjectText != null && messageContent != null)
            {
                messageList.Add(new MessageData
                {
                    from = fromText.text,
                    subject = subjectText.text,
                    message = messageContent.text
                });
            }
            else
            {
                Debug.LogWarning($"TinySave: Could not find all required text components on message object '{messageObject.name}'.", messageObject);
            }
        }

        // Serialize the list to JSON and save to PlayerPrefs
        string json = JsonUtility.ToJson(new Wrapper<MessageData> { items = messageList });
        PlayerPrefs.SetString(KEY_MESSAGES, json);
        Debug.Log($"TinySave: Saved {messageList.Count} messages.");

    }

    // Load messages from PlayerPrefs, same but in reverse, using the prefab
    public void LoadMessages()
    {
        if (!PlayerPrefs.HasKey(KEY_MESSAGES))
        {
            Debug.LogWarning("TinySave: No saved messages found.");
            return;
        }
        string json = PlayerPrefs.GetString(KEY_MESSAGES);
        var wrapper = JsonUtility.FromJson<Wrapper<MessageData>>(json);
        // Clear existing messages in the inbox, a loop and destroy them
        Transform inboxContent = InitialiseEverything._inboxCanvas.transform.Find("ModernInbox/Scroll View/Viewport/Content");
        foreach (Transform child in inboxContent)
        {
            Destroy(child.gameObject);
        }




        foreach (var data in wrapper.items)
        {
            // Instantiate a new message prefab
            GameObject messageObject = Instantiate(messagePrefab, InitialiseEverything._inboxCanvas.transform.Find("ModernInbox/Scroll View/Viewport/Content"));
            var fromText = messageObject.transform.Find("FromField/FromField").GetComponent<TMP_Text>();
            var subjectText = messageObject.transform.Find("SubjectField/SubjectField").GetComponent<TMP_Text>();
            var contentText = messageObject.transform.Find("Panel/ContentsField").GetComponent<TMP_Text>();
            fromText.text = data.from;
            subjectText.text = data.subject;
            contentText.text = data.message;
        }
        Debug.Log($"TinySave: Loaded {wrapper.items.Count} messages.");
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

        // e) save last node seen
        if (!string.IsNullOrEmpty(LastNodeSeen))
        {
            PlayerPrefs.SetString(KEY_LAST_NODE, LastNodeSeen);
        }


        // g) flush to disk
        PlayerPrefs.Save();
        Debug.Log("TinySave: Saved objects, stones, and variables.");
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

        // Load last node seen
        if (PlayerPrefs.HasKey(KEY_LAST_NODE))
        {
            LastNodeSeen = PlayerPrefs.GetString(KEY_LAST_NODE);
        }

        

        if(PlayerPrefs.HasKey(KEY_OBJECTS) || PlayerPrefs.HasKey(KEY_STONES) || PlayerPrefs.HasKey(KEY_VARIABLES))
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

    private static bool _loadGame = false;

    public static bool loadGame
    {
        
        get => _loadGame;
        set
        {
        _loadGame = value;
           
        }

    }

    public static string LastNodeSeen
    {
        get => PlayerPrefs.GetString(KEY_LAST_NODE, "");
        set => PlayerPrefs.SetString(KEY_LAST_NODE, value);
    }

    private void Update()
    {
        //if press h
        if (Input.GetKeyDown(KeyCode.H))
        {
            SaveMessages();
            //// f) collect variable data
            //var variableList = new List<VariableData>();
            //BasicFlowEngine flowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
            //if (flowEngine != null)
            //{
            //    foreach (var variableName in flowEngine.GetVariableNames())
            //    {
            //        var variableObj = flowEngine.GetVariable(variableName);
            //        string serializedValue = "";
            //        string variableType = "";

            //        if (variableObj is BooleanVariable booleanVariable)
            //        {
            //            serializedValue = booleanVariable.Value.ToString();
            //            variableType = "bool";
            //        }
            //        else if (variableObj is StringVariable stringVariable)
            //        {
            //            serializedValue = stringVariable.Value;
            //            variableType = "string";
            //        }
            //        else if (variableObj is FloatVariable floatVariable)
            //        {
            //            serializedValue = floatVariable.Value.ToString();
            //            variableType = "float";
            //        }
            //        else if (variableObj is LocationVariable locationVariable)
            //        {
            //            //serializedValue = JsonUtility.ToJson(locationVariable.Value);
            //            //var data = locationVariable.Value
            //            variableType = "location";
            //        }

            //        if (!string.IsNullOrEmpty(variableType))
            //        {
            //            variableList.Add(new VariableData { name = variableName, type = variableType, value = serializedValue });
            //        }
            //    }
            //}
            //string variablesJson = JsonUtility.ToJson(new Wrapper<VariableData> { items = variableList });
            //PlayerPrefs.SetString(KEY_VARIABLES, variablesJson);


            // g) flush to disk
            PlayerPrefs.Save();


        }

        if(Input.GetKeyDown(KeyCode.L))
        {

            LoadMessages();
            //// Load variables
            //if (PlayerPrefs.HasKey(KEY_VARIABLES))
            //{
            //    string json = PlayerPrefs.GetString(KEY_VARIABLES);
            //    var wrapper = JsonUtility.FromJson<Wrapper<VariableData>>(json);
            //    BasicFlowEngine flowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();

            //    if (flowEngine != null && wrapper != null)
            //    {
            //        foreach (var data in wrapper.items)
            //        {
            //            var variableObj = flowEngine.GetVariable(data.name);
            //            if (variableObj == null) continue;

            //            switch (data.type)
            //            {
            //                case "bool":
            //                    if (variableObj is BooleanVariable boolVar && bool.TryParse(data.value, out bool boolValue))
            //                    {
            //                        boolVar.Value = boolValue;
            //                    }
            //                    break;
            //                case "string":
            //                    if (variableObj is StringVariable strVar)
            //                    {
            //                        strVar.Value = data.value;
            //                    }
            //                    break;
            //                case "float":
            //                    if (variableObj is FloatVariable floatVar && float.TryParse(data.value, out float floatValue))
            //                    {
            //                        floatVar.Value = floatValue;
            //                    }
            //                    break;
            //                case "location":
            //                    if (variableObj is LocationVariable locVar)
            //                    {
            //                        locVar.Value = JsonUtility.FromJson<LUTELocationInfo>(data.value);
            //                    }
            //                    break;
            //            }
            //        }
            //        Debug.Log($"TinySave: Loaded {wrapper.items.Count} variables from storage.");
            //    }
            //}

        }


    }

    

    private void Start()
    {
        DontDestroyOnLoad(gameObject); // Ensure this object persists across scenes

        //if current scene is MainMenu
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
        {
            GameObject loadButton = GameObject.Find("Button_Load");

            // If the game has been played before, load the saved state
            if (HasPlayedBefore)
            {
                loadButton.SetActive(true);
                loadGame = true; // Set the flag to indicate we can load
                //Load(); // Load the saved state
            }
            else
            {
                loadButton.SetActive(false);
                loadGame = false; // Set the flag to indicate we cannot load
            }
           
        }

        //if(HasPlayedBefore)
        //{
        //    Load();
        //    //BasicFlowEngine basicFlowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
        //    //if (basicFlowEngine != null && !string.IsNullOrEmpty(LastNodeSeen))
        //    //{
        //    //    basicFlowEngine.ExecuteNode(LastNodeSeen);
        //    //}
        //}
        //else
        //{
        //    //get the basicflowengine
        //    BasicFlowEngine basicFlowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
        //    basicFlowEngine.ExecuteNode("FirstPlay");

        //    //save the game state
        //    Save();
        //}




    }

    [Serializable]
    class Wrapper<T> { public List<T> items; }
}
