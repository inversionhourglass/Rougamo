using BasicUsage;
using BasicUsage.Attributes;
using System;
using System.Collections;
using System.Threading.Tasks;
using Xunit;

namespace Rougamo.Fody.Tests
{
    public class BasicTest : TestBase
    {
        public BasicTest()
        {
            WeaveAssembly("BasicUsage.dll");
        }

        [Fact]
        public void InstanceSingleMethodTest()
        {
            var instance = GetInstance("BasicUsage.InstanceSingleMethod");
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
            var instance = GetInstance("BasicUsage.AsyncInstanceSingleMethod");
            var arrArg = new object[] { Guid.NewGuid(), 1.2, new object() };
            await (Task)instance.EntryAsync(1, "2", arrArg);
            Assert.Equal(instance.Context.Arguments.Length, 3);
            for (var i = 0; i < arrArg.Length; i++)
            {
                Assert.Equal(arrArg[i], instance.Context.Arguments[2][i]);
            }
            Assert.True(instance.Context.IsAsync);
            Assert.False(instance.Context.IsIterator);

            instance.Context = null;
            await Assert.ThrowsAsync<InvalidOperationException>(() => (Task)instance.ExceptionAsync());
            Assert.NotNull(instance.Context.Exception);
            // Combine the following two steps will throws an exception.
            IDictionary data = instance.Context.Exception.Data;
            Assert.Equal(1, data.Count);

            instance.Context = null;
            var successValue = await (Task<int>)instance.SuccessAsync();
            Assert.Equal(successValue, instance.Context.ReturnValue);

            instance.Context = null;
            await Assert.ThrowsAsync<InvalidOperationException>(() => (Task)instance.ExitWithExceptionAsync());
            Assert.Null(instance.Context.ReturnValue);
            Assert.NotNull(instance.Context.Exception);

            instance.Context = null;
            var exitWithSuccessValue = await (Task<string>)instance.ExitWithSuccessAsync();
            Assert.Equal(exitWithSuccessValue, instance.Context.ReturnValue);
            Assert.Null(instance.Context.Exception);
        }

        [Fact]
        public void ModifyReturnValueTest()
        {
            var originInstance = new ModifyReturnValue();
            var instance = GetInstance("BasicUsage.ModifyReturnValue");

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
        }

        [Fact]
        public async Task AsyncModifyReturnValueTest()
        {
            var originInstance = new AsyncModifyReturnValue();
            var instance = GetInstance("BasicUsage.AsyncModifyReturnValue");

            await Assert.ThrowsAsync<InvalidOperationException>(() => originInstance.ExceptionAsync());
            var exceptionHandledValue = await (Task<string>)instance.ExceptionAsync();
            Assert.Equal(ExceptionHandleAttribute.StringValue, exceptionHandledValue);

            await Assert.ThrowsAsync<InvalidOperationException>(() => originInstance.ExceptionWithUnboxAsync());
            var exceptionHandledUnboxValue = await (Task<int>)instance.ExceptionWithUnboxAsync();
            Assert.Equal(ExceptionHandleAttribute.IntValue, exceptionHandledUnboxValue);

            await Assert.ThrowsAsync<InvalidOperationException>(() => originInstance.ExceptionUnhandledAsync());
            await Assert.ThrowsAsync<InvalidOperationException>(() => (Task)instance.ExceptionUnhandledAsync());

            var args = new object[] { 1, '-', nameof(ModifyReturnValueTest), 3.14 };
            var originSuccessValue = await originInstance.SucceededAsync(args);
            var replacedSuccessValue = await (Task<string>)instance.SucceededAsync(args);
            Assert.NotEqual(originSuccessValue, replacedSuccessValue);
            Assert.Equal(ReturnValueReplaceAttribute.StringValue, replacedSuccessValue);

            var originUnboxValue = await originInstance.SucceededWithUnboxAsync();
            var replacedUnboxValue = await (Task<int>)instance.SucceededWithUnboxAsync();
            Assert.NotEqual(originUnboxValue, replacedUnboxValue);
            Assert.Equal(ReturnValueReplaceAttribute.IntValue, replacedUnboxValue);

            var originValue = await originInstance.SucceededUnrecognizedAsync();
            var unrecognizedValue = await (Task<double>)instance.SucceededUnrecognizedAsync();
            Assert.Equal(originValue, unrecognizedValue);
        }
    }
}
