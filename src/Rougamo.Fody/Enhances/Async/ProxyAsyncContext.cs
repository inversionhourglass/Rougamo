namespace Rougamo.Fody.Enhances.Async
{
    internal class StrictAsyncContext
    {
        public StrictAsyncContext(AsyncFields fields, StrictAsyncVariables variables)
        {
            Fields = fields;
            Variables = variables;
        }

        public AsyncFields Fields { get; }

        public StrictAsyncVariables Variables { get; }
    }
}
