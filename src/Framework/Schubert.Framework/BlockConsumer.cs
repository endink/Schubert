using Schubert.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    /// <summary>
    /// 阻塞式队列消费程序基础构造。
    /// </summary>
    public abstract class BlockConsumer
    {
        private bool _disposed;
        private long _queueSizeBytes;
        private long _elementCount;
        private CancellationTokenSource _cancellationSource;
        private bool _isRunning;
        private Barrier _barrier = null;
       

        private readonly ConcurrentQueue<byte[]> _queue = null;


        public BlockConsumer()
        {
            _queue = new ConcurrentQueue<byte[]>();
            _cancellationSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 获取本地队列选项。
        /// </summary>
        protected abstract QueueOptions QueueOptions { get; }
        
        /// <summary>
        /// 获取当前本地缓冲队列的大小（单位 kb）。
        /// </summary>
        public long QueueSizeKBytes
        {
            get { return _queueSizeBytes; }
        }

        /// <summary>
        /// 获取当前本地缓冲队列的记录数。
        /// </summary>
        public long QueueCount
        {
            get { return _elementCount; }
        }


        protected virtual void OnQueueFulled()
        {
        }

        protected virtual void OnConsumeThreadStated()
        { }

        public void Run()
        {
            ThrowIfDisposed();
            if (_isRunning)
            {
                return;
            }
            _isRunning = true;
            _barrier?.Dispose();
            _barrier = new Barrier(this.QueueOptions.ConsumeConcurrencyLevel);
            foreach (var i in Enumerable.Range(1, this.QueueOptions.ConsumeConcurrencyLevel))
            {
                this.StartParallel();
            }
        }

        private void StartParallel()
        {
            var task = new Task(state =>
            {
                this.OnConsumeThreadStated();
                while (!(_cancellationSource?.IsCancellationRequested ?? true))
                {
                    byte[] buffer = null;
                    if (this.QueueCount > 0 && (buffer = this.DequeueBuffer()) != null)
                    {
                        byte[] keyLengBytes = new byte[4];

                        Array.Copy(buffer, keyLengBytes, 4);
                        int keyLen = BitConverter.ToInt32(keyLengBytes, 0);

                        byte[] keyBytes = new byte[keyLen];

                        Array.Copy(buffer, 4, keyBytes, 0, keyLen);

                        string key = Encoding.UTF8.GetString(keyBytes);

                        int otherLength = 4 + keyLen;
                        byte[] dataBytes = new byte[buffer.Length - otherLength];

                        Array.Copy(buffer, otherLength, dataBytes, 0, dataBytes.Length);

                        try
                        {
                            this.Consume(key, dataBytes, buffer);
                        }
                        catch (Exception ex)
                        {
                            this.EqueueBuffer(buffer);
                            ex.ThrowIfNecessary();
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }, state:null, creationOptions: TaskCreationOptions.LongRunning);
            task.Start();
        }

        protected abstract void Consume(string key, byte[] dataBytes, byte[] rawBytes);



        public void Stop()
        {
            ThrowIfDisposed();
            _cancellationSource.Cancel();
            _isRunning = false;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _cancellationSource?.Cancel();
                _cancellationSource.Dispose();
                _cancellationSource = null;

                this.OnDisposing();
            }
        }

        protected virtual void OnDisposing()
        {
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
        

        protected void EqueueBuffer(byte[] buffer)
        {
            _queue.Enqueue(buffer);
            Interlocked.Add(ref _queueSizeBytes, buffer.Length);
            Interlocked.Increment(ref _elementCount);
        }

        protected void EqueueBuffer(byte[] data, string key)
        {
            int keyCount = 0;
            byte[] keyBytes = new byte[0];
            if (key != null)
            {
                keyBytes = Encoding.UTF8.GetBytes(key);
                keyCount = keyBytes.Length;
            }

            //前 4个字节为一个数字。
            long totalLength = 4L + keyBytes.Length + data.Length;

            //队列是否已经超限。
            bool queueFull = (_queue.Count >= QueueOptions.QueueMaxCount) || ((this._queueSizeBytes + totalLength) > (QueueOptions.QueueMaxSizeKBytes * 1024));


            byte[] combinedBytes = new byte[totalLength];

            //头四个字节字节用于存储 Key 的长度。
            byte[] keyLengBytes = BitConverter.GetBytes(keyCount);

            Array.Copy(keyLengBytes, 0, combinedBytes, 0, 4);
            Array.Copy(keyBytes, 0, combinedBytes, 4, keyBytes.Length);
            Array.Copy(data, 0, combinedBytes, 4 + keyBytes.Length, data.Length);

            if (queueFull)
            {
                this.OnQueueFulled();
                this.DequeueBuffer();
            }

            _queue.Enqueue(combinedBytes);
            Interlocked.Add(ref _queueSizeBytes, totalLength);
            Interlocked.Increment(ref _elementCount);
        }


        protected byte[] DequeueBuffer()
        {
            byte[] popItem = null;
            if (_queue.TryDequeue(out popItem))
            {
                Interlocked.Add(ref _queueSizeBytes, -popItem.Length);
                Interlocked.Decrement(ref _elementCount);
                return popItem;
            }
            return null;
        }
        
    }
}
