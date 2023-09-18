using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using PatternUsage.Attributes.Methods;
using PatternUsage.Attributes.Executions;
using PatternUsage.Attributes.Getters;
using PatternUsage.Attributes.Properties;
using PatternUsage.Attributes.Setters;
using System.Collections;
using System;

namespace Rougamo.Fody.Tests
{
    public class PatternTests : TestBase
    {
        public PatternTests() : base("PatternUsage.dll")
        {
        }

        protected override string RootNamespace => "PatternUsage";

        [Fact]
        public async Task MethodGlobalModifierTest()
        {
            var instance = GetInstance("Public");
            var executedMos = new List<string>();

            executedMos.Clear();
            instance.Instance(executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            instance.Call("PrivateInstance", executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            instance.Call("ProtectedInstance", executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            instance.Call("InternalInstanceXyz", executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("P2InstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask<int>)instance.Call("PiInstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("StaticAsync", executedMos);
            Assert.Contains(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask)instance.Call("PrivateStaticAsync", executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);
        }

        [Fact]
        public async Task MethodIRougamoTest()
        {
            var instance = GetInstance("Public");
            var executedMos = new List<string>();

            executedMos.Clear();
            instance.Instance(executedMos);
            Assert.DoesNotContain(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            instance.Call("PrivateInstance", executedMos);
            Assert.DoesNotContain(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            instance.Call("ProtectedInstance", executedMos);
            Assert.DoesNotContain(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            instance.Call("InternalInstanceXyz", executedMos);
            Assert.DoesNotContain(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("P2InstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask<int>)instance.Call("PiInstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("StaticAsync", executedMos);
            Assert.Contains(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask)instance.Call("PrivateStaticAsync", executedMos);
            Assert.Contains(nameof(StaticAttribute), executedMos);
        }

        [Fact]
        public async Task MethodFuzzyNameTest()
        {
            var instance = GetInstance("Public");
            var executedMos = new List<string>();

            executedMos.Clear();
            instance.Instance(executedMos);
            Assert.Contains(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            instance.Call("PrivateInstance", executedMos);
            Assert.Contains(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            instance.Call("ProtectedInstance", executedMos);
            Assert.Contains(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            instance.Call("InternalInstanceXyz", executedMos);
            Assert.DoesNotContain(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("P2InstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask<int>)instance.Call("PiInstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("StaticAsync", executedMos);
            Assert.DoesNotContain(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask)instance.Call("PrivateStaticAsync", executedMos);
            Assert.DoesNotContain(nameof(InstaceSuffixAttribute), executedMos);
        }

        [Fact]
        public async Task ExecutionChildFullyMatchTest()
        {
            var clsA = GetInstance("PatternUsage.ClsA", true);
            var clsAB = GetInstance("PatternUsage.ClsAB", true);
            var xClsA = GetInstance("PatternUsage.X.XClsA", true);
            var xClsAB = GetInstance("PatternUsage.X.XClsAB", true);
            var innerClsAB = GetInstance("PatternUsage.X.InnerClsAB", true);
            var pub = GetInstance("PatternUsage.Public", true);

            List<string> returnValue;
            var executedMos = new List<string>();

            returnValue = clsA.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), returnValue);
            executedMos.Clear();
            clsA.Call("set_PublicProp", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            returnValue = clsA.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), returnValue);
            executedMos.Clear();
            clsA.Call("set_StaticPrivateProp", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            clsA.Call("Protected", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsA.Call("StaticInternalAsync", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);

            returnValue = clsAB.Call("get_PublicProp", null);
            Assert.Contains(nameof(ChildOfInterfaceABAttribute), returnValue);
            executedMos.Clear();
            clsAB.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceABAttribute), executedMos);
            returnValue = clsAB.Call("get_StaticPrivateProp", null);
            Assert.Contains(nameof(ChildOfInterfaceABAttribute), returnValue);
            executedMos.Clear();
            clsAB.Call("set_StaticPrivateProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            clsAB.Call("Protected", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsAB.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceABAttribute), executedMos);

            returnValue = xClsA.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), returnValue);
            executedMos.Clear();
            xClsA.Call("set_PublicProp", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            returnValue = xClsA.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), returnValue);
            executedMos.Clear();
            xClsA.Call("set_StaticPrivateProp", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            xClsA.Call("Protected", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            await (Task)xClsA.Call("StaticInternalAsync", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);

            returnValue = xClsAB.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), returnValue);
            executedMos.Clear();
            xClsAB.Call("set_PublicProp", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            returnValue = xClsAB.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), returnValue);
            executedMos.Clear();
            xClsAB.Call("set_StaticPrivateProp", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            xClsAB.Call("Protected", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            await (Task)xClsAB.Call("StaticInternalAsync", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);

            returnValue = innerClsAB.Call("get_PublicProp", null);
            Assert.Contains(nameof(ChildOfInterfaceABAttribute), returnValue);
            executedMos.Clear();
            innerClsAB.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceABAttribute), executedMos);
            returnValue = innerClsAB.Call("get_StaticPrivateProp", null);
            Assert.Contains(nameof(ChildOfInterfaceABAttribute), returnValue);
            executedMos.Clear();
            innerClsAB.Call("set_StaticPrivateProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            innerClsAB.Call("Protected", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            await (Task)innerClsAB.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceABAttribute), executedMos);

            executedMos.Clear();
            pub.Instance(executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            pub.Call("PrivateInstance", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            pub.Call("ProtectedInstance", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            pub.Call("InternalInstanceXyz", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            await (Task)pub.Call("P2InstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            await (ValueTask<int>)pub.Call("PiInstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            await (Task)pub.Call("StaticAsync", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
            executedMos.Clear();
            await (ValueTask)pub.Call("PrivateStaticAsync", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceABAttribute), executedMos);
        }

        [Fact]
        public async Task ExecutionChildNameMatchTest()
        {
            var clsA = GetInstance("PatternUsage.ClsA", true);
            var clsAB = GetInstance("PatternUsage.ClsAB", true);
            var xClsA = GetInstance("PatternUsage.X.XClsA", true);
            var xClsAB = GetInstance("PatternUsage.X.XClsAB", true);
            var innerClsAB = GetInstance("PatternUsage.X.InnerClsAB", true);
            var pub = GetInstance("PatternUsage.Public", true);

            List<string> returnValue;
            var executedMos = new List<string>();

            returnValue = clsA.Call("get_PublicProp", null);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), returnValue);
            executedMos.Clear();
            clsA.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            returnValue = clsA.Call("get_StaticPrivateProp", null);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), returnValue);
            executedMos.Clear();
            clsA.Call("set_StaticPrivateProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            clsA.Call("Protected", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsA.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);

            returnValue = clsAB.Call("get_PublicProp", null);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), returnValue);
            executedMos.Clear();
            clsAB.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            returnValue = clsAB.Call("get_StaticPrivateProp", null);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), returnValue);
            executedMos.Clear();
            clsAB.Call("set_StaticPrivateProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            clsAB.Call("Protected", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsAB.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);

            returnValue = xClsA.Call("get_PublicProp", null);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), returnValue);
            executedMos.Clear();
            xClsA.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            returnValue = xClsA.Call("get_StaticPrivateProp", null);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), returnValue);
            executedMos.Clear();
            xClsA.Call("set_StaticPrivateProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            xClsA.Call("Protected", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            await (Task)xClsA.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);

            returnValue = xClsAB.Call("get_PublicProp", null);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), returnValue);
            executedMos.Clear();
            xClsAB.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            returnValue = xClsAB.Call("get_StaticPrivateProp", null);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), returnValue);
            executedMos.Clear();
            xClsAB.Call("set_StaticPrivateProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            xClsAB.Call("Protected", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            await (Task)xClsAB.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);

            returnValue = innerClsAB.Call("get_PublicProp", null);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), returnValue);
            executedMos.Clear();
            innerClsAB.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            returnValue = innerClsAB.Call("get_StaticPrivateProp", null);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), returnValue);
            executedMos.Clear();
            innerClsAB.Call("set_StaticPrivateProp", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            innerClsAB.Call("Protected", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            await (Task)innerClsAB.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);

            executedMos.Clear();
            pub.Instance(executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            pub.Call("PrivateInstance", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            pub.Call("ProtectedInstance", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            pub.Call("InternalInstanceXyz", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            await (Task)pub.Call("P2InstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            await (ValueTask<int>)pub.Call("PiInstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            await (Task)pub.Call("StaticAsync", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
            executedMos.Clear();
            await (ValueTask)pub.Call("PrivateStaticAsync", executedMos);
            Assert.DoesNotContain(nameof(ChildOfInterfaceAWidlyAttribute), executedMos);
        }

        [Fact]
        public async Task GetterReturnsDictionaryGenericTest()
        {
            var clsA = GetInstance("PatternUsage.ClsA", true);
            var xClsAB = GetInstance("PatternUsage.X.XClsAB", true);

            List<string> listReturn;
            IDictionary imapReturn;
            var listArg = new List<string>();
            var mapArg = new Dictionary<string, string>();
            var sortedMapArg = new SortedList<string, string>();

            listReturn = clsA.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listReturn);
            listArg.Clear();
            clsA.Call("set_PublicProp", listArg);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listArg);
            listReturn = clsA.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listReturn);
            listArg.Clear();
            clsA.Call("set_StaticPrivateProp", listArg);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listArg);
            imapReturn = clsA.Call("get_ProtectedInternalProp", null);
            Assert.True(imapReturn.Contains(nameof(ReturnsDictionaryGenericAttribute)));
            mapArg.Clear();
            clsA.Call("set_ProtectedInternalProp", mapArg);
            Assert.False(mapArg.ContainsKey(nameof(ReturnsDictionaryGenericAttribute)));
            imapReturn = clsA.Call("get_DefaultProp", null);
            Assert.False(imapReturn.Contains(nameof(ReturnsDictionaryGenericAttribute)));
            sortedMapArg.Clear();
            clsA.Call("set_DefaultProp", sortedMapArg);
            Assert.False(sortedMapArg.ContainsKey(nameof(ReturnsDictionaryGenericAttribute)));
            listArg.Clear();
            clsA.Call("Protected", listArg);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listArg);
            listArg.Clear();
            await (Task)clsA.Call("StaticInternalAsync", listArg);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listArg);
            listReturn = clsA.Call("PrivateProtected", null);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listReturn);

