using LoGaCulture.LUTE;
using LoGaCulture.LUTE.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO; // added for file-based cache

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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    const string KEY_HAS_PLAYED = "HasPlayedBefore";
    const string KEY_OBJECTS = "SavedObjects";   // JSON blob
    const string KEY_STONES = "SavedStones";    // JSON blob of meshes
    const string KEY_VARIABLES = "SavedVariables"; // JSON blob
    const string KEY_LAST_NODE = "LastNodeSeen";
    const string KEY_MESSAGES = "SavedMessages"; // JSON blob of messages
    const string KEY_MEDIEVAL_MESSAGES = "SavedMedievalMessages"; // JSON blob of medieval messages
    const string KEY_STONE_DECISION = "StoneDecision"; // JSON blob of stone decisions
    const string KEY_STONE_DATA_FIRST = "StoneDataFirst"; // JSON blob of stone 1 data
    const string KEY_STONE_DATA_SECOND = "StoneDataSecond"; // JSON blob of stone 2 data
    const string KEY_STONE_DATA_THIRD_SOCIAL = "StoneDataThirdSocial"; // JSON blob of stone 3 data
    const string KEY_GAME_COMPLETED = "GameCompleted"; // bool for game completed
    const string KEY_STONE_OUTCOME_PREFIX = "StoneOutcome_"; // cached per-stone outcome flag

    private static readonly Dictionary<int, GameObject> cachedSavedStonePrefabs = new();

    // File-based cache paths
    private const string StoneCacheFileName = "stonecache.json";
    private static string PersistentStoneCachePath => Path.Combine(Application.persistentDataPath, StoneCacheFileName);
    private const string ResourcesStoneCacheNameNoExt = "stonecache"; // Resources.Load<TextAsset>("stonecache")


    [SerializeField]
    private GameObject messagePrefab;

    [SerializeField]
    private GameObject medievalMessagePrefab;


    [Tooltip("Drop any objects whose state you want to persist")]
    public List<GameObject> trackedObjects = new List<GameObject>();

    //public static string LastNodeSeen { get; set; }


    [Serializable]
    class ObjData
    {
        public string name;      // use object's name as identifier
        public bool active;
        public float x, y, z;   // position
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
    class LocationSaveData
    {
        public LocationStatus Status;
        public bool Hidden;
        public bool Disabled;
    }

    [Serializable]
    class MessageData
    {
        public string from;
        public string subject;
        public string message;
        public bool highlighted;
    }

    [Serializable]
    public class StoneDecisionData
    {
        public DecisionMedieval.StoneDecision stoneDecision;
        public int WhichStone; // 1 = first stone, 2 = second stone, 3 = social stone
    }

    [Serializable]
    public class StoneDecisionCollection
    {
        public List<StoneDecisionData> decisions = new List<StoneDecisionData>();
    }

    public enum StoneOutcome
    {
        Unknown = 0,
        Saved = 1,
        Destroyed = 2
    }

    private static bool IsValidStoneIndex(int whichStone) => whichStone >= 1 && whichStone <= 3;

    private static string GetStoneOutcomeKey(int whichStone) => $"{KEY_STONE_OUTCOME_PREFIX}{whichStone}";

    private static bool TryGetStoneDataKey(int whichStone, out string key)
    {
        key = whichStone switch
        {
            1 => KEY_STONE_DATA_FIRST,
            2 => KEY_STONE_DATA_SECOND,
            3 => KEY_STONE_DATA_THIRD_SOCIAL,
            _ => string.Empty
        };
        return !string.IsNullOrEmpty(key);
    }

    private static void CacheStoneOutcome(int whichStone, StoneOutcome outcome)
    {
        if (!IsValidStoneIndex(whichStone)) return;
        PlayerPrefs.SetInt(GetStoneOutcomeKey(whichStone), (int)outcome);
    }

    private static void CacheStoneOutcomeFromDecision(DecisionMedieval.StoneDecision decision, int whichStone)
    {
        if (decision == null || !IsValidStoneIndex(whichStone)) return;
        CacheStoneOutcome(whichStone, decision.Save ? StoneOutcome.Saved : StoneOutcome.Destroyed);
    }

    private static void BackfillStoneOutcomeCache(StoneDecisionCollection collection)
    {
        if (collection?.decisions == null) return;

        bool updatedCache = false;
        foreach (var decisionData in collection.decisions)
        {
            if (decisionData?.stoneDecision == null || !IsValidStoneIndex(decisionData.WhichStone)) continue;

            var desiredOutcome = decisionData.stoneDecision.Save ? StoneOutcome.Saved : StoneOutcome.Destroyed;
            var currentOutcome = GetStoneOutcome(decisionData.WhichStone);
            if (currentOutcome == desiredOutcome) continue;

            CacheStoneOutcome(decisionData.WhichStone, desiredOutcome);
            updatedCache = true;
        }

        if (updatedCache)
        {
            PlayerPrefs.Save();
        }
    }

    public static StoneOutcome GetStoneOutcome(int whichStone)
    {
        if (!IsValidStoneIndex(whichStone)) return StoneOutcome.Unknown;
        return (StoneOutcome)PlayerPrefs.GetInt(GetStoneOutcomeKey(whichStone), (int)StoneOutcome.Unknown);
    }

    public static bool TryGetStoneOutcome(int whichStone, out StoneOutcome outcome)
    {
        outcome = GetStoneOutcome(whichStone);
        return outcome != StoneOutcome.Unknown;
    }

    public static bool StoneWasSaved(int whichStone) => GetStoneOutcome(whichStone) == StoneOutcome.Saved;

    public static bool StoneWasDestroyed(int whichStone) => GetStoneOutcome(whichStone) == StoneOutcome.Destroyed;

    public static bool HasPlayerStoneData(int whichStone)
    {
        return TryGetStoneDataKey(whichStone, out var key) && PlayerPrefs.HasKey(key);
    }

    public static bool HasAnyPlayerCreatedStones => HasPlayerStoneData(1) || HasPlayerStoneData(2) || HasPlayerStoneData(3);

    public static bool TryGetSavedStoneMeshData(int whichStone, out Mesh mesh, out List<Vector2> outline)
    {
        mesh = null;
        outline = null;

        if (!TryGetStoneDataKey(whichStone, out string key) || !PlayerPrefs.HasKey(key))
        {
            return false;
        }

        string base64 = PlayerPrefs.GetString(key);
        if (string.IsNullOrEmpty(base64))
        {
            return false;
        }

        try
        {
            MeshSerializer.FromBase64(base64, out mesh, out outline);
            return mesh != null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"TinySave: Failed to decode saved stone {whichStone}. {ex}");
            mesh = null;
            outline = null;
            return false;
        }
    }

    public static GameObject GetSavedStonePrefab(int whichStone, Material stoneMaterial, Material outlineMaterial)
    {

        Debug.Log($"TinySave: Getting saved stone prefab for stone {whichStone}.");

        if (!IsValidStoneIndex(whichStone)) return null;

        if (cachedSavedStonePrefabs.TryGetValue(whichStone, out GameObject cached) && cached != null)
        {
            return cached;
        }

        if (!TryGetSavedStoneMeshData(whichStone, out Mesh mesh, out _))
        {
            return null;
        }

        GameObject prefab = new GameObject($"PlayerStone_{whichStone}");
        var filter = prefab.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        var renderer = prefab.AddComponent<MeshRenderer>();
        if (stoneMaterial != null && outlineMaterial != null)
        {
            renderer.sharedMaterials = new[] { stoneMaterial, outlineMaterial };
        }
        else if (stoneMaterial != null)
        {
            renderer.sharedMaterial = stoneMaterial;
        }

        prefab.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(prefab);

        cachedSavedStonePrefabs[whichStone] = prefab;
        return prefab;
    }

    public static bool GetGameCompletionFlag()
    {
        return PlayerPrefs.GetInt(KEY_GAME_COMPLETED, 0) == 1;
    }


    public static StoneCreator stone1, stone2, stone3;

    public void SaveStoneData(string stoneBase64, int whichStone)
    {
        string key = whichStone switch
        {
            1 => KEY_STONE_DATA_FIRST,
            2 => KEY_STONE_DATA_SECOND,
            3 => KEY_STONE_DATA_THIRD_SOCIAL,
            _ => throw new ArgumentOutOfRangeException(nameof(whichStone), "whichStone must be 1, 2, or 3.")
        };
        PlayerPrefs.SetString(key, stoneBase64);
        PlayerPrefs.Save();
    }

    public void CompleteGame()
    {
        PlayerPrefs.SetInt(KEY_GAME_COMPLETED, 1);
        PlayerPrefs.Save();
    }

    public bool IsGameCompleted()
    {
        return GetGameCompletionFlag();
    }

    public void ResetGameCompletion()
    {
        PlayerPrefs.DeleteKey(KEY_GAME_COMPLETED);
        PlayerPrefs.Save();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            RefreshMainMenuUI();
        }
    }

    private void RefreshMainMenuUI()
    {
        // Find main menu buttons; bail out if not present
        GameObject playButton = GameObject.Find("Button_Play");
        GameObject loadButton = GameObject.Find("Button_Load");
        GameObject modelButton = GameObject.Find("Button_3DModel");

        if (playButton == null || loadButton == null || modelButton == null)
        {
            Debug.LogWarning("TinySave: Main Menu UI elements not found in scene.");
            //return;
        }

        // Update Play / Load
        TMP_Text playButtonText = playButton.GetComponentInChildren<TMP_Text>();
        if (playButtonText != null)
        {
            if (HasPlayedBefore)
            {
                playButtonText.text = "START NEW GAME";
                loadButton.SetActive(true);
                loadGame = true;
            }
            else
            {
                playButtonText.text = "PLAY GAME";
                loadButton.SetActive(false);
                loadGame = false;
            }
        }

        // Update 3D Model visibility based on completion
        modelButton.SetActive(IsGameCompleted());
    }

    public void LoadAllStoneData()
    {

        var mapCompletion = GameObject.Find("MapComplete").GetComponent<MapCompletion>();


        //if there is a saved stone 1
        if (PlayerPrefs.HasKey(KEY_STONE_DATA_FIRST))
        {
            string stoneBase64 = PlayerPrefs.GetString(KEY_STONE_DATA_FIRST);

            Mesh stoneMesh = null;
            List<Vector2> outline = new List<Vector2>();

            MeshSerializer.FromBase64(stoneBase64, out stoneMesh, out outline);


            if (stone1 != null)
            {
                stone1.GetComponent<MeshFilter>().sharedMesh = stoneMesh;
                stone1.outlinePoints = outline;

                mapCompletion.createdStone1 = stone1.gameObject;
            }
            else
            {
                Debug.LogError("stone1 is not assigned in TinySave.");
            }

        }

        if (PlayerPrefs.HasKey(KEY_STONE_DATA_SECOND))
        {
            string stoneBase64 = PlayerPrefs.GetString(KEY_STONE_DATA_SECOND);
            Mesh stoneMesh = null;
            List<Vector2> outline = new List<Vector2>();
            MeshSerializer.FromBase64(stoneBase64, out stoneMesh, out outline);

            if (stone2 != null)
            {
                stone2.GetComponent<MeshFilter>().sharedMesh = stoneMesh;
                stone2.outlinePoints = outline;

                mapCompletion.createdStone2 = stone2.gameObject;
            }
            else
            {
                Debug.LogError("stone2 is not assigned in TinySave.");
            }
        }
        if (PlayerPrefs.HasKey(KEY_STONE_DATA_THIRD_SOCIAL))
        {
            string stoneBase64 = PlayerPrefs.GetString(KEY_STONE_DATA_THIRD_SOCIAL);
            Mesh stoneMesh = null;
            List<Vector2> outline = new List<Vector2>();
            MeshSerializer.FromBase64(stoneBase64, out stoneMesh, out outline);


            if (stone3 != null)
            {

                stone3.GetComponent<MeshFilter>().sharedMesh = stoneMesh;
                stone3.outlinePoints = outline;

                mapCompletion.foundStone = stone3.gameObject;
            }
            else
            {
                Debug.LogError("stone3 is not assigned in TinySave.");
            }
        }
    }

    public void SaveStoneMedieval(DecisionMedieval.StoneDecision decision, int whichStone)
    {
        StoneDecisionCollection collection;
        if (PlayerPrefs.HasKey(KEY_STONE_DECISION))
        {
            string json = PlayerPrefs.GetString(KEY_STONE_DECISION);
            collection = JsonUtility.FromJson<StoneDecisionCollection>(json) ?? new StoneDecisionCollection();
        }
        else
        {
            collection = new StoneDecisionCollection();
        }

        // Find if a decision for this stone already exists
        var existingDecision = collection.decisions.FirstOrDefault(d => d.WhichStone == whichStone);
        if (existingDecision != null)
        {
            // Update existing decision
            existingDecision.stoneDecision = decision;
        }
        else
        {
            // Add new decision
            collection.decisions.Add(new StoneDecisionData { stoneDecision = decision, WhichStone = whichStone });
        }
        CacheStoneOutcomeFromDecision(decision, whichStone);

        // Serialize the entire collection and save it
        string updatedJson = JsonUtility.ToJson(collection);
        PlayerPrefs.SetString(KEY_STONE_DECISION, updatedJson);
        PlayerPrefs.Save();
    }



    public StoneDecisionCollection LoadStoneMedieval()
    {
        return LoadStoneMedievalFromPrefs();
    }

    public static StoneDecisionCollection LoadStoneMedievalFromPrefs()
    {
        if (PlayerPrefs.HasKey(KEY_STONE_DECISION))
        {
            string json = PlayerPrefs.GetString(KEY_STONE_DECISION);
            var collection = JsonUtility.FromJson<StoneDecisionCollection>(json) ?? new StoneDecisionCollection();
            BackfillStoneOutcomeCache(collection);
            return collection;
        }

        return new StoneDecisionCollection();
    }




    public void SaveMessagesMedieval()
    {
        var listOfMessages = InitialiseEverything._inboxMiddleCanvas.transform.Find("Scroll View/Viewport/Content").gameObject;

        var messageList = new List<MessageData>();


        foreach (Transform messageObject in listOfMessages.transform)
        {
            var fromField = messageObject.Find("MsgBG/FromField/FromField");
            var contentField = messageObject.Find("MsgBG/ContentsField");

            if (fromField != null && contentField != null)
            {
                var fromText = fromField.GetComponent<TMP_Text>();
                var contentText = contentField.GetComponent<TMP_Text>();
                if (fromText != null && contentText != null)
                {

                    messageList.Add(new MessageData
                    {
                        from = fromText.text,
                        subject = "Medieval Message", 
                        message = contentText.text,
                        highlighted = false 
                    });

                    Debug.Log($"TinySave: Saved message from '{fromText.text}' with content '{contentText.text}'.");
                }
                else
                {
                    Debug.LogWarning("TinySave: Could not find TMP_Text components in message object.", messageObject);
                }
            }
            else
            {
                Debug.LogWarning("TinySave: Could not find FromField or ContentsField in message object.", messageObject);
            }
        }

        // Serialize the list to JSON and save to PlayerPrefs
        string json = JsonUtility.ToJson(new Wrapper<MessageData> { items = messageList });
        PlayerPrefs.SetString(KEY_MEDIEVAL_MESSAGES, json);
        PlayerPrefs.Save(); // Ensure changes are saved immediately
        Debug.Log($"TinySave: Saved {messageList.Count} medieval messages.");



    }


    public void SaveMessages()
    {
        if (InitialiseEverything._inboxCanvas == null)
        {
            Debug.LogWarning("TinySave: Inbox content transform not assigned. Cannot save messages.");
            return;
        }

        var messageList = new List<MessageData>();

        Transform inboxContent = InitialiseEverything._inboxCanvas.transform.Find("ModernInbox/Scroll View/Viewport/Content");

        foreach (Transform messageObject in inboxContent)
        {
            var fromText = messageObject.Find("FromField/FromField")?.GetComponent<TMP_Text>();
            var subjectText = messageObject.Find("SubjectField/SubjectField")?.GetComponent<TMP_Text>();
            var messageContent = messageObject.Find("Panel/ContentsField")?.GetComponent<TMP_Text>();

            var highlighted = messageObject.Find("Highlight")?.gameObject.activeSelf ?? false;

            if (fromText != null && subjectText != null && messageContent != null)
            {
                messageList.Add(new MessageData
                {
                    from = fromText.text,
                    subject = subjectText.text,
                    message = messageContent.text,
                    highlighted = highlighted
                });
            }
            else
            {
                Debug.LogWarning($"TinySave: Could not find all required text components on message object '{messageObject.name}'.", messageObject);
            }
        }

        string json = JsonUtility.ToJson(new Wrapper<MessageData> { items = messageList });
        PlayerPrefs.SetString(KEY_MESSAGES, json);
        PlayerPrefs.Save();
        Debug.Log($"TinySave: Saved {messageList.Count} messages.");

    }

    public void LoadMessages()
    {
        if (!PlayerPrefs.HasKey(KEY_MESSAGES))
        {
            Debug.LogWarning("TinySave: No saved messages found.");
            return;
        }
        string json = PlayerPrefs.GetString(KEY_MESSAGES);
        var wrapper = JsonUtility.FromJson<Wrapper<MessageData>>(json);

        Transform inboxContent = InitialiseEverything._inboxCanvas.transform.Find("ModernInbox/Scroll View/Viewport/Content");
        foreach (Transform child in inboxContent)
        {
            Destroy(child.gameObject);
        }


        bool unreadMessages = false;

        foreach (var data in wrapper.items)
        {
            GameObject messageObject = Instantiate(messagePrefab, InitialiseEverything._inboxCanvas.transform.Find("ModernInbox/Scroll View/Viewport/Content"));
            var fromText = messageObject.transform.Find("FromField/FromField").GetComponent<TMP_Text>();
            var subjectText = messageObject.transform.Find("SubjectField/SubjectField").GetComponent<TMP_Text>();
            var contentText = messageObject.transform.Find("Panel/ContentsField").GetComponent<TMP_Text>();
            var highlightObject = messageObject.transform.Find("Highlight");
            fromText.text = data.from;
            subjectText.text = data.subject;
            contentText.text = data.message;
            if (highlightObject != null)
            {
                highlightObject.gameObject.SetActive(data.highlighted);
                if (data.highlighted)
                {
                    unreadMessages = true;
                }
            }
            else
            {
                Debug.LogWarning($"TinySave: Highlight object not found in message prefab for '{data.from}'.", messageObject);
            }
        }
        Debug.Log($"TinySave: Loaded {wrapper.items.Count} messages.");

        if(unreadMessages)
        {
            BasicFlowEngine flowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
            flowEngine.GetVariable<BooleanVariable>("unreadMessages").Value = true;
        }
        else
        {
            Debug.Log("TinySave: All messages are read.");
        }
       
    }

    public void LoadMedievalMessages()
    {
        if (!PlayerPrefs.HasKey(KEY_MEDIEVAL_MESSAGES))
        {
            Debug.LogWarning("TinySave: No saved medieval messages found.");
            return;
        }
        string json = PlayerPrefs.GetString(KEY_MEDIEVAL_MESSAGES);
        var wrapper = JsonUtility.FromJson<Wrapper<MessageData>>(json);
        Transform inboxContent = InitialiseEverything._inboxMiddleCanvas.transform.Find("Scroll View/Viewport/Content");
        foreach (Transform child in inboxContent)
        {
            Destroy(child.gameObject);
        }
        foreach (var data in wrapper.items)
        {
            GameObject messageObject = Instantiate(medievalMessagePrefab, InitialiseEverything._inboxMiddleCanvas.transform.Find("Scroll View/Viewport/Content"));
            var fromText = messageObject.transform.Find("MsgBG/FromField/FromField").GetComponent<TMP_Text>();
            var contentText = messageObject.transform.Find("MsgBG/ContentsField").GetComponent<TMP_Text>();
            fromText.text = data.from;
            contentText.text = data.message;
        }
        Debug.Log($"TinySave: Loaded {wrapper.items.Count} medieval messages.");
    }

    public void SaveEngineVariables()
    {
        var variableList = new List<VariableData>();
        BasicFlowEngine flowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
        if (flowEngine != null)
        {
            foreach (var variableName in flowEngine.GetVariableNames())
            {
                var variableObj = flowEngine.GetVariable(variableName);
                string serializedValue = "";
                string variableType = "";


                if(variableObj is IntegerVariable intVariable)
                {
                    serializedValue = intVariable.Value.ToString();
                    variableType = "int";
                }
                else if (variableObj is BooleanVariable booleanVariable)
                {
                    serializedValue = booleanVariable.Value.ToString();
                    variableType = "bool";
                }
                else if (variableObj is StringVariable stringVariable)
                {
                    serializedValue = stringVariable.Value;
                    variableType = "string";
                }
                else if (variableObj is FloatVariable floatVariable)
                {
                    serializedValue = floatVariable.Value.ToString();
                    variableType = "float";
                }
                else if (variableObj is LocationVariable locationVariable)
                {
                    var locationData = new LocationSaveData
                    {
                        Status = locationVariable.Value.LocationStatus,
                        Hidden = locationVariable.Value.LocationHidden,
                        Disabled = locationVariable.Value.LocationDisabled
                    };

                    serializedValue = JsonUtility.ToJson(locationData);
                    variableType = "location";
                }

                if (!string.IsNullOrEmpty(variableType))
                {
                    variableList.Add(new VariableData { name = variableName, type = variableType, value = serializedValue });
                }
            }
        }
        string variablesJson = JsonUtility.ToJson(new Wrapper<VariableData> { items = variableList });
        PlayerPrefs.SetString(KEY_VARIABLES, variablesJson);
        PlayerPrefs.Save();
    }


    public void LoadEngineVariables()
    {
        if (PlayerPrefs.HasKey(KEY_VARIABLES))
        {
            string json = PlayerPrefs.GetString(KEY_VARIABLES);
            var wrapper = JsonUtility.FromJson<Wrapper<VariableData>>(json);
            BasicFlowEngine flowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
            if (flowEngine != null && wrapper != null)
            {
                foreach (var data in wrapper.items)
                {
                    var variableObj = flowEngine.GetVariable(data.name);
                    if (variableObj == null) continue;
                    switch (data.type)
                    {
                        case "int":
                            if (variableObj is IntegerVariable intVar && int.TryParse(data.value, out int intValue))
                            {
                                intVar.Value = intValue;
                            }
                            break;

                        case "bool":
                            if (variableObj is BooleanVariable boolVar && bool.TryParse(data.value, out bool boolValue))
                            {
                                boolVar.Value = boolValue;
                            }
                            break;
                        case "string":
                            if (variableObj is StringVariable strVar)
                            {
                                strVar.Value = data.value;
                            }
                            break;
                        case "float":
                            if (variableObj is FloatVariable floatVar && float.TryParse(data.value, out float floatValue))
                            {
                                floatVar.Value = floatValue;
                            }
                            break;
                        case "location":
                            if (variableObj is LocationVariable locVar)
                            {
                                LocationSaveData locationData = JsonUtility.FromJson<LocationSaveData>(data.value);
                                
                                locVar.Value.LocationStatus = locationData.Status;
                                locVar.Value.LocationHidden = locationData.Hidden;
                                locVar.Value.LocationDisabled = locationData.Disabled;

                                if(!locationData.Hidden)
                                {
                                    flowEngine.GetMapManager().ShowLocationMarker(locVar);
                                }

                                var match = System.Text.RegularExpressions.Regex.Match(locVar.Value.name, @"^\d+");
                                if (match.Success && int.TryParse(match.Value, out int locationID))
                                {
                                    if (locationID > 5)
                                    {
                                        locVar.Value.LocationHidden = true;
                                        flowEngine.GetMapManager().HideLocationMarker(locVar);
                                    }

                                    if (locationID == 1)
                                    {
                                        locVar.Value.LocationStatus = LocationStatus.Completed;
                                    }
                                }

                            }
                            break;
                    }
                }
                Debug.Log($"TinySave: Loaded {wrapper.items.Count} variables from storage.");
            }
        }
    }

    public void SaveTheStones()
    {

        return; 
        var stoneList = new List<StoneData>();
        foreach (var stone in Compass.meshesAndOutlines)
        {
            if (stone.mesh == null || stone.outline == null) continue;
            stoneList.Add(new StoneData
            {
                base64 = MeshSerializer.ToBase64(stone.mesh, stone.outline)
            });
        }
        string json = JsonUtility.ToJson(new Wrapper<StoneData> { items = stoneList });
        PlayerPrefs.SetString(KEY_STONES, json);
        PlayerPrefs.Save();
        Debug.Log($"TinySave: Saved {stoneList.Count} stones.");
    }

    // New: Save stones to persistentDataPath
    public void SaveStonesToPersistentCacheFile()
    {
        try
        {
            var stoneList = new List<StoneData>();
            foreach (var stone in Compass.meshesAndOutlines)
            {
                if (stone.mesh == null || stone.outline == null) continue;
                stoneList.Add(new StoneData { base64 = MeshSerializer.ToBase64(stone.mesh, stone.outline) });
            }

            string json = JsonUtility.ToJson(new Wrapper<StoneData> { items = stoneList });
            File.WriteAllText(PersistentStoneCachePath, json);
            Debug.Log($"TinySave: Saved {stoneList.Count} stones to file: {PersistentStoneCachePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"TinySave: Failed saving stones to file. {ex}");
        }
    }

    public bool LoadStonesFromPersistentCacheFile()
    {
        try
        {
            if (!File.Exists(PersistentStoneCachePath))
            {
                Debug.Log("TinySave: No persistent stone cache file found.");
                return false;
            }

            string json = File.ReadAllText(PersistentStoneCachePath);
            var wrapper = JsonUtility.FromJson<Wrapper<StoneData>>(json);
            if (wrapper?.items == null || wrapper.items.Count == 0)
            {
                Debug.LogWarning("TinySave: Persistent stone cache is empty.");
                return false;
            }

            Compass.meshesAndOutlines.Clear();
            foreach (var data in wrapper.items)
            {
                MeshSerializer.FromBase64(data.base64, out Mesh mesh, out List<Vector2> outline);
                if (mesh != null)
                {
                    Compass.meshesAndOutlines.Add(new Compass.MeshAndOutline { mesh = mesh, outline = outline });
                }
            }
            Debug.Log($"TinySave: Loaded {Compass.meshesAndOutlines.Count} stones from file cache.");
            return Compass.meshesAndOutlines.Count > 0;
        }
        catch (Exception ex)
        {
            Debug.LogError($"TinySave: Failed loading stones from file cache. {ex}");
            return false;
        }
    }

    public bool LoadStonesFromResources()
    {
        try
        {
            TextAsset ta = Resources.Load<TextAsset>(ResourcesStoneCacheNameNoExt);
            if (ta == null)
            {
                Debug.Log("TinySave: No Resources stone cache found (Resources/stonecache.json).");
                return false;
            }
            var wrapper = JsonUtility.FromJson<Wrapper<StoneData>>(ta.text);
            if (wrapper?.items == null || wrapper.items.Count == 0)
            {
                Debug.LogWarning("TinySave: Resources stone cache is empty.");
                return false;
            }

            Compass.meshesAndOutlines.Clear();
            foreach (var data in wrapper.items)
            {
                MeshSerializer.FromBase64(data.base64, out Mesh mesh, out List<Vector2> outline);
                if (mesh != null)
                {
                    Compass.meshesAndOutlines.Add(new Compass.MeshAndOutline { mesh = mesh, outline = outline });
                }
            }
            Debug.Log($"TinySave: Loaded {Compass.meshesAndOutlines.Count} stones from Resources cache.");
            return Compass.meshesAndOutlines.Count > 0;
        }
        catch (Exception ex)
        {
            Debug.LogError($"TinySave: Failed loading stones from Resources. {ex}");
            return false;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Save Stones -> Assets/Resources/stonecache.json")]
    public void SaveStonesToResourcesAsset()
    {
        try
        {
            var stoneList = new List<StoneData>();
            foreach (var stone in Compass.meshesAndOutlines)
            {
                if (stone.mesh == null || stone.outline == null) continue;
                stoneList.Add(new StoneData { base64 = MeshSerializer.ToBase64(stone.mesh, stone.outline) });
            }
            string json = JsonUtility.ToJson(new Wrapper<StoneData> { items = stoneList });

            string resourcesDir = Path.Combine(Application.dataPath, "Resources");
            if (!Directory.Exists(resourcesDir)) Directory.CreateDirectory(resourcesDir);
            string assetPath = Path.Combine(resourcesDir, StoneCacheFileName);
            File.WriteAllText(assetPath, json);

            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"TinySave: Saved {stoneList.Count} stones to Resources at {assetPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"TinySave: Failed saving stones to Resources asset. {ex}");
        }
    }

    [ContextMenu("Prefetch 50 Stones -> persistent + Resources")]
    private void Prefetch50ToBoth()
    {
        PrefetchAndCacheStones(50, true, null);
    }
#endif

    public void EnsureSharedStoneCache(int count = 50, System.Action<bool> onFinished = null)
    {
        bool hasCompassCache = Compass.meshesAndOutlines != null && Compass.meshesAndOutlines.Count > 0;
        if (hasCompassCache)
        {
            onFinished?.Invoke(true);
            return;
        }

        void FallbackToCache()
        {
            if (LoadStonesFromPersistentCacheFile() || LoadStonesFromResources())
            {
                onFinished?.Invoke(true);
            }
            else
            {
                onFinished?.Invoke(false);
            }
        }

        PrefetchAndCacheStones(count, false, success =>
        {
            if (success)
            {
                onFinished?.Invoke(true);
            }
            else
            {
                FallbackToCache();
            }
        });
    }

    public void PrefetchAndCacheStones(int count = 50, bool alsoSaveToResourcesInEditor = false, System.Action<bool> onFinished = null)
    {
        if (ConnectionManager.Instance == null)
        {
            Debug.LogWarning("TinySave: ConnectionManager not available; cannot prefetch stones.");
            onFinished?.Invoke(false);
            return;
        }

        ConnectionManager.Instance.FetchSharedVariables(
            "StoneComplete",
            (variables) =>
            {
                Compass.meshesAndOutlines.Clear();
                if (variables != null && variables.Length > 0)
                {
                    int added = 0;
                    foreach (var variable in variables)
                    {
                        try
                        {
                            Mesh stoneMesh = null;
                            List<Vector2> outline = new List<Vector2>();
                            MeshSerializer.FromBase64(variable.data, out stoneMesh, out outline);
                            if (stoneMesh != null)
                            {
                                Compass.meshesAndOutlines.Add(new Compass.MeshAndOutline { mesh = stoneMesh, outline = outline });
                                added++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"TinySave: Failed to decode a stone variable. {ex}");
                        }
                    }
                    Debug.Log($"TinySave: Prefetched {added} stones from server.");

                    SaveStonesToPersistentCacheFile();
#if UNITY_EDITOR
                    if (alsoSaveToResourcesInEditor)
                    {
                        SaveStonesToResourcesAsset();
                    }
#endif
                    onFinished?.Invoke(Compass.meshesAndOutlines.Count > 0);
                }
                else
                {
                    Debug.LogWarning("TinySave: Prefetch returned no stones.");
                    onFinished?.Invoke(false);
                }
            },
            count);
    }

    public void Save(bool andStones = false)
    {
        PlayerPrefs.SetInt(KEY_HAS_PLAYED, 1);

        if (andStones)
        {
            SaveTheStones();
        }
        SaveMessages();
        SaveEngineVariables();

        if (!string.IsNullOrEmpty(LastNodeSeen))
        {
            PlayerPrefs.SetString(KEY_LAST_NODE, LastNodeSeen);
        }

        PlayerPrefs.Save();
        Debug.Log("TinySave: Saved objects, stones, and variables.");
    }

   
    public void Load()
    {
        bool hasPlayed = PlayerPrefs.GetInt(KEY_HAS_PLAYED, 0) == 1;
        Debug.Log("Has played before? " + hasPlayed);

        LoadEngineVariables();
        LoadMessages();

        if (PlayerPrefs.HasKey(KEY_STONES))
        {
            Compass.meshesAndOutlines.Clear();

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

        if (PlayerPrefs.HasKey(KEY_LAST_NODE))
        {
            LastNodeSeen = PlayerPrefs.GetString(KEY_LAST_NODE);
        }

        LoadAllStoneData();

        if (PlayerPrefs.HasKey(KEY_OBJECTS) || PlayerPrefs.HasKey(KEY_STONES) || PlayerPrefs.HasKey(KEY_VARIABLES))
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
        set
        {
            PlayerPrefs.SetString(KEY_LAST_NODE, value);
            PlayerPrefs.Save();
        }
    }
        

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            SaveMessages();
           SaveEngineVariables();

            PlayerPrefs.Save();


        }

        if(Input.GetKeyDown(KeyCode.L))
        {

            LoadMessages();
            LoadEngineVariables();

        }


    }

    

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        Debug.Log(Application.persistentDataPath);

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
        {
            RefreshMainMenuUI();
        }

    }

    [Serializable]
    class Wrapper<T> { public List<T> items; }
}
