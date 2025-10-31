using MQTTnet.Client;
using System;

namespace MqttClientObject
{
    internal class PooledMqttConnection : IDisposable
    {
        private readonly MqttConnectionPool _pool;

        public IMqttClient Connection { get; }

        public PooledMqttConnection(IMqttClient connection, MqttConnectionPool pool)
        {
            Connection = connection;
            _pool = pool;
        }

        public void Dispose()
        {
            _pool.ReturnConnection(Connection);
        }
    }
}
