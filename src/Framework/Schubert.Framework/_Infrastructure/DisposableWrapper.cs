using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert
{
    public class DisposableWrapper : IDisposable
    {
        private Action m_finallyAction;

        public DisposableWrapper(Action startingAction, Action finallyAction)
        {
            this.m_finallyAction = finallyAction;
            if (startingAction != null)
            {
                startingAction.Invoke();
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            if (this.m_finallyAction != null)
            {
                this.m_finallyAction.Invoke();
                this.m_finallyAction = null;
            }
        }

        #endregion
    }
}
