using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MqttClientObject
{
    /// <summary>
    /// 消息处理器，异步处理接收到的 MQTT 消息
    /// </summary>
    internal class MessageHandler : IDisposable
    {
        /// <summary>
        /// 消息缓冲区(消息通道)
        /// </summary>
        private readonly Channel<MqttMessage> _messageChannel;
        private readonly CancellationTokenSource _cancellationTokenSource;
        /// <summary>
        /// 工作线程列表
        /// </summary>
        private readonly List<Task> _workerTasks;
        /// <summary>
        /// 消息处理
        /// </summary>
        private readonly Func<MqttMessage, Task> _messageProcessor;
        /// <summary>
        /// 工作线程数
        /// </summary>
        private readonly int _maxDegreeOfParallelism;
        /// <summary>
        /// 是否已经销毁
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="messageProcessor">处理过程</param>
        /// <param name="bufferCapacity">消息处理通道的容量，防止内存溢出</param>
        /// <param name="maxParallelism">并行处理消息的工作线程数，通常设置为 CPU 核心数的 2 倍</param>
        public MessageHandler(Func<MqttMessage, Task> messageProcessor,int bufferCapacity = 10000,int maxParallelism = 10)
        {
            _messageProcessor = messageProcessor;
            _maxDegreeOfParallelism = maxParallelism;
            _cancellationTokenSource = new CancellationTokenSource();
            _workerTasks = new List<Task>();

            // 创建一条有容量限制的通道
            // 投放口 (Writer) → [🛄][🛄][🛄][🛄][🛄] → 取件口 (Reader)
            _messageChannel = Channel.CreateBounded<MqttMessage>(new BoundedChannelOptions(bufferCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,     //通道满了就暂停投放，等到通道出现空间
                SingleReader = false,                       //不允许多个消费者提取消息
                SingleWriter = true                         //只允许一个生产者提供消息
            });
            // 开始工作
            StartWorkers();
        }

        /// <summary>
        /// 开始工作，创建多个Task进行工作
        /// </summary>
        private void StartWorkers()
        {
            for (int i = 0; i < _maxDegreeOfParallelism; i++)
            {
                Task workerTask = Task.Run(async () => await ProcessMessagesAsync());
                _workerTasks.Add(workerTask);
            }
        }

        /// <summary>
        /// 将消息加入通道
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task EnqueueMessageAsync(MqttMessage message)
        {
            if (_disposed) 
                throw new ObjectDisposedException(nameof(MessageHandler));

            await _messageChannel.Writer.WriteAsync(message, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// 消息处理过程
        /// </summary>
        /// <returns></returns>
        private async Task ProcessMessagesAsync()
        {
            CancellationToken token = _cancellationTokenSource.Token;

            try
            {
                // 当有消息可以读取时并且通道是打开状态
                // 开始 → WaitToReadAsync() → 等待消息 → 有消息? → 进入内层循环
                //                     ↓
                //                   无消息且通道关闭 ? → 退出循环
                while (await _messageChannel.Reader.WaitToReadAsync(token))
                {
                    while (_messageChannel.Reader.TryRead(out MqttMessage message))
                    {
                        // 取消请求发生，则立即退出方法
                        if (token.IsCancellationRequested) return;
                        try
                        {
                            Stopwatch stopwatch = Stopwatch.StartNew();
                            await _messageProcessor(message);
                            stopwatch.Stop();
                            //加入性能监控 TODO...
                        }
                        catch (Exception ex)
                        {
                            await HandleMessageErrorAsync(message, ex);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 预期中的取消异常
            }
        }

        /// <summary>
        /// 处理消息异常情况
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private async Task HandleMessageErrorAsync(MqttMessage message, Exception ex)
        {
            Console.WriteLine($"Message processing failed: {ex.Message}, Topic: {message.Topic}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 资源清理
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // 停止接收新消息
            _messageChannel.Writer.TryComplete();

            // 取消正在处理的任务
            _cancellationTokenSource.Cancel();

            try
            {
                // 等待所有工作线程完成
                Task.WaitAll(_workerTasks.ToArray(), TimeSpan.FromSeconds(5));
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException))
            {
                // 预期中的取消异常
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred when the message handler was turned off: {ex.Message}");
            }

            _cancellationTokenSource?.Dispose();
        }
    }
}
