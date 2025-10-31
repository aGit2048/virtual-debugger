using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttClientObject
{
    internal class MqttMessageActivity
    {
        public static ActivitySource ActivitySource { get; } = new ActivitySource("MQTT.Client");

        public static async Task ProcessWithTracingAsync(MqttMessage message, Func<MqttMessage, Task> processor)
        {
            using (var activity = ActivitySource.StartActivity("mqtt.process.message"))
            {
                if (activity != null)
                {
                    activity.SetTag("mqtt.topic", message.Topic);
                    activity.SetTag("mqtt.qos", message.QoS);
                    activity.SetTag("mqtt.retain", message.Retain);
                }
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    await processor(message);
                    activity?.SetStatus(Status.Ok); // 使用 Status 而不是 ActivityStatusCode
                }
                catch (Exception ex)
                {
                    // 替代 RecordException 的方法
                    activity?.SetStatus(Status.Error, ex.Message);
                    activity?.AddTag("error", true);
                    activity?.AddTag("exception.message", ex.Message);
                    activity?.AddTag("exception.type", ex.GetType().FullName);
                    throw;
                }
                finally
                {
                    stopwatch.Stop();
                    activity?.AddTag("processing.time.ms", stopwatch.ElapsedMilliseconds);
                }

            }
        }
    }

    public static class Status
    {
        public static readonly string Ok = "OK";
        public static readonly string Error = "ERROR";

        public static void SetStatus(this Activity activity, string status, string description = null)
        {
            activity.SetTag("status", status);
            if (!string.IsNullOrEmpty(description))
            {
                activity.SetTag("status.description", description);
            }
        }
    }
}
