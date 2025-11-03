using DebuggerWpf.Components;
using System;
using System.Collections.Generic;

namespace DebuggerWpf.Models
{
    public enum LogTyp
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    internal class ConsoleLogItem : Notificator
    {
        private LogTyp _logType;
        public LogTyp LogType
        {
            get => _logType;
            set => SetProperty(ref _logType, value);
        }

        private string _excelItemInfo;
        public string ExcelItemInfo
        {
            get => _excelItemInfo;
            set => SetProperty(ref _excelItemInfo, value);
        }

        private string _mqttMessge;
        public string MqttMessge
        {
            get => _mqttMessge;
            set => SetProperty(ref _mqttMessge, value);
        }

        private string _content;
        public string Content 
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        private DateTime _dateTime;
        public DateTime DateTime
        {
            get => _dateTime;
            set => SetProperty(ref _dateTime, value);
        }
    }
}
