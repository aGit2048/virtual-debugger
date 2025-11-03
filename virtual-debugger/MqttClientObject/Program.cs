using System;
using System.Threading.Tasks;

namespace MqttClientObject
{
    internal class Program
    {
        static async Task Main1(string[] args)
        {
            MqttConnectOptions.DevelopmentEnvOptions.BrokerAddress = "127.0.0.1";
            MqttConnectOptions.DevelopmentEnvOptions.Port = 1883;
            MqttConnectOptions.DevelopmentEnvOptions.AutoReconnect = true;

            using (MqttClientService client = new MqttClientService(MqttConnectOptions.DevelopmentEnvOptions))
            {
                // 注册消息接收事件
                client.MessageReceived += async message =>
                {
                    Console.WriteLine($"收到消息: {message.Topic}, 长度: {message.Payload.Length}");
                    await Task.CompletedTask;
                };

                bool connectStatus = await client.ConnectAsync();
                if (connectStatus)
                {
                    Console.WriteLine("连接成功");
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

                        await Task.Delay(100);
                    }

                    Console.WriteLine("按任意键退出...");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Connection failed！");
                }
            }
            Console.WriteLine("The client has been released and the program has ended！");
        }
    }
}
