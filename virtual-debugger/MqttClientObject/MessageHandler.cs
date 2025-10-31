using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MqttClientObject
{
    internal class MessageHandler : IDisposable
    {
        private readonly Channel<MqttMessage> _messageChannel;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<Task> _workerTasks;
        private readonly Func<MqttMessage, Task> _messageProcessor;
        private readonly int _maxDegreeOfParallelism;
        private bool _disposed = false;

        public MessageHandler(Func<MqttMessage, Task> messageProcessor,int bufferCapacity = 10000,int maxParallelism = 10)
        {
            _messageProcessor = messageProcessor;
            _maxDegreeOfParallelism = maxParallelism;
            _cancellationTokenSource = new CancellationTokenSource();
            _workerTasks = new List<Task>();

            _messageChannel = Channel.CreateBounded<MqttMessage>(new BoundedChannelOptions(bufferCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = true
            });

            StartWorkers();
        }

        private void StartWorkers()
        {
            for (int i = 0; i < _maxDegreeOfParallelism; i++)
            {
                var workerTask = Task.Run(async () => await ProcessMessagesAsync());
                _workerTasks.Add(workerTask);
            }
        }

        public async Task EnqueueMessageAsync(MqttMessage message)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MessageHandler));

            await _messageChannel.Writer.WriteAsync(message, _cancellationTokenSource.Token);
        }

        private async Task ProcessMessagesAsync()
        {
            var token = _cancellationTokenSource.Token;

            try
            {
                while (await _messageChannel.Reader.WaitToReadAsync(token))
                {
                    while (_messageChannel.Reader.TryRead(out var message))
                    {
                        if (token.IsCancellationRequested)
                            return;

                        try
                        {
                            await _messageProcessor(message);
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

        private async Task HandleMessageErrorAsync(MqttMessage message, Exception ex)
        {
            Console.WriteLine($"处理消息失败: {ex.Message}, Topic: {message.Topic}");
            await Task.CompletedTask;
        }

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
                Console.WriteLine($"关闭消息处理器时发生异常: {ex.Message}");
            }

            _cancellationTokenSource?.Dispose();
        }
    }
}
