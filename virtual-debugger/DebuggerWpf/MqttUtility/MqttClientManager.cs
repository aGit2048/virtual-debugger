using MqttClientObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerWpf.MqttUtility
{
    internal static class MqttClientManager
    {
        private static bool _isInitialized;
        private static MqttClientService _client;

        public static void Initialize()
        {

            try
            {
                MqttConnectOptions.DevelopmentEnvOptions.BrokerAddress = "127.0.0.1";
                MqttConnectOptions.DevelopmentEnvOptions.Port = 1883;
                MqttConnectOptions.DevelopmentEnvOptions.AutoReconnect = true;

                _client = new MqttClientService(MqttConnectOptions.DevelopmentEnvOptions);
                _isInitialized = true;
            }
            catch(Exception ex) 
            {
                throw new Exception($"Mqtt客户端服务创建失败！ex={ex.ToString()}");
            }
            
        }

        public static async Task ConnectAsync()
        {
            if (_isInitialized == false)
                Initialize();

            try
            {
                await _client.ConnectAsync();
            }
            catch (Exception ex) 
            {
                throw new Exception($"Mqtt客户端连接连接失败! ex={ex.ToString()}");
            }
        }

        public static async Task PushMsgAsync(string topic, string msg)
        {
            byte[] payload = Encoding.UTF8.GetBytes(msg);
            try
            {
                await _client.PublishAsync(topic, payload);
            }
            catch (Exception ex)
            {
                throw new Exception($"Mqtt发送消息失败! ex={ex.ToString()}");
            }
        }
    }
}
