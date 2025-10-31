using Microsoft.Extensions.ObjectPool;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttClientObject
{
    internal class MqttMessagePool
    {
        private readonly ObjectPool<MqttApplicationMessage> _messagePool;

        public MqttMessagePool()
        {
            var policy = new DefaultPooledObjectPolicy<MqttApplicationMessage>();
            _messagePool = new DefaultObjectPool<MqttApplicationMessage>(policy, 1000);
        }

        public MqttApplicationMessage Rent()
        {
            return _messagePool.Get();
        }

        public void Return(MqttApplicationMessage message)
        {
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
