using MQTTnet.Protocol;
using System;
using System.Threading.Tasks;

namespace MqttClientObject
{
    /// <summary>
    /// Mqtt客户端服务接口
    /// </summary>
    internal interface IMqttClientService : IDisposable
    {
        Task<bool> ConnectAsync();

        Task DisconnectAsync();

        Task<bool> PublishAsync(string topic, byte[] payload, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce);
       
        Task SubscribeAsync(string topic, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce);
       
        Task UnsubscribeAsync(string topic);
        
        event Func<MqttMessage, Task> MessageReceived;
        
        bool IsConnected { get; }
    }
}
