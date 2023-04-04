using BasicUsage;
using BasicUsage.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rougamo.Fody.Tests
{
    [Collection(nameof(BasicUsage))]
    public class BasicTest : TestBase
    {
        public BasicTest() : base("BasicUsage.dll")
        {
        }

        protected override string RootNamespace => "BasicUsage";

        [Fact]
        public void InstanceSingleMethodTest()
        {
            var instance = GetInstance("InstanceSingleMethod");
            var arrArg = new object[] { Guid.NewGuid(), 1.2, new object() };
            instance.Entry(1, "2", arrArg);
            Assert.Equal(instance.Context.Arguments.Length, 3);
            for (var i = 0; i < arrArg.Length; i++)
            {
                Assert.Equal(arrArg[i], instance.Context.Arguments[2][i]);
            }
            Assert.False(instance.Context.IsAsync);
            Assert.False(instance.Context.IsIterator);

            instance.Context = null;
            Assert.Throws<InvalidOperationException>(() => instance.Exception());
            Assert.NotNull(instance.Context.Exception);
            // Combine the following two steps will throws an exception.
            IDictionary data = instance.Context.Exception.Data;
            Assert.Equal(1, data.Count);

            instance.Context = null;
            var successValue = instance.Success();
            Assert.Equal(successValue, instance.Context.ReturnValue);

            instance.Context = null;
            Assert.Throws<InvalidOperationException>(() => instance.ExitWithException());
            Assert.Null(instance.Context.ReturnValue);
            Assert.NotNull(instance.Context.Exception);

            instance.Context = null;
            var exitWithSuccessValue = instance.ExitWithSuccess();
            Assert.Equal(exitWithSuccessValue, instance.Context.ReturnValue);
            Assert.Null(instance.Context.Exception);
        }

        [Fact]
        public async Task AsyncInstanceSingleMethodTest()
        {
            var instance = GetInstance("AsyncInstanceSingleMethod");
            var arrArg = new object[] { Guid.NewGuid(), 1.2, new object() };
#if NET461 || NET6
            await (Task)instance.EntryAsync(1, "2", arrArg);
#else
            await (ValueTask<string>)instance.EntryAsync(1, "2", arrArg);
#endif
            Assert.Equal(instance.Context.Arguments.Length, 3);
            for (var i = 0; i < arrArg.Length; i++)
            {
                Assert.Equal(arrArg[i], instance.Context.Arguments[2][i]);
            }
            Assert.True(instance.Context.IsAsync);
            Assert.False(instance.Context.IsIterator);

            instance.Context = null;
#if NET461 || NET6
            await Assert.ThrowsAsync<InvalidOperationException>(() => (Task)instance.ExceptionAsync());
#else
            await Assert.ThrowsAsync<InvalidOperationException>(() => ((ValueTask<string>)instance.ExceptionAsync()).AsTask());
#endif
            Assert.NotNull(instance.Context.Exception);
            // Combine the following two steps will throws an exception.
            IDictionary data = instance.Context.Exception.Data;
            Assert.Equal(1, data.Count);

            instance.Context = null;
#if NET461 || NET6
            var successValue = await (Task<int>)instance.SuccessAsync();
#else
            var successValue = await (ValueTask<int>)instance.SuccessAsync();
#endif
            Assert.Equal(successValue, instance.Context.ReturnValue);

            instance.Context = null;
#if NET461 || NET6
            await Assert.ThrowsAsync<InvalidOperationException>(() => (Task)instance.ExitWithExceptionAsync());
#else
            await Assert.ThrowsAsync<InvalidOperationException>(() => ((ValueTask<string>)instance.ExitWithExceptionAsync()).AsTask());
#endif
            Assert.Null(instance.Context.ReturnValue);
            Assert.NotNull(instance.Context.Exception);

            instance.Context = null;
#if NET461 || NET6
            var exitWithSuccessValue = await (Task<string>)instance.ExitWithSuccessAsync();
#else
            var exitWithSuccessValue = await (ValueTask<string>)instance.ExitWithSuccessAsync();
#endif
            Assert.Equal(instance.Context.ReturnValue, exitWithSuccessValue);
            Assert.Null(instance.Context.Exception);
        }

        [Fact]
        public void ModifyReturnValueTest()
        {
            var originInstance = new ModifyReturnValue();
            var instance = GetInstance("ModifyReturnValue");

            Assert.Throws<InvalidOperationException>(() => originInstance.Exception());
            var exceptionHandledValue = instance.Exception();
            Assert.Equal(ExceptionHandleAttribute.StringValue, exceptionHandledValue);

            Assert.Throws<InvalidOperationException>(() => originInstance.ExceptionWithUnbox());
            var exceptionHandledUnboxValue = instance.ExceptionWithUnbox();
            Assert.Equal(ExceptionHandleAttribute.IntValue, exceptionHandledUnboxValue);

            Assert.Throws<InvalidOperationException>(() => originInstance.ExceptionUnhandled());
            Assert.Throws<InvalidOperationException>(() => instance.ExceptionUnhandled());

            var args = new object[] { 1, '-', nameof(ModifyReturnValueTest), 3.14 };
            var originSuccessValue = originInstance.Succeeded(args);
            var replacedSuccessValue = instance.Succeeded(args);
            Assert.NotEqual(originSuccessValue, replacedSuccessValue);
            Assert.Equal(ReturnValueReplaceAttribute.StringValue, replacedSuccessValue);

            var originUnboxValue = originInstance.SucceededWithUnbox();
            var replacedUnboxValue = instance.SucceededWithUnbox();
            Assert.NotEqual(originUnboxValue, replacedUnboxValue);
            Assert.Equal(ReturnValueReplaceAttribute.IntValue, replacedUnboxValue);

            var originValue = originInstance.SucceededUnrecognized();
            var unrecognizedValue = instance.SucceededUnrecognized();
            Assert.Equal(originValue, unrecognizedValue);

            var originNullableValue = originInstance.Nullable();
            var replacedNullableValue = instance.Nullable();
            Assert.Null(originNullableValue);
            Assert.Equal(ReturnValueReplaceAttribute.IntValue, replacedNullableValue);

            var originArrayValue = originInstance.CachedArray();
            var cachedArrayValue = instance.CachedArray();
            Assert.NotEqual(originArrayValue, cachedArrayValue);
            Assert.Equal(ReplaceValueOnEntryAttribute.ArrayValue, cachedArrayValue);

            Assert.Throws<NullReferenceException>(() => originInstance.CachedEvenThrows());
            var cachedArrayValueWithoutThrows = instance.CachedEvenThrows();
            Assert.Equal(ReplaceValueOnEntryAttribute.ArrayValue, cachedArrayValueWithoutThrows);

            Assert.Throws<NullReferenceException>(() => instance.TryReplaceLongToNull());
            Assert.Null(instance.TryReplaceNullableToNull());
        }

        [Fact]
        public async Task AsyncModifyReturnValueTest()
        {
            var originInstance = new AsyncModifyReturnValue();
            var instance = GetInstance("AsyncModifyReturnValue");

#if NET461 || NET6
            await Assert.ThrowsAsync<InvalidOperationException>(() => originInstance.ExceptionAsync());
            var exceptionHandledValue = await (Task<string>)instance.ExceptionAsync();
#else
            await Assert.ThrowsAsync<InvalidOperationException>(() => originInstance.ExceptionAsync().AsTask());
            var exceptionHandledValue = await (ValueTask<string>)instance.ExceptionAsync();
#endif
            Assert.Equal(ExceptionHandleAttribute.StringValue, exceptionHandledValue);

#if NET461 || NET6
            await Assert.ThrowsAsync<InvalidOperationException>(() => originInstance.ExceptionWithUnboxAsync());
            var exceptionHandledUnboxValue = await (Task<int>)instance.ExceptionWithUnboxAsync();
#else
            await Assert.ThrowsAsync<InvalidOperationException>(() => originInstance.ExceptionWithUnboxAsync().AsTask());
            var exceptionHandledUnboxValue = await (ValueTask<int>)instance.ExceptionWithUnboxAsync();
#endif
            Assert.Equal(ExceptionHandleAttribute.IntValue, exceptionHandledUnboxValue);

#if NET461 || NET6
            await Assert.ThrowsAsync<InvalidOperationException>(() => originInstance.ExceptionUnhandledAsync());
            await Assert.ThrowsAsync<InvalidOperationException>(() => (Task)instance.ExceptionUnhandledAsync());
#else
            await Assert.ThrowsAsync<InvalidOperationException>(() => originInstance.ExceptionUnhandledAsync().AsTask());
            await Assert.ThrowsAsync<InvalidOperationException>(() => ((ValueTask<double>)instance.ExceptionUnhandledAsync()).AsTask());
#endif

            var args = new object[] { 1, '-', nameof(ModifyReturnValueTest), 3.14 };
            var originSuccessValue = await originInstance.SucceededAsync(args);
#if NET461 || NET6
            var replacedSuccessValue = await (Task<string>)instance.SucceededAsync(args);
#else
            var replacedSuccessValue = await (ValueTask<string>)instance.SucceededAsync(args);
#endif
            Assert.NotEqual(originSuccessValue, replacedSuccessValue);
            Assert.Equal(ReturnValueReplaceAttribute.StringValue, replacedSuccessValue);

            var originUnboxValue = await originInstance.SucceededWithUnboxAsync();
#if NET461 || NET6
            var replacedUnboxValue = await (Task<int>)instance.SucceededWithUnboxAsync();
#else
            var replacedUnboxValue = await (ValueTask<int>)instance.SucceededWithUnboxAsync();
#endif
            Assert.NotEqual(originUnboxValue, replacedUnboxValue);
            Assert.Equal(ReturnValueReplaceAttribute.IntValue, replacedUnboxValue);

            var originValue = await originInstance.SucceededUnrecognizedAsync();
#if NET461 || NET6
            var unrecognizedValue = await (Task<double>)instance.SucceededUnrecognizedAsync();
#else
            var unrecognizedValue = await (ValueTask<double>)instance.SucceededUnrecognizedAsync();
#endif
            Assert.Equal(originValue, unrecognizedValue);


            var originArrayValue = await originInstance.CachedArrayAsync();
#if NET461 || NET6
            var cachedArrayValue = await (Task<string[]>)instance.CachedArrayAsync();
#else
            var cachedArrayValue = await (ValueTask<string[]>)instance.CachedArrayAsync();
#endif
            Assert.NotEqual(originArrayValue, cachedArrayValue);
            Assert.Equal(ReplaceValueOnEntryAttribute.ArrayValue, cachedArrayValue);

#if NET461 || NET6
            await Assert.ThrowsAsync<NullReferenceException>(() => originInstance.CachedEvenThrowsAsync());
            var cachedArrayValueWithoutThrows = await (Task<string[]>)instance.CachedEvenThrowsAsync();
#else
            await Assert.ThrowsAsync<NullReferenceException>(() => originInstance.CachedEvenThrowsAsync().AsTask());
            var cachedArrayValueWithoutThrows = await (ValueTask<string[]>)instance.CachedEvenThrowsAsync();
#endif
            Assert.Equal(ReplaceValueOnEntryAttribute.ArrayValue, cachedArrayValueWithoutThrows);

#if NET461 || NET6
            await Assert.ThrowsAsync<NullReferenceException>(() => (Task<long>)instance.TryReplaceLongToNullAsync());
            Assert.Null(await (Task<long?>)instance.TryReplaceNullableToNullAsync());
#else
            await Assert.ThrowsAsync<NullReferenceException>(() => ((ValueTask<long>)instance.TryReplaceLongToNullAsync()).AsTask());
            Assert.Null(await (ValueTask<long?>)instance.TryReplaceNullableToNullAsync());
#endif
        }

        [Fact]
        public async Task ModifyIteratorReturnValueTest()
        {
            // iterator can not handle exception and modify return value
            var originInstance = new ModifyIteratorReturnValue();
            var instance = GetInstance("ModifyIteratorReturnValue");

            var min = 5;
            var max = 30;
            var originSucceedValue = originInstance.Succeed(10, min, max).ToArray();
            var modifiedSucceedValue = ((IEnumerable<int>)instance.Succeed(10, min, max)).ToArray();
            Assert.NotEqual(ReturnValueReplaceAttribute.IteratorValue, modifiedSucceedValue);
            Assert.Equal(originSucceedValue.Length, modifiedSucceedValue.Length);
            Assert.All(modifiedSucceedValue, x => Assert.True(x >= min && x < max));

            Assert.Throws<InvalidOperationException>(() => originInstance.Exception(min).ToArray());
            Assert.Throws<InvalidOperationException>(() => ((IEnumerable<int>)instance.Exception(min)).ToArray());

            min = 10;
            max = 50;
            var originAsyncSucceedValue = await originInstance.SucceedAsync(10, min, max).ToArrayAsync();
            var modifiedAsyncSucceedValue = await ((IAsyncEnumerable<int>)instance.SucceedAsync(10, min, max)).ToArrayAsync();
            Assert.NotEqual(ExceptionHandleAttribute.IteratorValue, modifiedAsyncSucceedValue);
            Assert.Equal(originAsyncSucceedValue.Length, modifiedAsyncSucceedValue.Length);
            Assert.All(modifiedAsyncSucceedValue, x => Assert.True(x >= min && x < max));

            await Assert.ThrowsAsync<InvalidOperationException>(() => originInstance.ExceptionAsync(min).ToArrayAsync().AsTask());
            await Assert.ThrowsAsync<InvalidOperationException>(() => ((IAsyncEnumerable<int>)instance.ExceptionAsync(min)).ToArrayAsync().AsTask());
        }

        [Fact]
        public void SyncTest()
        {
            var instance = GetInstance("SyncExecution");

            var input = new List<string>();
            instance.Void(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(SyncExecution.Void), nameof(IMo.OnSuccess), nameof(IMo.OnExit) }, input);

            input = new List<string>();
            Assert.Throws<InvalidOperationException>(() => { instance.VoidThrows(input); });
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(SyncExecution.VoidThrows), nameof(IMo.OnException), nameof(IMo.OnExit) }, input);

            input = new List<string>();
            var output = instance.ReplaceOnEntry(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(IMo.OnExit) }, input);
            Assert.Equal(nameof(IMo.OnEntry), output);

            input = new List<string>();
            output = instance.ReplaceOnException(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(SyncExecution.ReplaceOnException), nameof(IMo.OnException), nameof(IMo.OnExit) }, input);
            Assert.Equal(nameof(IMo.OnException), output);

            input = new List<string>();
            output = instance.ReplaceOnSuccess(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(SyncExecution.ReplaceOnSuccess), nameof(IMo.OnSuccess), nameof(IMo.OnExit) }, input);
            Assert.Equal(nameof(IMo.OnSuccess), output);

            input = new List<string>();
            instance.Empty(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(IMo.OnSuccess), nameof(IMo.OnExit) }, input);

            input = new List<string>();
            instance.EmptyReplaceOnEntry(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(IMo.OnExit) }, input);
        }

        [Fact]
        public async Task AsyncTest()
        {
            var instance = GetInstance("AsyncExecution");

            var input = new List<string>();
            await (Task)instance.Void1(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(AsyncExecution.Void1), nameof(IMo.OnSuccess), nameof(IMo.OnExit) }, input);

            input = new List<string>();
            await (Task)instance.Void2(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(AsyncExecution.Void2), nameof(IMo.OnSuccess), nameof(IMo.OnExit) }, input);

            input = new List<string>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => instance.VoidThrows1(input));
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(AsyncExecution.VoidThrows1), nameof(IMo.OnException), nameof(IMo.OnExit) }, input);

            input = new List<string>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => instance.VoidThrows2(input));
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(AsyncExecution.VoidThrows2), nameof(IMo.OnException), nameof(IMo.OnExit) }, input);

            input = new List<string>();
            var output = await (Task<string>)instance.ReplaceOnEntry(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(IMo.OnExit) }, input);
            Assert.Equal(nameof(IMo.OnEntry), output);

            input = new List<string>();
            output = await (Task<string>)instance.ReplaceOnException(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(SyncExecution.ReplaceOnException), nameof(IMo.OnException), nameof(IMo.OnExit) }, input.ToArray());
            Assert.Equal(nameof(IMo.OnException), output);

            input = new List<string>();
            output = await (Task<string>)instance.ReplaceOnSuccess(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(SyncExecution.ReplaceOnSuccess), nameof(IMo.OnSuccess), nameof(IMo.OnExit) }, input);
            Assert.Equal(nameof(IMo.OnSuccess), output);

            input = new List<string>();
            await (Task)instance.Empty(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(IMo.OnSuccess), nameof(IMo.OnExit) }, input);

            input = new List<string>();
            await (Task)instance.EmptyReplaceOnEntry(input);
            Assert.Equal(new[] { nameof(IMo.OnEntry), nameof(IMo.OnExit) }, input);
        }

        [Fact]
        public async Task ArgumentRewriteTest()
        {
            var instance = GetInstance(nameof(ModifyArguments));
            var attribute = GetStaticInstance(typeof(RewriteArgsAttribute).FullName, true);

            string a = string.Empty;
            sbyte b = 1;
            int? c = -1;
            object d = new object();
            Type e = null;
            DbType f = DbType.VarNumeric;
            DbType[] g = new DbType[0];
            DateTime h = DateTime.Now;
            double i = -1;

            sbyte j = 0;
            int? k = 0;
            DbType l = DbType.Currency;
            DbType[] m = new DbType[0];
            double n = 1;

            string o;
            object p;
            Type q = typeof(BasicTest);
            DateTime r;
            double s = 0;
            var returns = (object[])instance.Sync<double>(a, b, c, d, e, f, g, h, i, ref j, ref k, ref l, ref m, ref n, out o, out p, out q, out r, out s);
            DoAssert(returns, 14);

            returns = await (Task<object[]>)instance.Async<double>(a, b, c, d, e, f, g, h, i);
            DoAssert(returns, returns.Length);

            returns = await (ValueTask<object[]>)instance.AsyncValue<double>(a, b, c, d, e, f, g, h, i);
            DoAssert(returns, returns.Length);

            returns = ((IEnumerable<object[]>)instance.Iterator<double>(a, b, c, d, e, f, g, h, i)).ToArray()[0];
            DoAssert(returns, returns.Length);

            await foreach (var item in (IAsyncEnumerable<object[]>)instance.AsyncIterator<double>(a, b, c, d, e, f, g, h, i))
            {
                DoAssert(item, returns.Length);
                break;
            }

            void DoAssert(object[] objs, int count)
            {
                var values = (IReadOnlyDictionary<Type, object>)attribute.Values;
                for (var x = 0; x < count; x++)
                {
                    var obj = objs[x];
                    if (obj == null) continue;

                    var type = obj.GetType();
                    if (type == typeof(int)) type = typeof(int?);
                    else if (type.FullName == "System.RuntimeType") type = typeof(Type);
                    Assert.Equal(values[type], obj);
                }
            }
        }

        [Fact]
        public async Task RetryTest()
        {
            var instance = GetInstance(nameof(SyncExecution));
            var asyncInstance = GetInstance(nameof(AsyncExecution));

            var datas = new List<int>();

            try
            {
                instance.RetryException(datas);
            }
            catch
            {
                // ignore
            }
            Assert.Equal(3, datas.Count);

            datas.Clear();
            instance.RetrySuccess(datas);
            Assert.Equal(3, datas.Count);

            try
            {
                datas.Clear();
                await (Task)asyncInstance.RetryException(datas);
            }
            catch
            {
                // ignore
            }
            Assert.Equal(3, datas.Count);

            datas.Clear();
            await (Task)asyncInstance.RetrySuccess(datas);
            Assert.Equal(3, datas.Count);
        }

        [Fact]
        public async Task OrderTest()
        {
            var instance = GetInstance(nameof(OrderUseCase));

            string[] expected;
            List<string> actual;

            expected = (string[])instance.Buildin();
            actual = (List<string>)instance.Executions;
            Assert.Equal(expected, actual);
            instance.Executions.Clear();

            expected = await (Task<string[]>)instance.TempPropAsync();
            actual = (List<string>)instance.Executions;
            Assert.Equal(expected, actual);
            instance.Executions.Clear();
        }
    }
}
