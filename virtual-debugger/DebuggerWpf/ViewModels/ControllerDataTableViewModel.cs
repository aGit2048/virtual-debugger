using DeBuggerCore.EventManagement;
using DebuggerWpf.Components;
using DebuggerWpf.DebugEventArg;
using DebuggerWpf.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace DebuggerWpf.ViewModels
{
    internal class ControllerDataTableViewModel : Notificator, IDisposable
    {
        public bool IsConsolePanelShowControllerInfo;

        private ControllerDataTableModel _model;
       
        /// <summary>
        /// Excel表中内容
        /// </summary>
        private List<ControllerDataTableItem> _controllerDataTable;
        public List<ControllerDataTableItem> ControllerDataTable
        {
            get => _controllerDataTable;
            set => SetProperty(ref _controllerDataTable, value);
        }
        /// <summary>
        /// 控制台Log列表
        /// </summary>
        private ObservableCollection<ConsoleLogItem> _consoleLogList;
        public ObservableCollection<ConsoleLogItem> ConsoleLogList
        {
            get => _consoleLogList;
            set => SetProperty(ref _consoleLogList, value);
        }


        public ControllerDataTableViewModel() 
        {
            _model = new ControllerDataTableModel();
            ControllerDataTable = _model.ControllerDataTables;
            ConsoleLogList = new ObservableCollection<ConsoleLogItem>();
            EventDispatcher.Instance.AddEvent(ConsoleLogEventArsg.ID, OnConsoleLogEventHandle);
        }

        private void OnConsoleLogEventHandle(object sender, System.EventArgs e)
        {
            ConsoleLogEventArsg args = (ConsoleLogEventArsg)(e);
            ConsoleLogItem item = new ConsoleLogItem();
            item.MqttMessge = args.MqttMessage;
            item.ExcelItemInfo = args.ExcelItemInfo;
            item.DateTime = args.DateTime;
            item.LogType = args.LogType;
            if (IsConsolePanelShowControllerInfo)
            {
                item.Content = $"Controller Info：{item.ExcelItemInfo}\nMqtt Message：{item.MqttMessge}";
            }
            else
            {
                item.Content = $"Mqtt Message：{item.MqttMessge}";
            }
            ConsoleLogList.Add(item);
        }

        public void ClearConsoleData()
        {
            if (ConsoleLogList.Count <= 0) return;
            ConsoleLogList.Clear();
        }
        public void ConsoleLogCheckControllerInfo(bool show)
        {
            if (IsConsolePanelShowControllerInfo == show) return;
            IsConsolePanelShowControllerInfo = show;
            if (IsConsolePanelShowControllerInfo)
            {
                foreach (var item in ConsoleLogList)
                {
                    item.Content = $"【Controller Info】{item.ExcelItemInfo}\n【Mqtt Message】{item.MqttMessge}";
                }
            }
            else
            {
                foreach (var item in ConsoleLogList)
                {
                    item.Content = $"【Mqtt Message】{item.MqttMessge}";
                }
            }
        }
        public void ConsoleLogCopy(ConsoleLogItem logItem, bool isAll)
        {
            if (isAll)
            {
                Clipboard.SetText($"【Controller Info】{logItem.ExcelItemInfo}\n【Mqtt Message】{logItem.MqttMessge}");
            }
            else
            {
                Clipboard.SetText($"【Mqtt Message】{logItem.MqttMessge}");
            }
        }

        public void Dispose()
        {
            EventDispatcher.Instance.RemoveEvent(ConsoleLogEventArsg.ID, OnConsoleLogEventHandle);
        }
    }
}
