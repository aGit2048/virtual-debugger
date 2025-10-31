using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttClientObject
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var options = new MqttClientOptions
            {
                BrokerAddress = "localhost",
                Port = 1883,
                ClientId = "high_performance_client",
                Username = "user",
                Password = "pass",
                UseTls = false,
                KeepAliveInterval = 30
            };

            // 使用普通的 using 语句而不是 await using
            using (var client = new UltimateMqttClient(options))
            {
                // 注册消息接收事件
                client.MessageReceived += async message =>
                {
                    Console.WriteLine($"收到消息: {message.Topic}, 长度: {message.Payload.Length}");
                    await Task.CompletedTask;
                };

                // 连接
                if (await client.ConnectAsync())
                {
                    Console.WriteLine("连接成功");

                    // 订阅主题
                    await client.SubscribeAsync("test/topic");

                    // 发布测试消息
                    for (int i = 0; i < 10; i++)
                    {
                        var payload = System.Text.Encoding.UTF8.GetBytes($"Message {i} - {DateTime.Now:HH:mm:ss}");
                        var success = await client.PublishAsync("test/topic", payload);

                        if (success)
                        {
                            Console.WriteLine($"消息 {i} 发布成功");
                        }
                        else
                        {
                            Console.WriteLine($"消息 {i} 发布失败");
                        }

                        await Task.Delay(1000);
                    }

                    Console.WriteLine("按任意键退出...");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("连接失败");
                }
            }

            Console.WriteLine("客户端已释放，程序结束");
        }
    }
}
