using ConfiguredMoUsage;
using IndependentMos;
using IndependentMos.Attributes;
using IndependentMos.Structs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Rougamo.Fody.Tests
{
    public class ConfiguredMoTests
    {
        private static readonly WeavedAssembly Assembly;

        private const string CONFIG = """
            <Rougamo>
              <Mos>
                <Mo assembly="IndependentMos" type="IndependentMos.Attributes.Public1Attribute" />
                <Mo assembly="IndependentMos" type="IndependentMos.Attributes.Public2Attribute" pattern="method(static * *(..))" />
                <Mo assembly="IndependentMos" type="IndependentMos.Structs.MethodStartsWithGet1" />
                <Mo assembly="IndependentMos" type="IndependentMos.Structs.MethodStartsWithGet2" pattern="method(!static * *.Get*(..))" />
                <Mo assembly="IndependentMos" type="IndependentMos.Default" pattern="method(* *.Async*(..))" />
              </Mos>
            </Rougamo>
            """;

        static ConfiguredMoTests()
        {
            Assembly = new("ConfiguredMoUsage", CONFIG);
        }

        [Fact]
        public async Task Test()
        {
            var iInstanceCls = Assembly.GetInstance(nameof(InstanceCls));
            var sInstanceCls = Assembly.GetStaticInstance(nameof(InstanceCls));
            var sStaticCls = Assembly.GetStaticInstance(nameof(StaticCls));

            var executedMos = new List<string>();

            executedMos.Clear();
            iInstanceCls.Sync(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName], executedMos);

            executedMos.Clear();
            iInstanceCls.SyncReturnInt32(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName], executedMos);

            executedMos.Clear();
            iInstanceCls.GetX(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(MethodStartsWithGet1).FullName, typeof(MethodStartsWithGet2).FullName], executedMos);

            executedMos.Clear();
            await (Task)iInstanceCls.AsyncTask(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Default).FullName], executedMos);

            executedMos.Clear();
            await (Task<string>)iInstanceCls.AsyncTaskReturnString(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Default).FullName], executedMos);

            executedMos.Clear();
            await (ValueTask)iInstanceCls.AsyncValueTask(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Default).FullName], executedMos);

            executedMos.Clear();
            await (ValueTask<object>)iInstanceCls.AsyncValueTaskReturnObject(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Default).FullName], executedMos);

            executedMos.Clear();
            sInstanceCls.SyncStatic(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName], executedMos);

            executedMos.Clear();
            sInstanceCls.SyncStaticReturnDouble(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName], executedMos);

            executedMos.Clear();
            sInstanceCls.GetY(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName, typeof(MethodStartsWithGet1).FullName], executedMos);

            executedMos.Clear();
            await (Task)sInstanceCls.AsyncStaticTask(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName, typeof(Default).FullName], executedMos);

            executedMos.Clear();
            await (Task<DateTime>)sInstanceCls.AsyncStaticTaskReturnDateTime(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName, typeof(Default).FullName], executedMos);

            executedMos.Clear();
            await (ValueTask)sInstanceCls.AsyncStaticValueTask(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName, typeof(Default).FullName], executedMos);

            executedMos.Clear();
            await (ValueTask<Guid>)sInstanceCls.AsyncStaticValueTaskReturnGuid(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName, typeof(Default).FullName], executedMos);

            executedMos.Clear();
            sStaticCls.SyncStatic(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName], executedMos);

            executedMos.Clear();
            sStaticCls.SyncStaticReturnDouble(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName], executedMos);

            executedMos.Clear();
            sStaticCls.GetY(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName, typeof(MethodStartsWithGet1).FullName], executedMos);

            executedMos.Clear();
            await (Task)sStaticCls.AsyncStaticTask(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName, typeof(Default).FullName], executedMos);

            executedMos.Clear();
            await (Task<DateTime>)sStaticCls.AsyncStaticTaskReturnDateTime(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName, typeof(Default).FullName], executedMos);

            executedMos.Clear();
            await (ValueTask)sStaticCls.AsyncStaticValueTask(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName, typeof(Default).FullName], executedMos);

            executedMos.Clear();
            await (ValueTask<Guid>)sStaticCls.AsyncStaticValueTaskReturnGuid(executedMos);
            Assert.Equal([typeof(Public1Attribute).FullName, typeof(Public2Attribute).FullName, typeof(Default).FullName], executedMos);
        }
    }
}
