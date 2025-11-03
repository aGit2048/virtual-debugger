using DeBuggerCore.EventManagement;
using DebuggerWpf.DebugEventArg;
using DebuggerWpf.Models;
using DebuggerWpf.MqttUtility;
using FileUtility;
using System;

namespace DebuggerWpf.Components
{
    internal static class MqttMsgGenerator
    {
        public static string Generate(object sender, ControllerDataTableItem controllerDataTableItem)
        {
            if (controllerDataTableItem.SignalType == "io")
            {
                return GenerateIOMsg(sender, controllerDataTableItem);
            }
            else if (controllerDataTableItem.SignalType == "io")
            {
                return GenerateRobotMsg(sender, controllerDataTableItem);
            }
            return string.Empty;
        }

        private static string GenerateIOMsg(object sender, ControllerDataTableItem controllerDataTableItem)
        {
            IOMessage ioMsg = new IOMessage();
            ioMsg.Topic = controllerDataTableItem.Topic;
            ioMsg.Timestamp = System.DateTime.Now;
            ioMsg.MessageType = controllerDataTableItem.SignalType;

            Random random = new Random();
            int result = random.Next(2); // 生成 0 或 1
            ioMsg.Value = result == 1;

            string msgString = JsonHelper.Serialize(ioMsg);
            EventDispatcher.Instance.Fire(sender, new ConsoleLogEventArsg(controllerDataTableItem.ToString(), msgString, LogTyp.Info));
            return msgString;
        }

        private static string GenerateRobotMsg(object sender, ControllerDataTableItem controllerDataTableItem)
        {
            RobotJoints robotMsg = new RobotJoints();
            robotMsg.Topic = controllerDataTableItem.Topic;
            robotMsg.Timestamp = System.DateTime.Now;
            robotMsg.MessageType = controllerDataTableItem.SignalType;

            robotMsg.Value = new float[6];
            Random random = new Random();
            for (int i = 0; i < 6; i++)
            {
                float result = (float)random.NextDouble() * 360; // 生成 0.0 到 360.0 的浮点数
                robotMsg.Value[i] = result;
            }

            string msgString = JsonHelper.Serialize(robotMsg);
            EventDispatcher.Instance.Fire(sender, new ConsoleLogEventArsg(controllerDataTableItem.ToString(), msgString, LogTyp.Info));
            return msgString;
        }
    }
}
