using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void WeaveMos()
        {
            foreach (var rouType in _rouTypes)
            {
                foreach (var rouMethod in rouType.Methods)
                {
                    if (rouMethod.IsAsync)
                    {
                        AsyncMethodWeave(rouMethod);
                    }
                    else
                    {
                        SyncMethodWeave(rouMethod);
                    }
                }
            }
        }

        private void SyncMethodWeave(RouMethod rouMethod)
        {

        }

        private void AsyncMethodWeave(RouMethod rouMethod)
        {

        }
    }
}
