using MQTTnet.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttClientObject
{
    /// <summary>
    /// 客户端连接池
    /// </summary>
    internal class MqttConnectionPool : IDisposable
    {
        private readonly ConcurrentBag<IMqttClient> _connections;
        private readonly Func<Task<IMqttClient>> _connectionFactory;
        private readonly int _maxPoolSize;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed = false;

        public MqttConnectionPool(int maxPoolSize, Func<Task<IMqttClient>> connectionFactory)
        {
            _connections = new ConcurrentBag<IMqttClient>();
            _connectionFactory = connectionFactory;
            _maxPoolSize = maxPoolSize;
            _semaphore = new SemaphoreSlim(maxPoolSize, maxPoolSize);
        }

        public async Task<PooledMqttConnection> GetConnectionAsync(TimeSpan timeout = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MqttConnectionPool));

            if (timeout == default) timeout = TimeSpan.FromSeconds(30);

            if (!await _semaphore.WaitAsync(timeout))
            {
                throw new TimeoutException("获取MQTT连接超时");
            }

            try
            {
                if (_connections.TryTake(out var connection) && connection.IsConnected)
                {
                    return new PooledMqttConnection(connection, this);
                }

                // 创建新连接
                var newConnection = await _connectionFactory();
                return new PooledMqttConnection(newConnection, this);
            }
            catch
            {
                _semaphore.Release();
                throw;
            }
        }

        internal void ReturnConnection(IMqttClient connection)
        {
            if (_disposed || !connection.IsConnected)
            {
                connection?.DisconnectAsync()?.ConfigureAwait(false);
                connection?.Dispose();
                _semaphore.Release();
                return;
            }

            if (_connections.Count < _maxPoolSize)
            {
                _connections.Add(connection);
            }
            else
            {
                connection.Dispose();
            }
            _semaphore.Release();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            while (_connections.TryTake(out var connection))
            {
                try
                {
                    if (connection.IsConnected)
                    {
                        connection.DisconnectAsync()?.ConfigureAwait(false);
                    }
                    connection.Dispose();
                }
                catch
                {
                    // 忽略清理时的异常
                }
            }
            _semaphore?.Dispose();
        }

    }
}
