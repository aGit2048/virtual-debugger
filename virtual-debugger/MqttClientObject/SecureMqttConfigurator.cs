using MQTTnet.Client;
using System;
using System.Net.Security;

namespace MqttClientObject
{
    public static class SecureMqttConfigurator
    {
        public static MqttClientOptionsBuilder ConfigureSecurity(this MqttClientOptionsBuilder builder, MqttClientOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            // 使用传入的 builder 参数
            var resultBuilder = builder
                .WithTcpServer(options.BrokerAddress, options.Port)
                .WithClientId(options.ClientId)
                .WithCredentials(options.Username, options.Password)
                .WithCleanSession(options.CleanSession)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(options.KeepAliveInterval))
                .WithTimeout(TimeSpan.FromSeconds(options.CommunicationTimeout));

            if (options.UseTls)
            {
                resultBuilder = resultBuilder.WithTlsOptions(tlsOptions =>
                {
                    // 使用新的 API - UseTls 现在是方法
                    tlsOptions.UseTls();

                    // 证书验证回调
                    tlsOptions.WithCertificateValidationHandler(context =>
                    {
                        // 生产环境中应实现严格的证书验证
                        if (options.IgnoreCertificateErrors)
                            return true;

                        // 基本验证：检查证书错误
                        return context.SslPolicyErrors == SslPolicyErrors.None;
                    });

                    // 其他 TLS 配置
                    if (options.IgnoreCertificateErrors)
                    {
                        tlsOptions.WithAllowUntrustedCertificates();
                    }
                });
            }

            return resultBuilder;
        }

        public static MqttClientOptionsBuilder ConfigureReconnection(this MqttClientOptionsBuilder builder)
        {
            // 新的重连配置 API
            return builder;
        }
    }
}
