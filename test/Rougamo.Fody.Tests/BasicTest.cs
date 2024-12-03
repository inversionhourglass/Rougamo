using BasicUsage;
using BasicUsage.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rougamo.Fody.Tests
{
    public partial class BasicTest
    {
        private static readonly WeavedAssembly Assembly;

        static BasicTest()
        {
            //#if NET461
            //Assembly = new("../../Release/net461/BasicUsage.dll", "BasicUsage");
            //#elif NET48
            //Assembly = new("../../Release/net48/BasicUsage.dll", "BasicUsage");
            //#elif NETCOREAPP3_1
            //Assembly = new("../../Release/netcoreapp3.1/BasicUsage.dll", "BasicUsage");
            //#elif NET6_0
            //Assembly = new("../../Release/net6.0/BasicUsage.dll", "BasicUsage");
            //#elif NET7_0
            //Assembly = new("../../Release/net7.0/BasicUsage.dll", "BasicUsage");
            //#elif NET8_0
            //Assembly = new("../../Release/net8.0/BasicUsage.dll", "BasicUsage");
            //#elif NET9_0
            //Assembly = new("../../Release/net9.0/BasicUsage.dll", "BasicUsage");
            //#endif
            Assembly = new("BasicUsage");
        }

        [Fact]
        public async Task MethodPropertyTest()
        {
            var instance = Assembly.GetInstance(nameof(AccessableUseCase));
            var staticInstance = Assembly.GetStaticInstance(nameof(AccessableUseCase));
            var type = (Type)instance.GetType();

            var recording = (List<string>)instance.Recording;
            var staticRecording = (List<string>)type.GetProperty("StaticRecording").GetValue(null);

            recording.Clear();
            staticRecording.Clear();
            instance.IntItem = 2;
            Assert.Equal(AccessableUseCase.Expected_Property_Set, recording);

            recording.Clear();
            staticRecording.Clear();
            var intItem = (int)instance.IntItem;
            Assert.Equal(AccessableUseCase.Expected_Property_Get, recording);
            Assert.NotEqual(int.MinValue, intItem);

            recording.Clear();
            staticRecording.Clear();
            instance.Transfer(3);
            Assert.Equal(AccessableUseCase.Expected_Method, recording);

            recording.Clear();
            staticRecording.Clear();
            type.GetProperty("StringItem").SetValue(null, "ok");
            Assert.Equal(AccessableUseCase.Expected_Property_Set, staticRecording);

            recording.Clear();
            staticRecording.Clear();
            var stringItem = (string)type.GetProperty("StringItem").GetValue(null);
            Assert.Equal(AccessableUseCase.Expected_Property_Get, staticRecording);
            Assert.NotNull(stringItem);

            recording.Clear();
            staticRecording.Clear();
            await (Task<int>)staticInstance.TransferAsync(1);
            Assert.Equal(AccessableUseCase.Expected_Method, staticRecording);
        }

        [Fact]
        public void ModifyReturnValueTest()
        {
            var originInstance = new ModifyReturnValue();
            var instance = Assembly.GetInstance("ModifyReturnValue");

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
            var instance = Assembly.GetInstance("AsyncModifyReturnValue");

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
            var instance = Assembly.GetInstance("ModifyIteratorReturnValue");

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
            var instance = Assembly.GetInstance("SyncExecution");

            string flag;
            flag = instance.Call("get_InstanceProp1", null);
            Assert.Equal(CtorInitAttribute.FLAG, flag);
            flag = instance.Call("get_InstanceProp2", null);
            Assert.Equal(CcctorInitAttribute.FLAG, flag);
            flag = instance.Call("get_StaticProp1", null);
            Assert.Equal(CctorInitAttribute.FLAG, flag);
            flag = instance.Call("get_StaticProp2", null);
            Assert.Equal(CcctorInitAttribute.FLAG, flag);

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
            var instance = Assembly.GetInstance("AsyncExecution");

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
            var instance = Assembly.GetInstance(nameof(ModifyArguments));
            var attribute = Assembly.GetStaticInstance(typeof(RewriteArgsAttribute).FullName, true);

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
            var instance = Assembly.GetInstance(nameof(SyncExecution));
            var asyncInstance = Assembly.GetInstance(nameof(AsyncExecution));

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
            var instance = Assembly.GetInstance(nameof(OrderUseCase));

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

        [Fact]
        public void FreshArgsTest()
        {
            var instance = Assembly.GetInstance(nameof(FreshArguments));

            var x = 0;
            var y = 0;
            var s = string.Empty;
            var ls = new List<int>();

            Tuple<object[], object[]> value;

            value = instance.FreshTest(x, out y);
            Assert.NotEqual(value.Item1, value.Item2);

            value = instance.NonFreshTest(x, out y);
            Assert.Equal(value.Item1, value.Item2);
        }

        [Fact]
        public async Task RougamoTest()
        {
            var instance = Assembly.GetInstance(nameof(RougamoUsage));

            var executedMos = new List<string>();

            executedMos.Clear();
            instance.M(executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.Mo), executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueMo), executedMos);

            executedMos.Clear();
            await (Task)instance.MAsync(executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.Mo), executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueMo), executedMos);
        }

        [Fact]
        public async Task GenericMoTest()
        {
            var instance = Assembly.GetInstance(nameof(GenericMoUseCase));

            var executedMos = new List<string>();

            executedMos.Clear();
            instance.M(executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueMo), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueMo1), executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueMo2), executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueMo3), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.Mo), executedMos);

            executedMos.Clear();
            await (ValueTask)instance.MAsync(executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueMo), executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueMo1), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueMo2), executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueMo3), executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.Mo), executedMos);
        }

        [Fact]
        public async Task OmitTest()
        {
            var instance = Assembly.GetInstance(nameof(OmitUseCase));
            var sInstance = Assembly.GetStaticInstance(nameof(OmitUseCase));

            var executedMos = new List<string>();
            List<string> returnMos;

            executedMos.Clear();
            instance.Mos(executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueOmitMos), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArguments), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArgumentsButFeature), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitAll), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ClassOmitMos), executedMos);

            returnMos = instance.Arguments(0);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitMos), returnMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueOmitArguments), returnMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArgumentsButFeature), returnMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitAll), returnMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ClassOmitMos), returnMos);

            returnMos = instance.ArgumentsStillSucceed(0);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitMos), returnMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArguments), returnMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueOmitArgumentsButFeature), returnMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitAll), returnMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ClassOmitMos), returnMos);

            returnMos = sInstance.All(0);
            Assert.Null(returnMos);

            executedMos.Clear();
            await (Task)sInstance.MosAsync(executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueOmitMos), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArguments), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArgumentsButFeature), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitAll), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ClassOmitMos), executedMos);

            returnMos = await (Task<List<string>>)sInstance.ArgumentsAsync(0);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitMos), returnMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueOmitArguments), returnMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArgumentsButFeature), returnMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitAll), returnMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ClassOmitMos), returnMos);

            returnMos = await (Task<List<string>>)sInstance.ArgumentsStillSucceedAsync(0);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitMos), returnMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArguments), returnMos);
            Assert.Contains(nameof(BasicUsage.Mos.ValueOmitArgumentsButFeature), returnMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitAll), returnMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ClassOmitMos), returnMos);

            returnMos = await (ValueTask<List<string>>)instance.AllAsync(0);
            Assert.Null(returnMos);

            executedMos.Clear();
            ((IEnumerable<string>)instance.Iterator(executedMos)).ToArray();
            Assert.Contains(nameof(BasicUsage.Mos.ValueOmitMos), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArguments), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArgumentsButFeature), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitAll), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ClassOmitMos), executedMos);

            executedMos.Clear();
            await ((IAsyncEnumerable<string>)instance.IteratorAsync(executedMos)).ToArrayAsync();
            Assert.Contains(nameof(BasicUsage.Mos.ValueOmitMos), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArguments), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArgumentsButFeature), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitAll), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ClassOmitMos), executedMos);

            executedMos.Clear();
            instance.Cls(executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitMos), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArguments), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArgumentsButFeature), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitAll), executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.ClassOmitMos), executedMos);

            executedMos.Clear();
            await (Task)instance.ClsAsync(executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitMos), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArguments), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitArgumentsButFeature), executedMos);
            Assert.DoesNotContain(nameof(BasicUsage.Mos.ValueOmitAll), executedMos);
            Assert.Contains(nameof(BasicUsage.Mos.ClassOmitMos), executedMos);
        }

        [Fact]
        public async Task IndirectDependencyTest()
        {
            var instance = Assembly.GetInstance(nameof(IndirectDependencyUseCase));
            var sInstance = Assembly.GetStaticInstance(nameof(IndirectDependencyUseCase));

            var mos = new List<string>();

            mos.Clear();
            var syncResult = (string[])instance.Sync(mos);
            Assert.Equal(syncResult, mos);

            mos.Clear();
            var asyncResult = await (Task<string[]>)sInstance.Async(mos);
            Assert.Equal(asyncResult, mos);
        }

        [Fact]
        public void CtorTest()
        {
            var sEmptyInstance = Assembly.GetStaticInstance(nameof(ConstructorEmpty));
            var sThrowsInstance = Assembly.GetStaticInstance(nameof(ConstructorThrows));
            var sTryCatchInstance = Assembly.GetStaticInstance(nameof(ConstructorTryCatch));

            string[] expected = ["CtorValueMo"];
            Assert.Equal(expected, sEmptyInstance.GetExecutedMos());
            Assert.Equal(expected, sThrowsInstance.GetExecutedMos());
            Assert.Equal(expected, sTryCatchInstance.GetExecutedMos());

            var executedMos = new List<string>();

            executedMos.Clear();
            var emptyInstance = Assembly.GetInstance(nameof(ConstructorEmpty), false, null, [executedMos]);
            Assert.Equal(executedMos, executedMos);

            executedMos.Clear();
            var throwsInstance = Assembly.GetInstance(nameof(ConstructorThrows), false, null, [executedMos]);
            Assert.Equal(executedMos, executedMos);

            executedMos.Clear();
            var tryCatchInstance = Assembly.GetInstance(nameof(ConstructorTryCatch), false, null, [executedMos]);
            Assert.Equal(executedMos, executedMos);
        }

        [Fact]
        public async Task LifetimeTest()
        {
            var instance = Assembly.GetInstance(nameof(LifetimeUseCase));

            var list = new List<object>();

            #region Singleton
            list.Clear();
            instance.SingletonNested(list);
            Assert.Equal(2, list.Count);
            Assert.Same(list[0], list[1]);
            instance.Singleton(list);
            Assert.Equal(3, list.Count);
            Assert.Same(list[0], list[2]);

            list.Clear();
            await (Task)instance.SingletonNestedAsync(list);
            Assert.Equal(2, list.Count);
            Assert.Same(list[0], list[1]);
            await (Task)instance.SingletonAsync(list);
            Assert.Equal(3, list.Count);
            Assert.Same(list[0], list[2]);

            list.Clear();
            ((IEnumerable<int>)instance.SingletonNestedIterator(list)).ToArray();
            Assert.Equal(2, list.Count);
            Assert.Same(list[0], list[1]);
            ((IEnumerable<int>)instance.SingletonIterator(list)).ToArray();
            Assert.Equal(3, list.Count);
            Assert.Same(list[0], list[2]);

            list.Clear();
            await ((IAsyncEnumerable<int>)instance.SingletonNestedAiterator(list)).ToArrayAsync();
            Assert.Equal(2, list.Count);
            Assert.Same(list[0], list[1]);
            await ((IAsyncEnumerable<int>)instance.SingletonAiterator(list)).ToArrayAsync();
            Assert.Equal(3, list.Count);
            Assert.Same(list[0], list[2]);
            #endregion Singleton

            #region Pooled
            list.Clear();
            instance.PooledNested(list);
            Assert.Equal(2, list.Count);
            Assert.NotSame(list[0], list[1]);
            instance.Pooled(list);
            Assert.Equal(3, list.Count);
            Assert.True(list[0] == list[2] || list[1] == list[2]);

            list.Clear();
            await (Task)instance.PooledNestedAsync(list);
            Assert.Equal(2, list.Count);
            Assert.NotSame(list[0], list[1]);
            await (Task)instance.PooledAsync(list);
            Assert.Equal(3, list.Count);
            Assert.True(list[0] == list[2] || list[1] == list[2]);

            list.Clear();
            ((IEnumerable<int>)instance.PooledNestedIterator(list)).ToArray();
            Assert.Equal(2, list.Count);
            Assert.NotSame(list[0], list[1]);
            ((IEnumerable<int>)instance.PooledIterator(list)).ToArray();
            Assert.Equal(3, list.Count);
            Assert.True(list[0] == list[2] || list[1] == list[2]);

            list.Clear();
            await ((IAsyncEnumerable<int>)instance.PooledNestedAiterator(list)).ToArrayAsync();
            Assert.Equal(2, list.Count);
            Assert.NotSame(list[0], list[1]);
            await ((IAsyncEnumerable<int>)instance.PooledAiterator(list)).ToArrayAsync();
            Assert.Equal(3, list.Count);
            Assert.True(list[0] == list[2] || list[1] == list[2]);
            #endregion Pooled

            #region Transient
            list.Clear();
            instance.TransientNested(list);
            Assert.Equal(2, list.Count);
            Assert.NotSame(list[0], list[1]);
            instance.Transient(list);
            Assert.Equal(3, list.Count);
            Assert.NotSame(list[0], list[2]);
            Assert.NotSame(list[1], list[2]);

            list.Clear();
            await (Task)instance.TransientNestedAsync(list);
            Assert.Equal(2, list.Count);
            Assert.NotSame(list[0], list[1]);
            await (Task)instance.TransientAsync(list);
            Assert.Equal(3, list.Count);
            Assert.NotSame(list[0], list[2]);
            Assert.NotSame(list[1], list[2]);

            list.Clear();
            ((IEnumerable<int>)instance.TransientNestedIterator(list)).ToArray();
            Assert.Equal(2, list.Count);
            Assert.NotSame(list[0], list[1]);
            ((IEnumerable<int>)instance.TransientIterator(list)).ToArray();
            Assert.Equal(3, list.Count);
            Assert.NotSame(list[0], list[2]);
            Assert.NotSame(list[1], list[2]);

            list.Clear();
            await ((IAsyncEnumerable<int>)instance.TransientNestedAiterator(list)).ToArrayAsync();
            Assert.Equal(2, list.Count);
            Assert.NotSame(list[0], list[1]);
            await ((IAsyncEnumerable<int>)instance.TransientAiterator(list)).ToArrayAsync();
            Assert.Equal(3, list.Count);
            Assert.NotSame(list[0], list[2]);
            Assert.NotSame(list[1], list[2]);
            #endregion Transient

            list.Clear();
            instance.GenericPooled(list);
            Assert.Single(list);
            await (Task)instance.GenericPooledAsync(list);
            Assert.Equal(2, list.Count);
            Assert.Same(list[0], list[1]);
        }
    }
}