            listReturn = xClsAB.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listReturn);
            listArg.Clear();
            xClsAB.Call("set_PublicProp", listArg);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listArg);
            listReturn = xClsAB.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listReturn);
            listArg.Clear();
            xClsAB.Call("set_StaticPrivateProp", listArg);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listArg);
            imapReturn = xClsAB.Call("get_ProtectedInternalProp", null);
            Assert.True(imapReturn.Contains(nameof(ReturnsDictionaryGenericAttribute)));
            mapArg.Clear();
            xClsAB.Call("set_ProtectedInternalProp", mapArg);
            Assert.False(mapArg.ContainsKey(nameof(ReturnsDictionaryGenericAttribute)));
            imapReturn = xClsAB.Call("get_DefaultProp", null);
            Assert.False(imapReturn.Contains(nameof(ReturnsDictionaryGenericAttribute)));
            sortedMapArg.Clear();
            xClsAB.Call("set_DefaultProp", sortedMapArg);
            Assert.False(sortedMapArg.ContainsKey(nameof(ReturnsDictionaryGenericAttribute)));
            listArg.Clear();
            xClsAB.Call("Protected", listArg);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listArg);
            listArg.Clear();
            await (Task)xClsAB.Call("StaticInternalAsync", listArg);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listArg);
            listReturn = xClsAB.Call("PrivateProtected", null);
            Assert.DoesNotContain(nameof(ReturnsDictionaryGenericAttribute), listReturn);
        }

        [Fact]
        public async Task SetterReturnsChildOfIDictionaryTest()
        {
            var clsA = GetInstance("PatternUsage.ClsA", true);
            var xClsAB = GetInstance("PatternUsage.X.XClsAB", true);

            List<string> listReturn;
            IDictionary imapReturn;
            var listArg = new List<string>();
            var mapArg = new Dictionary<string, string>();
            var sortedMapArg = new SortedList<string, string>();

            listReturn = clsA.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listReturn);
            listArg.Clear();
            clsA.Call("set_PublicProp", listArg);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listArg);
            listReturn = clsA.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listReturn);
            listArg.Clear();
            clsA.Call("set_StaticPrivateProp", listArg);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listArg);
            imapReturn = clsA.Call("get_ProtectedInternalProp", null);
            Assert.False(imapReturn.Contains(nameof(ReturnsChildOfIDictionaryAttribute)));
            mapArg.Clear();
            clsA.Call("set_ProtectedInternalProp", mapArg);
            Assert.True(mapArg.ContainsKey(nameof(ReturnsChildOfIDictionaryAttribute)));
            imapReturn = clsA.Call("get_DefaultProp", null);
            Assert.False(imapReturn.Contains(nameof(ReturnsChildOfIDictionaryAttribute)));
            sortedMapArg.Clear();
            clsA.Call("set_DefaultProp", sortedMapArg);
            Assert.True(sortedMapArg.ContainsKey(nameof(ReturnsChildOfIDictionaryAttribute)));
            listArg.Clear();
            clsA.Call("Protected", listArg);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listArg);
            listArg.Clear();
            await (Task)clsA.Call("StaticInternalAsync", listArg);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listArg);
            listReturn = clsA.Call("PrivateProtected", null);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listReturn);

            listReturn = xClsAB.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listReturn);
            listArg.Clear();
            xClsAB.Call("set_PublicProp", listArg);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listArg);
            listReturn = xClsAB.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listReturn);
            listArg.Clear();
            xClsAB.Call("set_StaticPrivateProp", listArg);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listArg);
            imapReturn = xClsAB.Call("get_ProtectedInternalProp", null);
            Assert.False(imapReturn.Contains(nameof(ReturnsChildOfIDictionaryAttribute)));
            mapArg.Clear();
            xClsAB.Call("set_ProtectedInternalProp", mapArg);
            Assert.True(mapArg.ContainsKey(nameof(ReturnsChildOfIDictionaryAttribute)));
            imapReturn = xClsAB.Call("get_DefaultProp", null);
            Assert.False(imapReturn.Contains(nameof(ReturnsChildOfIDictionaryAttribute)));
            sortedMapArg.Clear();
            xClsAB.Call("set_DefaultProp", sortedMapArg);
            Assert.True(sortedMapArg.ContainsKey(nameof(ReturnsChildOfIDictionaryAttribute)));
            listArg.Clear();
            xClsAB.Call("Protected", listArg);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listArg);
            listArg.Clear();
            await (Task)xClsAB.Call("StaticInternalAsync", listArg);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listArg);
            listReturn = xClsAB.Call("PrivateProtected", null);
            Assert.DoesNotContain(nameof(ReturnsChildOfIDictionaryAttribute), listReturn);
        }

        [Fact]
        public async Task PropertyReturnsChildOfDicOrListTest()
        {
            var clsA = GetInstance("PatternUsage.ClsA", true);
            var xClsAB = GetInstance("PatternUsage.X.XClsAB", true);

            List<string> listReturn;
            IDictionary imapReturn;
            var listArg = new List<string>();
            var mapArg = new Dictionary<string, string>();
            var sortedMapArg = new SortedList<string, string>();

            listReturn = clsA.Call("get_PublicProp", null);
            Assert.Contains(nameof(ReturnsChildOfDicOrListAttribute), listReturn);
            listArg.Clear();
            clsA.Call("set_PublicProp", listArg);
            Assert.Contains(nameof(ReturnsChildOfDicOrListAttribute), listArg);
            listReturn = clsA.Call("get_StaticPrivateProp", null);
            Assert.Contains(nameof(ReturnsChildOfDicOrListAttribute), listReturn);
            listArg.Clear();
            clsA.Call("set_StaticPrivateProp", listArg);
            Assert.Contains(nameof(ReturnsChildOfDicOrListAttribute), listArg);
            imapReturn = clsA.Call("get_ProtectedInternalProp", null);
            Assert.True(imapReturn.Contains(nameof(ReturnsChildOfDicOrListAttribute)));
            mapArg.Clear();
            clsA.Call("set_ProtectedInternalProp", mapArg);
            Assert.True(mapArg.ContainsKey(nameof(ReturnsChildOfDicOrListAttribute)));
            imapReturn = clsA.Call("get_DefaultProp", null);
            Assert.True(imapReturn.Contains(nameof(ReturnsChildOfDicOrListAttribute)));
            sortedMapArg.Clear();
            clsA.Call("set_DefaultProp", sortedMapArg);
            Assert.True(sortedMapArg.ContainsKey(nameof(ReturnsChildOfDicOrListAttribute)));
            listArg.Clear();
            clsA.Call("Protected", listArg);
            Assert.DoesNotContain(nameof(ReturnsChildOfDicOrListAttribute), listArg);
            listArg.Clear();
            await (Task)clsA.Call("StaticInternalAsync", listArg);
            Assert.DoesNotContain(nameof(ReturnsChildOfDicOrListAttribute), listArg);
            listReturn = clsA.Call("PrivateProtected", null);
            Assert.DoesNotContain(nameof(ReturnsChildOfDicOrListAttribute), listReturn);

            listReturn = xClsAB.Call("get_PublicProp", null);
            Assert.Contains(nameof(ReturnsChildOfDicOrListAttribute), listReturn);
            listArg.Clear();
            xClsAB.Call("set_PublicProp", listArg);
            Assert.Contains(nameof(ReturnsChildOfDicOrListAttribute), listArg);
            listReturn = xClsAB.Call("get_StaticPrivateProp", null);
            Assert.Contains(nameof(ReturnsChildOfDicOrListAttribute), listReturn);
            listArg.Clear();
            xClsAB.Call("set_StaticPrivateProp", listArg);
            Assert.Contains(nameof(ReturnsChildOfDicOrListAttribute), listArg);
            imapReturn = xClsAB.Call("get_ProtectedInternalProp", null);
            Assert.True(imapReturn.Contains(nameof(ReturnsChildOfDicOrListAttribute)));
            mapArg.Clear();
            xClsAB.Call("set_ProtectedInternalProp", mapArg);
            Assert.True(mapArg.ContainsKey(nameof(ReturnsChildOfDicOrListAttribute)));
            imapReturn = xClsAB.Call("get_DefaultProp", null);
            Assert.True(imapReturn.Contains(nameof(ReturnsChildOfDicOrListAttribute)));
            sortedMapArg.Clear();
            xClsAB.Call("set_DefaultProp", sortedMapArg);
            Assert.True(sortedMapArg.ContainsKey(nameof(ReturnsChildOfDicOrListAttribute)));
            listArg.Clear();
            xClsAB.Call("Protected", listArg);
            Assert.DoesNotContain(nameof(ReturnsChildOfDicOrListAttribute), listArg);
            listArg.Clear();
            await (Task)xClsAB.Call("StaticInternalAsync", listArg);
            Assert.DoesNotContain(nameof(ReturnsChildOfDicOrListAttribute), listArg);
            listReturn = xClsAB.Call("PrivateProtected", null);
            Assert.DoesNotContain(nameof(ReturnsChildOfDicOrListAttribute), listReturn);
        }

        [Fact]
        public async Task MethodAnyMethodGenericTest()
        {
            var clsAB = GetInstance("PatternUsage.ClsAB", true);
            var xClsA = GetInstance("PatternUsage.X.XClsA", true);

            List<string> listReturn;
            var listArg = new List<string>();

            listReturn = clsAB.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(AnyMethodGenericAttribute), listReturn);
            listArg.Clear();
            clsAB.Call("set_PublicProp", listArg);
            Assert.DoesNotContain(nameof(AnyMethodGenericAttribute), listArg);
            listReturn = clsAB.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(AnyMethodGenericAttribute), listReturn);
            listArg.Clear();
            clsAB.Call("set_StaticPrivateProp", listArg);
            Assert.DoesNotContain(nameof(AnyMethodGenericAttribute), listArg);
            listArg.Clear();
            clsAB.Call("Protected", listArg);
            Assert.DoesNotContain(nameof(AnyMethodGenericAttribute), listArg);
            listArg.Clear();
            await (Task)clsAB.Call("StaticInternalAsync", listArg);
            Assert.DoesNotContain(nameof(AnyMethodGenericAttribute), listArg);
            listArg.Clear();
            await (Task)clsAB.Call("PublicSingleGenericAsync", listArg, typeof(int));
            Assert.Contains(nameof(AnyMethodGenericAttribute), listArg);
            listArg.Clear();
            clsAB.Call("ProtectedStaticDoubleGeneric", listArg, typeof(DateTime), typeof(Guid));
            Assert.Contains(nameof(AnyMethodGenericAttribute), listArg);

            listReturn = xClsA.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(AnyMethodGenericAttribute), listReturn);
            listArg.Clear();
            xClsA.Call("set_PublicProp", listArg);
            Assert.DoesNotContain(nameof(AnyMethodGenericAttribute), listArg);
            listReturn = xClsA.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(AnyMethodGenericAttribute), listReturn);
            listArg.Clear();
            xClsA.Call("set_StaticPrivateProp", listArg);
            Assert.DoesNotContain(nameof(AnyMethodGenericAttribute), listArg);
            listArg.Clear();
            xClsA.Call("Protected", listArg);
            Assert.DoesNotContain(nameof(AnyMethodGenericAttribute), listArg);
            listArg.Clear();
            await (Task)xClsA.Call("StaticInternalAsync", listArg);
            Assert.DoesNotContain(nameof(AnyMethodGenericAttribute), listArg);
            listArg.Clear();
            await (Task)xClsA.Call("PublicSingleGenericAsync", listArg, typeof(int));
            Assert.Contains(nameof(AnyMethodGenericAttribute), listArg);
            listArg.Clear();
            xClsA.Call("ProtectedStaticDoubleGeneric", listArg, typeof(DateTime), typeof(Guid));
            Assert.Contains(nameof(AnyMethodGenericAttribute), listArg);
        }

        [Fact]
        public async Task ExecutionSingleMethodGenericTest()
        {
            var clsAB = GetInstance("PatternUsage.ClsAB", true);
            var xClsA = GetInstance("PatternUsage.X.XClsA", true);

            List<string> listReturn;
            var listArg = new List<string>();

            listReturn = clsAB.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listReturn);
            listArg.Clear();
            clsAB.Call("set_PublicProp", listArg);
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listArg);
            listReturn = clsAB.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listReturn);
            listArg.Clear();
            clsAB.Call("set_StaticPrivateProp", listArg);
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listArg);
            listArg.Clear();
            clsAB.Call("Protected", listArg);
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listArg);
            listArg.Clear();
            await (Task)clsAB.Call("StaticInternalAsync", listArg);
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listArg);
            listArg.Clear();
            await (Task)clsAB.Call("PublicSingleGenericAsync", listArg, typeof(int));
            Assert.Contains(nameof(SingleMethodGenericAttribute), listArg);
            listArg.Clear();
            clsAB.Call("ProtectedStaticDoubleGeneric", listArg, typeof(DateTime), typeof(Guid));
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listArg);

            listReturn = xClsA.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listReturn);
            listArg.Clear();
            xClsA.Call("set_PublicProp", listArg);
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listArg);
            listReturn = xClsA.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listReturn);
            listArg.Clear();
            xClsA.Call("set_StaticPrivateProp", listArg);
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listArg);
            listArg.Clear();
            xClsA.Call("Protected", listArg);
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listArg);
            listArg.Clear();
            await (Task)xClsA.Call("StaticInternalAsync", listArg);
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listArg);
            listArg.Clear();
            await (Task)xClsA.Call("PublicSingleGenericAsync", listArg, typeof(int));
            Assert.Contains(nameof(SingleMethodGenericAttribute), listArg);
            listArg.Clear();
            xClsA.Call("ProtectedStaticDoubleGeneric", listArg, typeof(DateTime), typeof(Guid));
            Assert.DoesNotContain(nameof(SingleMethodGenericAttribute), listArg);
        }

        [Fact]
        public async Task ExecutionDoubleMethodGenericTest()
        {
            var clsAB = GetInstance("PatternUsage.ClsAB", true);
            var xClsA = GetInstance("PatternUsage.X.XClsA", true);

            List<string> listReturn;
            var listArg = new List<string>();

            listReturn = clsAB.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listReturn);
            listArg.Clear();
            clsAB.Call("set_PublicProp", listArg);
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listArg);
            listReturn = clsAB.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listReturn);
            listArg.Clear();
            clsAB.Call("set_StaticPrivateProp", listArg);
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listArg);
            listArg.Clear();
            clsAB.Call("Protected", listArg);
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listArg);
            listArg.Clear();
            await (Task)clsAB.Call("StaticInternalAsync", listArg);
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listArg);
            listArg.Clear();
            await (Task)clsAB.Call("PublicSingleGenericAsync", listArg, typeof(int));
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listArg);
            listArg.Clear();
            clsAB.Call("ProtectedStaticDoubleGeneric", listArg, typeof(DateTime), typeof(Guid));
            Assert.Contains(nameof(DoubleMethodGenericAttribute), listArg);

            listReturn = xClsA.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listReturn);
            listArg.Clear();
            xClsA.Call("set_PublicProp", listArg);
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listArg);
            listReturn = xClsA.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listReturn);
            listArg.Clear();
            xClsA.Call("set_StaticPrivateProp", listArg);
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listArg);
            listArg.Clear();
            xClsA.Call("Protected", listArg);
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listArg);
            listArg.Clear();
            await (Task)xClsA.Call("StaticInternalAsync", listArg);
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listArg);
            listArg.Clear();
            await (Task)xClsA.Call("PublicSingleGenericAsync", listArg, typeof(int));
            Assert.DoesNotContain(nameof(DoubleMethodGenericAttribute), listArg);
            listArg.Clear();
            xClsA.Call("ProtectedStaticDoubleGeneric", listArg, typeof(DateTime), typeof(Guid));
            Assert.Contains(nameof(DoubleMethodGenericAttribute), listArg);
        }

        [Fact]
        public async Task ExecutionSingleTypeGenericTest()
        {

        }

        [Fact]
        public async Task ExecutionDoubleTypeGenericTest()
        {

        }

        [Fact]
        public async Task MethodNestedTypeTest()
        {

        }

        [Fact]
        public async Task MethodAsyncSyntaxTest()
        {

        }

        [Fact]
        public async Task ExecutionTupleSyntaxTest()
        {

        }

        [Fact]
        public async Task ExecutionNullableSyntaxTest()
        {

        }

        [Fact]
        public async Task RegexTest()
        {

        }
    }
}
