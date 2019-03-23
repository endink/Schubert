namespace Schubert.Framework.Events
{
    /// <summary>
    /// 表示事件订阅执行的线程。
    /// </summary>
    public enum ThreadOption
    {
        /// <summary>
        /// 回调在事件出发的线程执行。
        /// </summary>
        PublisherThread,

        /// <summary>
        /// 回调在线程池中排队执行。
        /// </summary>
        BackgroundThread
    }
}