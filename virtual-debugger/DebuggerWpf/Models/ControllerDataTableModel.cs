using DebuggerWpf.AOD;
using DebuggerWpf.Components;
using System;
using System.Collections.Generic;

namespace DebuggerWpf.Models
{
    internal class ControllerDataTableModel : Notificator
    {
        private List<ControllerDataTableItem> _controllerDataTable;
        public List<ControllerDataTableItem> ControllerDataTables
        {
            get => _controllerDataTable;
            set => SetProperty(ref _controllerDataTable, value);
        }

        private string[] _tableHeader;
        public string[] TableHeader
        {
            get => _tableHeader;
            set => SetProperty(ref _tableHeader, value);
        }


        public ControllerDataTableModel()
        {
            ControllerDataTables = new List<ControllerDataTableItem>();
            LoadControllerDataTable.LoadExecel((excel) =>
            {
                if (!excel.Success)
                {
                    Console.WriteLine($"Topic数据表加载失败：{excel.Error}");
                    return;
                }
                for (int i = 0; i < excel.Data.Count; i++)
                {
                    ControllerDataTableItem item = new ControllerDataTableItem()
                    {
                        Id = excel.Data[i][0],
                        Topic = excel.Data[i][1],
                        ModelName = excel.Data[i][2],
                        IsActive = excel.Data[i][3],
                        SignalType = excel.Data[i][4],
                        DataType = excel.Data[i][5],
                        ScriptType = excel.Data[i][6]
                    };
                    ControllerDataTables.Add(item);
                }
                TableHeader = excel.ColumnNames;
            });
        }
        
    }

    internal class ControllerDataTableItem : Notificator
    {
        private string _id;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        private string _topic;
        public string Topic
        {
            get => _topic;
            set => SetProperty(ref _topic, value);
        }
        private string _modelName;
        public string ModelName
        {
            get => _modelName;
            set => SetProperty(ref _modelName, value);
        }
        private string _isActive;
        public string IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }
        private string _signalType;
        public string SignalType
        {
            get => _signalType;
            set => SetProperty(ref _signalType, value);
        }
        private string _dataType;
        public string DataType
        {
            get => _dataType;
            set => SetProperty(ref _dataType, value);
        }
        private string _scriptType;
        public string ScriptType
        {
            get => _scriptType;
            set => SetProperty(ref _scriptType, value);
        }

        public override string ToString()
        {
            return $"Id={_id}  Topic={_topic}  ModelName={_modelName}  IsActive={_isActive}  SignalType={_signalType}  DataType={_dataType}  ScriptType={_scriptType}";
        }
    }
}
