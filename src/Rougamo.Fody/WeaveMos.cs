using Mono.Cecil.Cil;
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
                    if (rouMethod.IsIterator)
                    {
                        IteratorMethodWeave(rouMethod);
                    }
                    else if (rouMethod.IsAsync)
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

        private void IteratorMethodWeave(RouMethod rouMethod)
        {

        }

        private void AsyncMethodWeave(RouMethod rouMethod)
        {

        }

        private void SyncMethodWeave(RouMethod rouMethod)
        {
        }

        private MoVariables CreateVariables(RouMethod rouMethod)
        {
            //var entryContext = rouMethod.MethodDef.Body.CreateVariable(_entryContextRef);
            //var successContext = rouMethod.MethodDef.Body.CreateVariable(_sucessContextRef);
            //var exceptionContext = rouMethod.MethodDef.Body.CreateVariable(_exceptionContextRef);
            //var exitContext = rouMethod.MethodDef.Body.CreateVariable(_exitContextRef);
            //var arguments = rouMethod.MethodDef.Body.CreateVariable(_objectArrayRef);
            //var mos = new VariableDefinition[rouMethod.Mos.Count];
            //var i = 0;
            //foreach (var mo in rouMethod.Mos)
            //{
            //    //mos[i++] = rouMethod.MethodDef.Body.CreateVariable();
            //}
            //return new MoVariables
            //{
            //    EntryContext = entryContext,
            //    SuccessContext = successContext,
            //    ExceptionContext = exceptionContext,
            //    ExitContext = exitContext,
            //    Mos = mos,
            //    Arguments = arguments
            //};
            return null;
        }
    }
}
