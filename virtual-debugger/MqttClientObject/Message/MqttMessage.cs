using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MqttClientObject
{
    /// <summary>
    /// Mqtt消息
    /// </summary>
    public class MqttMessage
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string Topic { get; set; } = string.Empty;
        /// <summary>
        /// 实际发送的二进制内容
        /// </summary>
        public byte[] Payload { get; set; } = Array.Empty<byte>();
        /// <summary>
        /// 服务质量等级
        /// </summary>
        public MqttQualityOfServiceLevel QoS { get; set; }
        /// <summary>
        /// 保留消息标志: 控制服务器是否保留此消息
        /// true: 服务器保留此消息，新订阅者立即收到
        /// false: 服务器不保留，只发送给当前订阅者
        /// </summary>
        public bool Retain { get; set; }
        /// <summary>
        /// 记录消息的创建或接收时间
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 获取文本内容
        /// </summary>
        /// <returns></returns>
        public string GetPayloadAsString()
        {
            return Encoding.UTF8.GetString(Payload);
        }

        /// <summary>
        /// 设置文本内容
        /// </summary>
        /// <param name="text"></param>
        public void SetPayloadAsString(string text)
        {
            Payload = Encoding.UTF8.GetBytes(text);
        }

        /// <summary>
        /// 获取 JSON 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetPayloadAsJson<T>()
        {
            return JsonSerializer.Deserialize<T>(GetPayloadAsString());
        }

        /// <summary>
        /// 设置 JSON 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public void SetPayloadAsJson<T>(T obj)
        {
            SetPayloadAsString(JsonSerializer.Serialize(obj));
        }
    }
}
