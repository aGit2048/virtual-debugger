using Microsoft.Extensions.ObjectPool;
using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Threading;

namespace MqttClientObject
{
    /// <summary>
    /// 消息池：减少内存分配和垃圾回收压力
    /// </summary>
    internal class MqttMessagePool
    {
        private readonly ObjectPool<MqttApplicationMessage> _messagePool;
        private bool _disposed = false;

        public MqttMessagePool()
        {
            var policy = new DefaultPooledObjectPolicy<MqttApplicationMessage>();
            _messagePool = new DefaultObjectPool<MqttApplicationMessage>(policy, 1000);
        }

        public MqttApplicationMessage Rent()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MqttMessagePool));
            return _messagePool.Get();
        }

        [Obsolete]
        public void Return(MqttApplicationMessage message)
        {
            if (_disposed || message == null)
                return;

            if (message != null)
            {
                // 重置消息状态以便重用
                message.Payload = Array.Empty<byte>();
                message.ResponseTopic = null;
                message.ContentType = null;
                message.UserProperties = null;
                _messagePool.Return(message);
            }
        }
    }
}
