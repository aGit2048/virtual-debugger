using System;

namespace MqttClientObject
{
    /// <summary>
    /// 客户端连接配置
    /// </summary>
    public class MqttConnectOptions
    {
        /// <summary>
        /// 代理服务器地址
        /// </summary>
        public string BrokerAddress { get; set; } = "127.0.0.1";
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; } = 1883;
        /// <summary>
        /// 客户端唯一标识符
        /// </summary>
        public string ClientId { get; set; } = $"client_{Guid.NewGuid()}";
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// 是否启用 TLS/SSL 加密通信（若启用加密，通常需要配合正常的端口8883）
        /// </summary>
        public bool UseTls { get; set; } = false;
        /// <summary>
        /// 是否忽略 SSL 证书验证错误
        /// </summary>
        public bool IgnoreCertificateErrors { get; set; } = false;
        /// <summary>
        /// 是否清理会话，控制会话状态的持久化
        /// </summary>
        public bool CleanSession { get; set; } = true;
        /// <summary>
        /// 客户端发送保活包的时间间隔（秒）,设置为0表示禁用保活机制
        /// </summary>
        public int KeepAliveInterval { get; set; } = 30;
        /// <summary>
        /// 通信超时
        /// </summary>
        public int CommunicationTimeout { get; set; } = 10;

        /// <summary>
        /// 自动重连
        /// </summary>
        public bool AutoReconnect { get; set; } = true;
        /// <summary>
        /// 重连延迟(秒)
        /// </summary>
        public int ReconnectDelay { get; set; } = 5;
        /// <summary>
        /// 最大重连次数
        /// </summary>
        public int MaxReconnectAttempts { get; set; } = 10;        


        /// <summary>
        /// 开发环境下的基础配置
        /// </summary>
        public static MqttConnectOptions DevelopmentEnvOptions { get; set; } = new MqttConnectOptions
        {
            UseTls = false,
            CleanSession = true,
            KeepAliveInterval = 30
        };
        /// <summary>
        /// 生产环境下的基础配置
        /// </summary>
        public static MqttConnectOptions ProductionEnvoptions = new MqttConnectOptions
        {
            UseTls = true,
            IgnoreCertificateErrors = false,  // 生产环境严格验证
            CleanSession = false,  // 保留会话状态
            KeepAliveInterval = 60,
            CommunicationTimeout = 15
        };
    }
}
