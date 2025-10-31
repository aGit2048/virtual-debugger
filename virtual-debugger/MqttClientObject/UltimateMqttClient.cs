using MQTTnet.Client;
using MQTTnet.Internal;
using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MqttClientObject
{
    internal class UltimateMqttClient : IMqttClientService
    {
        private readonly MqttConnectionPool _connectionPool;
        private readonly MqttMessagePool _messagePool;
        private readonly MessageHandler _messageHandler;
        private readonly MqttPerformanceMonitor _performanceMonitor;
        private readonly MqttClientOptions _clientOptions;
        private readonly AsyncLock _publishLock = new AsyncLock();
        private bool _disposed = false;
        private bool _isConnected = false;

        public event Func<MqttMessage, Task> MessageReceived;
        public bool IsConnected => _isConnected && !_disposed;

        public UltimateMqttClient(MqttClientOptions clientOptions)
        {
            _clientOptions = clientOptions;
            _messagePool = new MqttMessagePool();
            _performanceMonitor = new MqttPerformanceMonitor();

            // 初始化消息处理器
            _messageHandler = new MessageHandler(
                async message => await OnMessageReceivedInternal(message),
                bufferCapacity: 10000,
                maxParallelism: Environment.ProcessorCount * 2);

            // 初始化连接池
            _connectionPool = new MqttConnectionPool(
                maxPoolSize: 10,
                connectionFactory: CreateMqttClientAsync);
        }

        private async Task<IMqttClient> CreateMqttClientAsync()
        {
            var factory = new MqttFactory();
            var client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_clientOptions.BrokerAddress, _clientOptions.Port)
                .WithClientId(_clientOptions.ClientId)
                .WithCredentials(_clientOptions.Username, _clientOptions.Password)
                .WithCleanSession(_clientOptions.CleanSession)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(_clientOptions.KeepAliveInterval))
                .WithTimeout(TimeSpan.FromSeconds(_clientOptions.CommunicationTimeout))
                .Build();

            // 设置事件处理器
            client.ConnectedAsync += OnConnectedAsync;
            client.DisconnectedAsync += OnDisconnectedAsync;
            client.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;

            var result = await client.ConnectAsync(options);
            _isConnected = result.ResultCode == MqttClientConnectResultCode.Success;

            return client;
        }

        private Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            _isConnected = true;
            Console.WriteLine($"MQTT连接已建立: {e.ConnectResult.ResultCode}");
            return Task.CompletedTask;
        }

        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            _isConnected = false;
            Console.WriteLine($"MQTT连接断开: {e.Reason}");
            return Task.CompletedTask;
        }

        private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var message = new MqttMessage
            {
                Topic = e.ApplicationMessage.Topic,
                Payload = e.ApplicationMessage.PayloadSegment.ToArray(),
                QoS = e.ApplicationMessage.QualityOfServiceLevel,
                Retain = e.ApplicationMessage.Retain,
                Timestamp = DateTime.UtcNow
            };

            await _messageHandler.EnqueueMessageAsync(message);
        }

        private async Task OnMessageReceivedInternal(MqttMessage message)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (MessageReceived != null)
                {
                    await MessageReceived.Invoke(message);
                }
            }
            finally
            {
                stopwatch.Stop();
                _performanceMonitor.RecordMessageReceived(stopwatch.Elapsed);
            }
        }

        public async Task<bool> ConnectAsync()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(UltimateMqttClient));

            try
            {
                // 测试连接
                using (var connection = await _connectionPool.GetConnectionAsync())
                {
                    return connection.Connection.IsConnected;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"连接失败: {ex.Message}");
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            Dispose();
        }

        public async Task<bool> PublishAsync(string topic, byte[] payload, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(UltimateMqttClient));

            using (await _publishLock.LockAsync())
            {
                using (var connection = await _connectionPool.GetConnectionAsync())
                {
                    if (!connection.Connection.IsConnected)
                    {
                        throw new InvalidOperationException("MQTT客户端未连接");
                    }

                    var message = _messagePool.Rent();

                    try
                    {
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        message.Topic = topic;
                        message.Payload = payload;
                        message.QualityOfServiceLevel = qos;

                        var result = await connection.Connection.PublishAsync(message);

                        stopwatch.Stop();
                        _performanceMonitor.RecordMessagePublished(stopwatch.Elapsed);

                        return result.ReasonCode == MqttClientPublishReasonCode.Success;
                    }
                    finally
                    {
                        _messagePool.Return(message);
                    }
                }

                
            }
        }

        public async Task SubscribeAsync(string topic, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(UltimateMqttClient));

            using (var connection = await _connectionPool.GetConnectionAsync())
            {
                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic, qos)
                .Build();

                var result = await connection.Connection.SubscribeAsync(subscribeOptions);

                if (result.Items.Any(item => item.ResultCode > MqttClientSubscribeResultCode.GrantedQoS2))
                {
                    throw new Exception($"订阅失败: {result.Items.First().ResultCode}");
                }
            }
        }

        public async Task UnsubscribeAsync(string topic)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(UltimateMqttClient));

            using (var connection = await _connectionPool.GetConnectionAsync())
            {
                var unsubscribeOptions = new MqttClientUnsubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build();

                await connection.Connection.UnsubscribeAsync(unsubscribeOptions);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _messageHandler?.Dispose();
            _connectionPool?.Dispose();
            _performanceMonitor?.Dispose();
            _publishLock?.Dispose();
        }
    }
}
