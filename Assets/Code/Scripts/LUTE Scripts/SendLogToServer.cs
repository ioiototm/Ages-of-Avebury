using LoGaCulture.LUTE;
using LoGaCulture.LUTE.Logs;
using UnityEngine;

[OrderInfo("AgesOfAvebury",
           "Send Log To Server",
           "Sends a single log line to the server with the chosen level and message.")]
[AddComponentMenu("")]
public class SendLogToServer : Order
{
    [SerializeField] private LogLevel level = LogLevel.Info;

    [SerializeField, TextArea]
    private string message = "";



    public override void OnEnter()
    {
        // Replace tokens
        string finalMessage = message.Replace("{appversion}", Application.version);

        // Send to server
        LogaManager.Instance.LogManager.Log(level, finalMessage);

        Continue();
    }

    public override string GetSummary()
    {
        return $"Level: {level} | Message: \"{message}\"";
    }
}
