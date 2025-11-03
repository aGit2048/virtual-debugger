using System;

namespace DebuggerWpf.MqttUtility
{
    public class MQTTBaseMessage
    {
        // 协议规定的消息类型标识MessageType
        public int MessageCode;
        public string MessageType;
        public DateTime Timestamp { get; set; }

        public string Topic;

        //以下是为了兼容之前的消息播放未删除的
        public Guid Guid { get; set; }
        public bool IsPlayback { get; set; }
        public DateTime StartDt { get; set; }
        public DateTime EndDt { get; set; }
        public string Type { get; set; }
    }

    public class TemperatureMessage : MQTTBaseMessage
    {
        public float value;
        public string unit = "°C";
    }


    [Serializable]
    public class RobotJoints : MQTTBaseMessage
    {
        public float[] Value;
    }

    public class RobotJoint : MQTTBaseMessage
    {
        public float Value;
    }

    public class IOMessage : MQTTBaseMessage
    {
        public bool Value;
    }
}
