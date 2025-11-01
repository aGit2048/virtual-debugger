using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace MqttClientObject
{
    /// <summary>
    /// MQTT客户端
    /// </summary>
    internal class MqttClientService : IMqttClientService
    {
        /// <summary>
        /// 持久连接
        /// </summary>
        private IMqttClient _persistentConnection;
        /// <summary>
        /// 消息池
        /// </summary>
        private readonly MqttMessagePool _messagePool;
        /// <summary>
        /// 消息处理器
        /// </summary>
        private readonly MessageHandler _messageHandler;
        /// <summary>
        /// MQTT性能监控
        /// </summary>
        private readonly MqttPerformanceMonitor _performanceMonitor;
        /// <summary>
        /// 连接参数
        /// </summary>
        private readonly MqttConnectOptions _connectOptions;
        /// <summary>
        /// 推送锁
        /// </summary>
        private readonly AsyncLock _publishLock = new AsyncLock();
        /// <summary>
        /// 重连锁
        /// </summary>
        private readonly AsyncLock _reconnectLock = new AsyncLock();
        /// <summary>
        /// 连接锁
        /// </summary>
        private readonly AsyncLock _connectLock = new AsyncLock();
        /// <summary>
        /// 是否已经销毁
        /// </summary>
        private bool _disposed = false;
        /// <summary>
        /// 是否已经连接
        /// </summary>
        private bool _isConnected = false;
        /// <summary>
        /// 是否在连接中
        /// </summary>
        private bool _isReconnecting = false;
        /// <summary>
        /// 尝试重连次数
        /// </summary>
        private int _reconnectAttempts = 0;

        /// <summary>
        /// 接收消息事件
        /// </summary>
        public event Func<MqttMessage, Task> MessageReceived;

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool IsConnected => _isConnected && !_disposed;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="clientOptions">连接参数</param>
        public MqttClientService(MqttConnectOptions clientOptions)
        {
            _connectOptions = clientOptions;
            _messagePool = new MqttMessagePool();
            _performanceMonitor = new MqttPerformanceMonitor();

            // 初始化消息处理器
            _messageHandler = new MessageHandler(
                async message => await OnMessageReceivedInternal(message),
                bufferCapacity: 10000,
                maxParallelism: Environment.ProcessorCount * 2);

            //// 初始化连接池
            //_connectionPool = new MqttConnectionPool(
            //    maxPoolSize: 10,
            //    connectionFactory: CreateMqttClientAsync);
        }

        /// <summary>
        /// 异步建立连接
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task<bool> ConnectAsync()
        {
            if (_disposed) 
                throw new ObjectDisposedException(nameof(MqttClientService));

            using (await _connectLock.LockAsync())
            {
                if (IsConnected)
                {
                    Console.WriteLine("MQTT客户端已连接，无需重复连接");
                    return true;
                }

                try
                {
                    // 清理现有连接
                    if (_persistentConnection != null)
                    {
                        _persistentConnection.ConnectedAsync -= OnConnectedAsync;
                        _persistentConnection.DisconnectedAsync -= OnDisconnectedAsync;
                        _persistentConnection.ApplicationMessageReceivedAsync -= OnApplicationMessageReceivedAsync;
                        _persistentConnection.Dispose();
                        _persistentConnection = null;
                    }

                    // 创建新连接
                    _persistentConnection = await CreateMqttClientAsync();

                    if (_persistentConnection.IsConnected)
                    {
                        _isConnected = true;
                        Console.WriteLine("MQTT连接建立成功");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("MQTT连接建立失败");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"MQTT连接失败: {ex.Message}");
                    _isConnected = false;
                    return false;
                }

            }
        }

        /// <summary>
        /// 异步断开连接
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (_disposed || _persistentConnection == null)
                return;

            // 使用连接锁确保线程安全
            using (await _connectLock.LockAsync())
            {
                if (_isConnected && _persistentConnection.IsConnected)
                {
                    try
                    {
                        await _persistentConnection.DisconnectAsync();
                        Console.WriteLine("MQTT连接已断开");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"断开连接时发生异常: {ex.Message}");
                    }
                }

                _isConnected = false;
            }

            //// 异步断开连接
            //if (_isConnected)
            //{
            //    try
            //    {
            //        using (PooledMqttConnection connection = await _connectionPool.GetConnectionAsync())
            //        {
            //            await connection.Connection.DisconnectAsync();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"An exception occurred when the connection was disconnected: {ex.Message}");
            //    }
            //}
        }

        /// <summary>
        /// 异步：发布消息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="payload">消息的实际内容，二进制数据</param>
        /// <param name="qos">服务质量等级</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<bool> PublishAsync(string topic, byte[] payload, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce)
        {
            if (_disposed) 
                throw new ObjectDisposedException(nameof(MqttClientService));

            if (_persistentConnection == null || !_persistentConnection.IsConnected)
                throw new InvalidOperationException("MQTT客户端未连接");

            // 使用异步锁确保线程安全
            using (await _publishLock.LockAsync())
            {
                MqttApplicationMessage message = _messagePool.Rent();

                try
                {
                    // 高精度计时：用于计算发布消息所消耗的时间
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    message.Topic = topic;
                    message.PayloadSegment = new ArraySegment<byte>(payload);
                    message.QualityOfServiceLevel = qos;

                    MqttClientPublishResult result = await _persistentConnection.PublishAsync(message);

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

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task SubscribeAsync(string topic, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce)
        {
            if (_disposed) 
                throw new ObjectDisposedException(nameof(MqttClientService));

            if (_persistentConnection == null || !_persistentConnection.IsConnected)
                throw new InvalidOperationException("MQTT客户端未连接");

            MqttClientSubscribeOptions subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic, qos)
                .Build();

            //在result中实际上包含了多个订阅项的结果
            MqttClientSubscribeResult result = await _persistentConnection.SubscribeAsync(subscribeOptions);

            /* 订阅结果Code
             * 
             * // 成功的订阅结果（0-2）
             * GrantedQoS0 = 0x00,  // 订阅成功，最大QoS为0
             * GrantedQoS1 = 0x01,  // 订阅成功，最大QoS为1  
             * GrantedQoS2 = 0x02,  // 订阅成功，最大QoS为2
             * 
             * // 失败的订阅结果（> 2）
             * UnspecifiedError = 0x80,        // 未指定错误
             * ImplementationSpecificError = 0x83, // 实现特定错误
             * NotAuthorized = 0x87,           // 未授权
             * TopicFilterInvalid = 0x8F,      // 主题过滤器无效
             * PacketIdentifierInUse = 0x91,   // 包标识符正在使用
             * QuotaExceeded = 0x97,           // 配额超出
             * SharedSubscriptionsNotSupported = 0x9E, // 不支持共享订阅
             * SubscriptionIdentifiersNotSupported = 0xA1, // 不支持订阅标识符
             * WildcardSubscriptionsNotSupported = 0xA2, // 不支持通配符订阅
             *
             */
            if (result.Items.Any(item => item.ResultCode > MqttClientSubscribeResultCode.GrantedQoS2))
            {
                throw new Exception($"Subscription failed: {result.Items.First().ResultCode}");
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task UnsubscribeAsync(string topic)
        {
            if (_disposed) 
                throw new ObjectDisposedException(nameof(MqttClientService));

            if (_persistentConnection == null || !_persistentConnection.IsConnected)
                throw new InvalidOperationException("MQTT客户端未连接");

            MqttClientUnsubscribeOptions unsubscribeOptions = new MqttClientUnsubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build();
            await _persistentConnection.UnsubscribeAsync(unsubscribeOptions);
        }

        /// <summary>
        /// 异步创建MQTT客户端，并发起连接，绑定事件处理器
        /// </summary>
        /// <returns></returns>
        private async Task<IMqttClient> CreateMqttClientAsync()
        {
            MqttFactory factory = new MqttFactory();
            IMqttClient client = factory.CreateMqttClient();

            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder()
                .WithTcpServer(_connectOptions.BrokerAddress, _connectOptions.Port)
                .WithClientId(_connectOptions.ClientId)
                .WithCredentials(_connectOptions.Username, _connectOptions.Password)
                .WithCleanSession(_connectOptions.CleanSession)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(_connectOptions.KeepAliveInterval))
                .WithTimeout(TimeSpan.FromSeconds(_connectOptions.CommunicationTimeout));

            if (_connectOptions.UseTls)
            {
                builder = builder.WithTlsOptions(tlsOptions =>
                {
                    tlsOptions.UseTls();
                    if (_connectOptions.IgnoreCertificateErrors)
                    {
                        tlsOptions.WithAllowUntrustedCertificates();
                    }
                });
            }

            MqttClientOptions options = builder.Build();

            // 绑定事件处理器
            client.ConnectedAsync += OnConnectedAsync;
            client.DisconnectedAsync += OnDisconnectedAsync;
            client.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;

            MqttClientConnectResult result = await client.ConnectAsync(options);
            _isConnected = result.ResultCode == MqttClientConnectResultCode.Success;
            return client;
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            _isConnected = true;
            _reconnectAttempts = 0;
            Console.WriteLine($"The MQTT connection has been established: {e.ConnectResult.ResultCode}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 连接断开
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            _isConnected = false;
            Console.WriteLine($"The MQTT connection is disconnected: {e.Reason}");

            // 手动实现自动重连逻辑
            if (_connectOptions.AutoReconnect && !_disposed &&
                e.Reason != MqttClientDisconnectReason.NormalDisconnection)
            {
                _ = Task.Run(async () => await TryReconnectAsync()); // 异步执行重连，不阻塞
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 尝试重连
        /// </summary>
        /// <returns></returns>
        private async Task TryReconnectAsync()
        {
            // 使用锁防止多个重连任务同时运行
            using (await _reconnectLock.LockAsync())
            {
                if (_isReconnecting || _disposed) return;
                _isReconnecting = true;
            }

            try
            {
                Console.WriteLine("🔄 开始自动重连...");

                for (int attempt = 1; attempt <= _connectOptions.MaxReconnectAttempts; attempt++)
                {
                    if (_disposed) break;

                    _reconnectAttempts = attempt;
                    Console.WriteLine($"🔄 第 {attempt} 次重连尝试...");

                    try
                    {
                        bool success = await ConnectAsync();
                        if (success)
                        {
                            _isConnected = true;
                            _reconnectAttempts = 0;
                            Console.WriteLine($"✅ 第 {attempt} 次重连成功");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ 第 {attempt} 次重连失败: {ex.Message}");
                    }


                    if (attempt < _connectOptions.MaxReconnectAttempts)
                    {
                        // 指数退避延迟
                        var delay = TimeSpan.FromSeconds(_connectOptions.ReconnectDelay * Math.Pow(2, attempt - 1));
                        Console.WriteLine($"⏳ 等待 {delay.TotalSeconds} 秒后再次尝试...");
                        await Task.Delay(delay);
                    }
                }

                Console.WriteLine("💥 自动重连失败，已达到最大重连次数");
            }
            finally
            {
                _isReconnecting = false;
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="e">接收消息事件参数</param>
        /// <returns></returns>
        private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            MqttMessage message = new MqttMessage
            {
                Topic = e.ApplicationMessage.Topic,
                Payload = e.ApplicationMessage.PayloadSegment.ToArray(),
                QoS = e.ApplicationMessage.QualityOfServiceLevel,
                Retain = e.ApplicationMessage.Retain,
                Timestamp = DateTime.UtcNow
            };
            // 将消息加入处理队列
            await _messageHandler.EnqueueMessageAsync(message);
        }

        /// <summary>
        /// 内部消息处理
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        private async Task OnMessageReceivedInternal(MqttMessage message)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                if (MessageReceived != null)
                {
                    // 触发用户注册的消息处理事件
                    await MessageReceived.Invoke(message);
                }
            }
            finally
            {
                stopwatch.Stop();
                _performanceMonitor.RecordMessageReceived(stopwatch.Elapsed);
            }
        }
       
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // 断开连接
            if (_persistentConnection != null)
            {
                if (_persistentConnection.IsConnected)
                {
                    _persistentConnection.DisconnectAsync()?.ConfigureAwait(false);
                }

                _persistentConnection.ConnectedAsync -= OnConnectedAsync;
                _persistentConnection.DisconnectedAsync -= OnDisconnectedAsync;
                _persistentConnection.ApplicationMessageReceivedAsync -= OnApplicationMessageReceivedAsync;

                _persistentConnection.Dispose();
                _persistentConnection = null;
            }

            _messageHandler?.Dispose();
            _performanceMonitor?.Dispose();
            _publishLock?.Dispose();
            _connectLock?.Dispose();
            _reconnectLock?.Dispose();
        }

    }
}
