namespace SignatureUsage
{
    class DefaultCls
    {
        public string Public()
        {
            return nameof(Public);
        }

        protected string Protected()
        {
            return nameof(Protected);
        }

        private string Private()
        {
            return nameof(Private);
        }

        internal string Internal()
        {
            return nameof(Internal);
        }

        protected internal string ProtectedInternal()
        {
            return nameof(ProtectedInternal);
        }

        private protected string PrivateProtected()
        {
            return nameof(PrivateProtected);
        }

        string Default()
        {
            return nameof(Default);
        }

        static string DefaultStatic()
        {
            return nameof(DefaultStatic);
        }

        protected internal static string ProtectedInternalStatic()
        {
            return nameof(ProtectedInternalStatic);
        }

        private protected static string PrivateProtectedStatic()
        {
            return nameof(PrivateProtectedStatic);
        }
    }
}
