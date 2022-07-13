using BasicUsage.Attributes;
using System;

namespace BasicUsage
{
    public class ModifyReturnValue : MoDataContainer
    {
        [ExceptionHandle]
        public string Exception(bool throwException = true)
        {
            if (throwException) throw new InvalidOperationException(nameof(Exception));

            return Guid.NewGuid().ToString();
        }

        [ExceptionHandle]
        public int ExceptionWithUnbox(bool throwException = true)
        {
            if (throwException) throw new InvalidOperationException(nameof(ExceptionWithUnbox));

            return GetHashCode();
        }

        [ExceptionHandle]
        public double ExceptionUnhandled(bool throwException = true)
        {
            if (throwException) throw new InvalidOperationException(nameof(ExceptionUnhandled));

            return GetHashCode() * 1.0 / nameof(ExceptionUnhandled).Length;
        }

        [ReturnValueReplace]
        public string Succeeded(object[] args)
        {
            return Guid.NewGuid().ToString() + "|" + string.Join("|", args);
        }

        [ReturnValueReplace]
        public int SucceededWithUnbox()
        {
            return int.MaxValue % nameof(SucceededWithUnbox).Length;
        }

        [ReturnValueReplace]
        public double SucceededUnrecognized()
        {
            return int.MaxValue * 1.0 / nameof(SucceededUnrecognized).Length;
        }

        [ReturnValueReplace]
        public int? Nullable()
        {
            return null;
        }

        [ReplaceValueOnEntry]
        public string[] CachedArray()
        {
            return new string[0];
        }

        [ReplaceValueOnEntry]
        public string[] CachedEvenThrows()
        {
            throw new NullReferenceException();
        }

        [ReplaceValueOnEntry]
        public long TryReplaceLongToNull()
        {
            return 123;
        }

        [ReplaceValueOnEntry]
        public long? TryReplaceNullableToNull()
        {
            return 321;
        }
    }
}
