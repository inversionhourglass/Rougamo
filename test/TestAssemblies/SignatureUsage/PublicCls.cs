namespace SignatureUsage
{
    public class PublicCls
    {
        public void Void() { }

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

        public class PublicICls
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
        }

        protected class ProtectedICls
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
        }

        private class PrivateICls
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
        }

        internal class InternalICls
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
        }

        protected internal class ProtectedInternalICls
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
        }

        private protected class PrivateProtectedICls
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
        }
    }
}
