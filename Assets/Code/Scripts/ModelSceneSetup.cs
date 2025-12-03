using UnityEngine;

public class ModelSceneSetup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        //try to find object of type MapCompletion
        var mapCompletion = FindAnyObjectByType<MapCompletion>();

        if ( mapCompletion==null)
        {
            //spawn a new GameObject with MapCompletion component
            var mapCompletionObject = new GameObject("MapCompletion");
            mapCompletionObject.AddComponent<MapCompletion>();


        }
        else
        {
            //mapCompletion.SyncFromTinySave();
            //StartCoroutine(mapCompletion.wait10AndCreateStones(1));

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
