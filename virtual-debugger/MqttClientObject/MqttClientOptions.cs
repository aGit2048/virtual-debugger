using System;

namespace MqttClientObject
{
    public class MqttClientOptions
    {
        public string BrokerAddress { get; set; } = "localhost";
        public int Port { get; set; } = 1883;
        public string ClientId { get; set; } = $"client_{Guid.NewGuid()}";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseTls { get; set; } = false;
        public bool IgnoreCertificateErrors { get; set; } = false;
        public bool CleanSession { get; set; } = true;
        public int KeepAliveInterval { get; set; } = 30;
        public int CommunicationTimeout { get; set; } = 10;
    }
}
