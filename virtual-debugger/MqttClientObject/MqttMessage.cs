using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttClientObject
{
    /// <summary>
    /// Mqtt消息
    /// </summary>
    internal class MqttMessage
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string Topic { get; set; } = string.Empty;
        public byte[] Payload { get; set; } = Array.Empty<byte>();
        public MqttQualityOfServiceLevel QoS { get; set; }
        public bool Retain { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
