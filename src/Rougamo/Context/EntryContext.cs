namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行前上线文
    /// </summary>
    public sealed class EntryContext : MethodContext
    {
        private readonly ExitContext _exit;

        /// <summary>
        /// </summary>
        public EntryContext(ExitContext exit) : base(exit.Target, exit.Method, exit.Arguments)
        {
            _exit = exit;
        }
    }
}
