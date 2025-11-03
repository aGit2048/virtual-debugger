using DeBuggerCore.EventManagement;
using DebuggerWpf.Models;
using System;

namespace DebuggerWpf.DebugEventArg
{
    internal class ConsoleLogEventArsg : DebuggerEventArgs
    {
        public string ExcelItemInfo { get; private set; }
        public string MqttMessage { get; private set; }
        public DateTime DateTime { get; private set; }
        public LogTyp LogType { get; private set; }


        public ConsoleLogEventArsg(string excelItemInfo, string mqttMessage, LogTyp logType) 
        {
            ExcelItemInfo = excelItemInfo;
            MqttMessage = mqttMessage;
            DateTime = DateTime.Now;
            LogType = logType;
        }
    }
}
