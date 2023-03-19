using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogCanvas : MonoBehaviour
{

    private List<LogMessage> logList = new List<LogMessage>();

    [SerializeField] private TextMeshProUGUI output;

    private void Awake() {
        Application.logMessageReceived += Application_logMessageReceived;
        logList.Capacity = 10;
    }

    private void OnDestroy() {
        Application.logMessageReceived -= Application_logMessageReceived;
    }

    private void Application_logMessageReceived(string condition, string stackTrace, LogType type) {

        if (logList.Capacity == logList.Count) {
            logList.RemoveAt(0);
        }

        logList.Add(new LogMessage { condition = condition, type = type });

        UpdateLogText();
    }

    private void UpdateLogText() {
        output.text = "";

        output.text = string.Join("\n", logList.ConvertAll(p => p.condition));
    }


    private struct LogMessage {
        public string condition;
        //public string stackTrace;
        public LogType type;
    }
}
