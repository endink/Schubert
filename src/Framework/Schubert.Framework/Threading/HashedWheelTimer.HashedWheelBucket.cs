using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Threading
{
    partial class HashedWheelTimer
    {
        internal sealed class HashedWheelBucket : IDisposable
        {

            // Used for the linked-list datastructure
            private HashedWheelTimeout head;
            private HashedWheelTimeout tail;

            /**
             * 添加一个<see cref="HashedWheelTimeout"/> 到哈希桶中。
             */
            public void addTimeout(HashedWheelTimeout timeout)
            {
                //assert timeout.bucket == null;
                timeout.Bucket = this;
                if (head == null)
                {
                    head = tail = timeout;
                }
                else
                {
                    tail.Next = timeout;
                    timeout.Prev = tail;
                    tail = timeout;
                }
            }

            /// <summary>
            /// 过期所有给定 <paramref name="deadline"/> 的 <see cref="HashedWheelTimeout"/>。
            /// </summary>
            /// <param name="deadline"></param>
            public void ExpireTimeouts(long deadline)
            {
                HashedWheelTimeout timeout = head;

                // process all timeouts
                while (timeout != null)
                {
                    bool remove = false;
                    if (timeout.RemainingRounds <= 0)
                    {
                        if (timeout.Deadline <= deadline)
                        {
                            timeout.Expire();
                        }
                        else
                        {
                            // The timeout was placed into a wrong slot. This should never happen.
                            throw new HashedWheelTimerException($"timeout.deadline ({timeout.Deadline}) > deadline ({deadline})");
                        }
                        remove = true;
                    }
                    else if (timeout.IsCancelled)
                    {
                        remove = true;
                    }
                    else
                    {
                        timeout.RemainingRounds--;
                    }
                    // store reference to next as we may null out timeout.next in the remove block.
                    HashedWheelTimeout next = timeout.Next;
                    if (remove)
                    {
                        this.Remove(timeout);
                    }
                    timeout = next;
                }
            }

            public void Remove(HashedWheelTimeout timeout)
            {
                HashedWheelTimeout next = timeout.Next;
                // remove timeout that was either processed or cancelled by updating the linked-list
                if (timeout.Prev != null)
                {
                    timeout.Prev.Next = next;
                }
                if (timeout.Next != null)
                {
                    timeout.Next.Prev = timeout.Prev;
                }

                if (timeout == head)
                {
                    // if timeout is also the tail we need to adjust the entry too
                    if (timeout == tail)
                    {
                        tail = null;
                        head = null;
                    }
                    else
                    {
                        head = next;
                    }
                }
                else if (timeout == tail)
                {
                    // if the timeout is the tail modify the tail to be the prev node.
                    tail = timeout.Prev;
                }
                // null out prev, next and bucket to allow for GC.
                timeout.Prev = null;
                timeout.Next = null;
                timeout.Bucket = null;
            }

            /**
             * Clear this bucket and return all not expired / cancelled {@link Timeout}s.
             */
            public void ClearTimeouts(ISet<ITimeout> set)
            {
                while(true)
                {
                    HashedWheelTimeout timeout = PollTimeout();
                    if (timeout == null)
                    {
                        return;
                    }
                    if (timeout.IsExpired || timeout.IsCancelled)
                    {
                        continue;
                    }
                    set.Add(timeout);
                }
            }

            private HashedWheelTimeout PollTimeout()
            {
                HashedWheelTimeout head = this.head;
                if (head == null)
                {
                    return null;
                }
                HashedWheelTimeout next = head.Next;
                if (next == null)
                {
                    tail = this.head = null;
                }
                else
                {
                    this.head = next;
                    next.Prev = null;
                }

                // null out prev and next to allow for GC.
                head.Next = null;
                head.Prev = null;
                return head;
            }

            public void Dispose()
            {
                this.head?.Dispose();
                this.tail?.Dispose();
                this.head = null;
                this.tail = null;
            }
        }
    }
}
