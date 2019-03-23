using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    /// <summary>
    /// 可是释放对象的基础类型。
    /// </summary>
    public class DisposableObject : IDisposable
    {
        private volatile bool _disposed;

        /// <summary>
        /// 当对象被释放后，抛出 <see cref="ObjectDisposedException"/> 异常。
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        ~DisposableObject()
        {
            Dispose(false);
        }

        /// <summary>
        /// 释放托管资源。
        /// </summary>
        protected virtual void DisposeManagedResources()
        {

        }

        /// <summary>
        /// 释放非托管资源。
        /// </summary>
        protected virtual void DisposeUnmanagedResources()
        {

        }

        ///<summary>
        /// 非密封类修饰用protected virtual
        /// 密封类修饰用private
        ///</summary>
        ///<param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            if (disposing)
            {
                this.DisposeManagedResources();
            }
            this.DisposeUnmanagedResources();
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
