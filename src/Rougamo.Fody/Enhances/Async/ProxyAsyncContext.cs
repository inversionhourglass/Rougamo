namespace Rougamo.Fody.Enhances.Async
{
    internal class ProxyAsyncContext
    {
        public ProxyAsyncContext(AsyncFields fields, ProxyAsyncVariables variables)
        {
            Fields = fields;
            Variables = variables;
        }

        public AsyncFields Fields { get; }

        public ProxyAsyncVariables Variables { get; }
    }
}
