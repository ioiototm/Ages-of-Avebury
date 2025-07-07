using UnityEngine;

public class KeepOne : MonoBehaviour
{
    static KeepOne instance;   // shared across scenes

    void Awake()
    {
        if (instance == null)
        {
            instance = this;               
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);          
        }
    }
}
