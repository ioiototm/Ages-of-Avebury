using LoGaCulture.LUTE;
using LoGaCulture.LUTE.Logs;
using UnityEngine;



public class SendStoneToServer : MonoBehaviour
{


    public static void sendStoneToServer (GameObject stone)
    {

        ConnectionManager.Instance.SaveSharedVariable("stone1", "stone", "123,123,234,4444");


    }
  

   
}