using System;
using System.Diagnostics.Metrics;

namespace MqttClientObject
{
    /// <summary>
    /// MQTT性能监控
    /// </summary>
    internal class MqttPerformanceMonitor : IDisposable
    {
        private readonly Meter _meter;
        private readonly Counter<long> _messagesPublished;
        private readonly Counter<long> _messagesReceived;
        private readonly Histogram<double> _publishLatency;
        private readonly Histogram<double> _processLatency;

        public MqttPerformanceMonitor()
        {
            _meter = new Meter("MQTT.Client", "1.0.0");

            _messagesPublished = _meter.CreateCounter<long>(
                "mqtt.messages.published",
                "messages",
                "Number of published messages");

            _messagesReceived = _meter.CreateCounter<long>(
                "mqtt.messages.received",
                "messages",
                "Number of received messages");

            _publishLatency = _meter.CreateHistogram<double>(
                "mqtt.publish.latency",
                "ms",
                "Message publish latency");

            _processLatency = _meter.CreateHistogram<double>(
                "mqtt.process.latency",
                "ms",
                "Message processing latency");
        }

        public void RecordMessagePublished(TimeSpan latency)
        {
            _messagesPublished.Add(1);
            _publishLatency.Record(latency.TotalMilliseconds);
        }

        public void RecordMessageReceived(TimeSpan latency)
        {
            _messagesReceived.Add(1);
            _processLatency.Record(latency.TotalMilliseconds);
        }

        public void Dispose()
        {
            _meter?.Dispose();
        }
    }
}
