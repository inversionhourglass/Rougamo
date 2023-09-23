using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using PatternUsage.Attributes;
using PatternUsage.Attributes.Methods;
using PatternUsage.Attributes.Executions;
using PatternUsage.Attributes.Getters;
using PatternUsage.Attributes.Properties;
using PatternUsage.Attributes.Setters;
using System.Collections;
using System;
using PatternUsage.Attributes.Regexes;

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
            var singleGeneric = GetInstance("PatternUsage.SingleGeneric`1", true, x => x.MakeGenericType(typeof(int)));
            var doubleGeneric = GetInstance("PatternUsage.X.DoubleGeneric`2", true, x => x.MakeGenericType(typeof(string), typeof(double)));
            var clsAB = GetInstance("PatternUsage.ClsAB", true);

            List<string> returnValue;
            var executedMos = new List<string>();

            returnValue = singleGeneric.Call("get_PublicProp", null);
            Assert.Contains(nameof(SingleTypeGenericAttribute), returnValue);
            executedMos.Clear();
            singleGeneric.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(SingleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)singleGeneric.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(SingleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            singleGeneric.Call("ProtectedGeneric", executedMos, typeof(DateTime));
            Assert.Contains(nameof(SingleTypeGenericAttribute), executedMos);

            returnValue = doubleGeneric.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(SingleTypeGenericAttribute), returnValue);
            executedMos.Clear();
            doubleGeneric.Call("set_PublicProp", executedMos);
            Assert.DoesNotContain(nameof(SingleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)doubleGeneric.Call("StaticInternalAsync", executedMos);
            Assert.DoesNotContain(nameof(SingleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            doubleGeneric.Call("ProtectedGeneric", executedMos, typeof(DateTime));
            Assert.DoesNotContain(nameof(SingleTypeGenericAttribute), executedMos);

            returnValue = clsAB.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(SingleTypeGenericAttribute), returnValue);
            executedMos.Clear();
            clsAB.Call("set_PublicProp", executedMos);
            Assert.DoesNotContain(nameof(SingleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsAB.Call("StaticInternalAsync", executedMos);
            Assert.DoesNotContain(nameof(SingleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsAB.Call("PublicSingleGenericAsync", executedMos, typeof(string));
            Assert.DoesNotContain(nameof(SingleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            clsAB.Call("ProtectedStaticDoubleGeneric", executedMos, typeof(Guid), typeof(decimal));
            Assert.DoesNotContain(nameof(SingleTypeGenericAttribute), executedMos);
        }

        [Fact]
        public async Task ExecutionDoubleTypeGenericTest()
        {
            var singleGeneric = GetInstance("PatternUsage.SingleGeneric`1", true, x => x.MakeGenericType(typeof(int)));
            var doubleGeneric = GetInstance("PatternUsage.X.DoubleGeneric`2", true, x => x.MakeGenericType(typeof(string), typeof(double)));
            var clsAB = GetInstance("PatternUsage.ClsAB", true);

            List<string> returnValue;
            var executedMos = new List<string>();

            returnValue = singleGeneric.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(DoubleTypeGenericAttribute), returnValue);
            executedMos.Clear();
            singleGeneric.Call("set_PublicProp", executedMos);
            Assert.DoesNotContain(nameof(DoubleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)singleGeneric.Call("StaticInternalAsync", executedMos);
            Assert.DoesNotContain(nameof(DoubleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            singleGeneric.Call("ProtectedGeneric", executedMos, typeof(DateTime));
            Assert.DoesNotContain(nameof(DoubleTypeGenericAttribute), executedMos);

            returnValue = doubleGeneric.Call("get_PublicProp", null);
            Assert.Contains(nameof(DoubleTypeGenericAttribute), returnValue);
            executedMos.Clear();
            doubleGeneric.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(DoubleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)doubleGeneric.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(DoubleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            doubleGeneric.Call("ProtectedGeneric", executedMos, typeof(DateTime));
            Assert.Contains(nameof(DoubleTypeGenericAttribute), executedMos);

            returnValue = clsAB.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(DoubleTypeGenericAttribute), returnValue);
            executedMos.Clear();
            clsAB.Call("set_PublicProp", executedMos);
            Assert.DoesNotContain(nameof(DoubleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsAB.Call("StaticInternalAsync", executedMos);
            Assert.DoesNotContain(nameof(DoubleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsAB.Call("PublicSingleGenericAsync", executedMos, typeof(string));
            Assert.DoesNotContain(nameof(DoubleTypeGenericAttribute), executedMos);
            executedMos.Clear();
            clsAB.Call("ProtectedStaticDoubleGeneric", executedMos, typeof(Guid), typeof(decimal));
            Assert.DoesNotContain(nameof(DoubleTypeGenericAttribute), executedMos);
        }

        [Fact]
        public async Task ExecutionAnyTypeGenericTest()
        {
            var singleGeneric = GetInstance("PatternUsage.SingleGeneric`1", true, x => x.MakeGenericType(typeof(int)));
            var doubleGeneric = GetInstance("PatternUsage.X.DoubleGeneric`2", true, x => x.MakeGenericType(typeof(string), typeof(double)));
            var clsAB = GetInstance("PatternUsage.ClsAB", true);

            List<string> returnValue;
            var executedMos = new List<string>();

            returnValue = singleGeneric.Call("get_PublicProp", null);
            Assert.Contains(nameof(AnyTypeGenericAttribute), returnValue);
            executedMos.Clear();
            singleGeneric.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(AnyTypeGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)singleGeneric.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(AnyTypeGenericAttribute), executedMos);
            executedMos.Clear();
            singleGeneric.Call("ProtectedGeneric", executedMos, typeof(DateTime));
            Assert.Contains(nameof(AnyTypeGenericAttribute), executedMos);

            returnValue = doubleGeneric.Call("get_PublicProp", null);
            Assert.Contains(nameof(AnyTypeGenericAttribute), returnValue);
            executedMos.Clear();
            doubleGeneric.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(AnyTypeGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)doubleGeneric.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(AnyTypeGenericAttribute), executedMos);
            executedMos.Clear();
            doubleGeneric.Call("ProtectedGeneric", executedMos, typeof(DateTime));
            Assert.Contains(nameof(AnyTypeGenericAttribute), executedMos);

            returnValue = clsAB.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(AnyTypeGenericAttribute), returnValue);
            executedMos.Clear();
            clsAB.Call("set_PublicProp", executedMos);
            Assert.DoesNotContain(nameof(AnyTypeGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsAB.Call("StaticInternalAsync", executedMos);
            Assert.DoesNotContain(nameof(AnyTypeGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsAB.Call("PublicSingleGenericAsync", executedMos, typeof(string));
            Assert.DoesNotContain(nameof(AnyTypeGenericAttribute), executedMos);
            executedMos.Clear();
            clsAB.Call("ProtectedStaticDoubleGeneric", executedMos, typeof(Guid), typeof(decimal));
            Assert.DoesNotContain(nameof(AnyTypeGenericAttribute), executedMos);
        }

        [Fact]
        public async Task ExecutionAnyTypeGenericNoneMethodGenericTest()
        {
            var singleGeneric = GetInstance("PatternUsage.SingleGeneric`1", true, x => x.MakeGenericType(typeof(int)));
            var doubleGeneric = GetInstance("PatternUsage.X.DoubleGeneric`2", true, x => x.MakeGenericType(typeof(string), typeof(double)));
            var clsAB = GetInstance("PatternUsage.ClsAB", true);

            List<string> returnValue;
            var executedMos = new List<string>();

            returnValue = singleGeneric.Call("get_PublicProp", null);
            Assert.Contains(nameof(AnyTypeGenericNoneMethodGenericAttribute), returnValue);
            executedMos.Clear();
            singleGeneric.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(AnyTypeGenericNoneMethodGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)singleGeneric.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(AnyTypeGenericNoneMethodGenericAttribute), executedMos);
            executedMos.Clear();
            singleGeneric.Call("ProtectedGeneric", executedMos, typeof(DateTime));
            Assert.DoesNotContain(nameof(AnyTypeGenericNoneMethodGenericAttribute), executedMos);

            returnValue = doubleGeneric.Call("get_PublicProp", null);
            Assert.Contains(nameof(AnyTypeGenericNoneMethodGenericAttribute), returnValue);
            executedMos.Clear();
            doubleGeneric.Call("set_PublicProp", executedMos);
            Assert.Contains(nameof(AnyTypeGenericNoneMethodGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)doubleGeneric.Call("StaticInternalAsync", executedMos);
            Assert.Contains(nameof(AnyTypeGenericNoneMethodGenericAttribute), executedMos);
            executedMos.Clear();
            doubleGeneric.Call("ProtectedGeneric", executedMos, typeof(DateTime));
            Assert.DoesNotContain(nameof(AnyTypeGenericNoneMethodGenericAttribute), executedMos);

            returnValue = clsAB.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(AnyTypeGenericNoneMethodGenericAttribute), returnValue);
            executedMos.Clear();
            clsAB.Call("set_PublicProp", executedMos);
            Assert.DoesNotContain(nameof(AnyTypeGenericNoneMethodGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsAB.Call("StaticInternalAsync", executedMos);
            Assert.DoesNotContain(nameof(AnyTypeGenericNoneMethodGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsAB.Call("PublicSingleGenericAsync", executedMos, typeof(string));
            Assert.DoesNotContain(nameof(AnyTypeGenericNoneMethodGenericAttribute), executedMos);
            executedMos.Clear();
            clsAB.Call("ProtectedStaticDoubleGeneric", executedMos, typeof(Guid), typeof(decimal));
            Assert.DoesNotContain(nameof(AnyTypeGenericNoneMethodGenericAttribute), executedMos);
        }

        [Fact]
        public async Task ExecutionNoneTypeGenericAnyMethodGenericTest()
        {
            var singleGeneric = GetInstance("PatternUsage.SingleGeneric`1", true, x => x.MakeGenericType(typeof(int)));
            var doubleGeneric = GetInstance("PatternUsage.X.DoubleGeneric`2", true, x => x.MakeGenericType(typeof(string), typeof(double)));
            var clsAB = GetInstance("PatternUsage.ClsAB", true);

            List<string> returnValue;
            var executedMos = new List<string>();

            returnValue = singleGeneric.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(NoneTypeGenericAnyMethodGenericAttribute), returnValue);
            executedMos.Clear();
            singleGeneric.Call("set_PublicProp", executedMos);
            Assert.DoesNotContain(nameof(NoneTypeGenericAnyMethodGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)singleGeneric.Call("StaticInternalAsync", executedMos);
            Assert.DoesNotContain(nameof(NoneTypeGenericAnyMethodGenericAttribute), executedMos);
            executedMos.Clear();
            singleGeneric.Call("ProtectedGeneric", executedMos, typeof(DateTime));
            Assert.DoesNotContain(nameof(NoneTypeGenericAnyMethodGenericAttribute), executedMos);

            returnValue = doubleGeneric.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(NoneTypeGenericAnyMethodGenericAttribute), returnValue);
            executedMos.Clear();
            doubleGeneric.Call("set_PublicProp", executedMos);
            Assert.DoesNotContain(nameof(NoneTypeGenericAnyMethodGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)doubleGeneric.Call("StaticInternalAsync", executedMos);
            Assert.DoesNotContain(nameof(NoneTypeGenericAnyMethodGenericAttribute), executedMos);
            executedMos.Clear();
            doubleGeneric.Call("ProtectedGeneric", executedMos, typeof(DateTime));
            Assert.DoesNotContain(nameof(NoneTypeGenericAnyMethodGenericAttribute), executedMos);

            returnValue = clsAB.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(NoneTypeGenericAnyMethodGenericAttribute), returnValue);
            executedMos.Clear();
            clsAB.Call("set_PublicProp", executedMos);
            Assert.DoesNotContain(nameof(NoneTypeGenericAnyMethodGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsAB.Call("StaticInternalAsync", executedMos);
            Assert.DoesNotContain(nameof(NoneTypeGenericAnyMethodGenericAttribute), executedMos);
            executedMos.Clear();
            await (Task)clsAB.Call("PublicSingleGenericAsync", executedMos, typeof(string));
            Assert.Contains(nameof(NoneTypeGenericAnyMethodGenericAttribute), executedMos);
            executedMos.Clear();
            clsAB.Call("ProtectedStaticDoubleGeneric", executedMos, typeof(Guid), typeof(decimal));
            Assert.Contains(nameof(NoneTypeGenericAnyMethodGenericAttribute), executedMos);
        }

        [Fact]
        public async Task MethodNestedTypeTest()
        {
            var nestedType = GetInstance("PatternUsage.X.NestedType", true);
            var nestedTypeInner = GetInstance("PatternUsage.X.NestedType+Inner", true);
            var nestedTypeInnerDeeply = GetInstance("PatternUsage.X.NestedType+Inner+Deeply", true);

            var executedMos = new List<string>();

            executedMos.Clear();
            nestedType.Call("M1", executedMos);
            Assert.DoesNotContain(nameof(SpecificNestedTypeAttribute), executedMos);
            Assert.DoesNotContain(nameof(AnyDoubleDepthNestedTypeAttribute), executedMos);

            executedMos.Clear();
            nestedTypeInner.Call("M2", executedMos);
            Assert.Contains(nameof(SpecificNestedTypeAttribute), executedMos);
            Assert.Contains(nameof(AnyDoubleDepthNestedTypeAttribute), executedMos);

            executedMos.Clear();
            await (Task)nestedTypeInner.Call("M3", executedMos);
            Assert.Contains(nameof(SpecificNestedTypeAttribute), executedMos);
            Assert.Contains(nameof(AnyDoubleDepthNestedTypeAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask)nestedTypeInnerDeeply.Call("M4", executedMos);
            Assert.DoesNotContain(nameof(SpecificNestedTypeAttribute), executedMos);
            Assert.DoesNotContain(nameof(AnyDoubleDepthNestedTypeAttribute), executedMos);
        }

        [Fact]
        public void ParameterTest()
        {
            var instance = GetInstance("Public");

            var executedMos = new List<string>();

            executedMos.Clear();
            instance.P1_1(executedMos);
            Assert.DoesNotContain(nameof(DoubleAnyParameterAttribute), executedMos);
            Assert.DoesNotContain(nameof(IntAtTheLastOfThreeParameterAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleAtTheSecondOfThreeParameterAttribute), executedMos);

            executedMos.Clear();
            instance.P2_1(executedMos, 1);
            Assert.Contains(nameof(DoubleAnyParameterAttribute), executedMos);
            Assert.DoesNotContain(nameof(IntAtTheLastOfThreeParameterAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleAtTheSecondOfThreeParameterAttribute), executedMos);

            executedMos.Clear();
            instance.P2_2(executedMos, 1.1D);
            Assert.Contains(nameof(DoubleAnyParameterAttribute), executedMos);
            Assert.DoesNotContain(nameof(IntAtTheLastOfThreeParameterAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleAtTheSecondOfThreeParameterAttribute), executedMos);

            executedMos.Clear();
            instance.P3_1(executedMos, 1, 2);
            Assert.DoesNotContain(nameof(DoubleAnyParameterAttribute), executedMos);
            Assert.Contains(nameof(IntAtTheLastOfThreeParameterAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleAtTheSecondOfThreeParameterAttribute), executedMos);

            executedMos.Clear();
            instance.P3_2(executedMos, 1D, 2D);
            Assert.DoesNotContain(nameof(DoubleAnyParameterAttribute), executedMos);
            Assert.DoesNotContain(nameof(IntAtTheLastOfThreeParameterAttribute), executedMos);
            Assert.Contains(nameof(DoubleAtTheSecondOfThreeParameterAttribute), executedMos);

            executedMos.Clear();
            instance.P3_3(executedMos, 3D, 2);
            Assert.DoesNotContain(nameof(DoubleAnyParameterAttribute), executedMos);
            Assert.Contains(nameof(IntAtTheLastOfThreeParameterAttribute), executedMos);
            Assert.Contains(nameof(DoubleAtTheSecondOfThreeParameterAttribute), executedMos);
        }

        [Fact]
        public void ExecutionTupleSyntaxTest()
        {
            var instance = GetInstance("Public");

            var executedMos = new List<string>();
            List<string> returnValue;

            executedMos.Clear();
            instance.Call("ProtectedInstance", executedMos);
            Assert.DoesNotContain(nameof(AnyTwoItemTupleParameterAttribute), executedMos);
            Assert.DoesNotContain(nameof(SpecificTupleReturnAttribute), executedMos);

            executedMos.Clear();
            instance.Call("TupleOut", executedMos);
            Assert.DoesNotContain(nameof(AnyTwoItemTupleParameterAttribute), executedMos);
            Assert.Contains(nameof(SpecificTupleReturnAttribute), executedMos);

            executedMos.Clear();
            returnValue = instance.Call("TupleIn", new Tuple<DateTime, DateTimeOffset>(default, default));
            Assert.Contains(nameof(AnyTwoItemTupleParameterAttribute), returnValue);
            Assert.DoesNotContain(nameof(SpecificTupleReturnAttribute), returnValue);
        }

        [Fact]
        public async Task MethodAsyncSyntaxTest()
        {
            var instance = GetInstance("Async");

            var executedMos = new List<string>();

            executedMos.Clear();
            instance.Call("Sync", executedMos);
            Assert.DoesNotContain(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);

            executedMos.Clear();
            instance.Call("AsyncVoid", executedMos);
            Assert.DoesNotContain(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("Task1", executedMos);
            Assert.DoesNotContain(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.Contains(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask)instance.Call("ValueTask1", executedMos);
            Assert.DoesNotContain(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.Contains(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("TaskAsync", executedMos);
            Assert.DoesNotContain(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.Contains(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask)instance.Call("ValueTaskAsync", executedMos);
            Assert.DoesNotContain(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.Contains(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);

            executedMos.Clear();
            await (Task<int>)instance.Call("TaskInt", executedMos);
            Assert.Contains(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.Contains(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);

            executedMos.Clear();
            await (Task<string>)instance.Call("TaskString", executedMos);
            Assert.Contains(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask<int>)instance.Call("ValueTaskInt", executedMos);
            Assert.Contains(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.Contains(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask<string>)instance.Call("ValueTaskString", executedMos);
            Assert.Contains(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);

            executedMos.Clear();
            await (Task<int>)instance.Call("TaskIntAsync", executedMos);
            Assert.Contains(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.Contains(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask<string>)instance.Call("ValueTaskStringAsync", executedMos);
            Assert.Contains(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);

            executedMos.Clear();
            await (Task<double>)instance.Call("TaskDoubleAsync", executedMos);
            Assert.Contains(nameof(GenericVoTaskReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(NoGenericVoTaskReturnAttribute), executedMos);
            Assert.Contains(nameof(DoubleOrIntVoTaskReturnAttribute), executedMos);
        }

        [Fact]
        public async Task ExecutionNullableSyntaxTest()
        {
            var instance = GetInstance("Public");

            var executedMos = new List<string>();

            executedMos.Clear();
            instance.Call("ProtectedInstance", executedMos);
            Assert.DoesNotContain(nameof(NullableIntReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(AsyncNullableIntReturnAttribute), executedMos);

            executedMos.Clear();
            instance.Call("IntNullable", executedMos);
            Assert.Contains(nameof(NullableIntReturnAttribute), executedMos);
            Assert.DoesNotContain(nameof(AsyncNullableIntReturnAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask<int?>)instance.Call("StaticNullableAsync", executedMos);
            Assert.DoesNotContain(nameof(NullableIntReturnAttribute), executedMos);
            Assert.Contains(nameof(AsyncNullableIntReturnAttribute), executedMos);
        }

        [Fact]
        public async Task RegexTest()
        {
            var pub = GetInstance("Public");
            var nested = GetInstance("PatternUsage.X.NestedType", true);
            var nestedInner = GetInstance("PatternUsage.X.NestedType+Inner", true);
            var nestedInnerDeeply = GetInstance("PatternUsage.X.NestedType+Inner+Deeply", true);

            var executedMos = new List<string>();
            var returnValue = new List<string>();

            returnValue = pub.Call("get_Prop1", null);
            Assert.Contains(nameof(NumberSuffixAttribute), returnValue);

            executedMos.Clear();
            pub.Call("set_Prop1", executedMos);
            Assert.Contains(nameof(NumberSuffixAttribute), executedMos);

            executedMos.Clear();
            pub.Call("Instance", executedMos);
            Assert.DoesNotContain(nameof(NumberSuffixAttribute), executedMos);

            executedMos.Clear();
            await (Task)pub.Call("P2InstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(NumberSuffixAttribute), executedMos);

            executedMos.Clear();
            pub.P1_1(executedMos);
            Assert.Contains(nameof(NumberSuffixAttribute), executedMos);

            executedMos.Clear();
            pub.P2_1(executedMos, 1);
            Assert.Contains(nameof(NumberSuffixAttribute), executedMos);

            executedMos.Clear();
            pub.P2_2(executedMos, 2D);
            Assert.Contains(nameof(NumberSuffixAttribute), executedMos);

            executedMos.Clear();
            pub.P3_1(executedMos, 1, 2);
            Assert.Contains(nameof(NumberSuffixAttribute), executedMos);

            executedMos.Clear();
            pub.P3_2(executedMos, 3D, 5D);
            Assert.Contains(nameof(NumberSuffixAttribute), executedMos);

            executedMos.Clear();
            pub.P3_3(executedMos, 2D, -1);
            Assert.Contains(nameof(NumberSuffixAttribute), executedMos);

            executedMos.Clear();
            nested.Call("M1", executedMos);
            Assert.Contains(nameof(NumberSuffixAttribute), executedMos);

            executedMos.Clear();
            nestedInner.Call("M2", executedMos);
            Assert.Contains(nameof(NumberSuffixAttribute), executedMos);

            executedMos.Clear();
            await (Task)nestedInner.Call("M3", executedMos);
            Assert.Contains(nameof(NumberSuffixAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask)nestedInnerDeeply.Call("M4", executedMos);
            Assert.Contains(nameof(NumberSuffixAttribute), executedMos);
        }

        [Fact]
        public async Task InlineTest()
        {
            var clsA = GetInstance("ClsA");

            List<string> listReturn;
            IDictionary imapReturn;
            var listArg = new List<string>();
            var mapArg = new Dictionary<string, string>();
            var sortedMapArg = new SortedList<string, string>();

            listReturn = clsA.Call("get_PublicProp", null);
            Assert.DoesNotContain(nameof(InlineDeclareAttribute), listReturn);
            listArg.Clear();
            clsA.Call("set_PublicProp", listArg);
            Assert.DoesNotContain(nameof(InlineDeclareAttribute), listArg);
            listReturn = clsA.Call("get_StaticPrivateProp", null);
            Assert.DoesNotContain(nameof(InlineDeclareAttribute), listReturn);
            listArg.Clear();
            clsA.Call("set_StaticPrivateProp", listArg);
            Assert.DoesNotContain(nameof(InlineDeclareAttribute), listArg);
            imapReturn = clsA.Call("get_ProtectedInternalProp", null);
            Assert.False(imapReturn.Contains(nameof(InlineDeclareAttribute)));
            mapArg.Clear();
            clsA.Call("set_ProtectedInternalProp", mapArg);
            Assert.False(mapArg.ContainsKey(nameof(InlineDeclareAttribute)));
            imapReturn = clsA.Call("get_DefaultProp", null);
            Assert.False(imapReturn.Contains(nameof(InlineDeclareAttribute)));
            sortedMapArg.Clear();
            clsA.Call("set_DefaultProp", sortedMapArg);
            Assert.False(sortedMapArg.ContainsKey(nameof(InlineDeclareAttribute)));
            listArg.Clear();
            clsA.Call("Protected", listArg);
            Assert.Contains(nameof(InlineDeclareAttribute), listArg);
            listArg.Clear();
            await (Task)clsA.Call("StaticInternalAsync", listArg);
            Assert.Contains(nameof(InlineDeclareAttribute), listArg);
            listReturn = clsA.Call("PrivateProtected", null);
            Assert.Contains(nameof(InlineDeclareAttribute), listReturn);
        }

        [Fact]
        public void ApllyToMethodDirectlyTest()
        {
            var instance = GetInstance("Public");

            var executedMos = new List<string>();

            executedMos.Clear();
            instance.P1_1(executedMos);
            Assert.Contains(nameof(InlineDeclareAttribute), executedMos);

            executedMos.Clear();
            instance.P2_1(executedMos, 1);
            Assert.Contains(nameof(InlineDeclareAttribute), executedMos);

            executedMos.Clear();
            instance.P2_2(executedMos, 2D);
            Assert.DoesNotContain(nameof(InlineDeclareAttribute), executedMos);

            executedMos.Clear();
            instance.P3_1(executedMos, 1, 2);
            Assert.Contains(nameof(InlineDeclareAttribute), executedMos);

            executedMos.Clear();
            instance.P3_2(executedMos, 3D, 5D);
            Assert.DoesNotContain(nameof(InlineDeclareAttribute), executedMos);

            executedMos.Clear();
            instance.P3_3(executedMos, 2D, -1);
            Assert.DoesNotContain(nameof(InlineDeclareAttribute), executedMos);
        }

        [Fact]
        public async Task IntegrativeTest()
        {
            var pub = GetInstance("Public");
            var nested = GetInstance("PatternUsage.X.NestedType", true);
            var nestedInner = GetInstance("PatternUsage.X.NestedType+Inner", true);
            var nestedInnerDeeply = GetInstance("PatternUsage.X.NestedType+Inner+Deeply", true);

            var executedMos = new List<string>();
            var returnValue = new List<string>();

            returnValue = pub.Call("get_Prop1", null);
            Assert.DoesNotContain(nameof(IntegrativeAttribute), returnValue);

            executedMos.Clear();
            pub.Call("set_Prop1", executedMos);
            Assert.DoesNotContain(nameof(IntegrativeAttribute), executedMos);

            executedMos.Clear();
            pub.Call("Instance", executedMos);
            Assert.DoesNotContain(nameof(IntegrativeAttribute), executedMos);

            executedMos.Clear();
            await (Task)pub.Call("P2InstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(IntegrativeAttribute), executedMos);

            executedMos.Clear();
            pub.P1_1(executedMos);
            Assert.DoesNotContain(nameof(IntegrativeAttribute), executedMos);

            executedMos.Clear();
            pub.P2_1(executedMos, 1);
            Assert.Contains(nameof(IntegrativeAttribute), executedMos);

            executedMos.Clear();
            pub.P2_2(executedMos, 2D);
            Assert.Contains(nameof(IntegrativeAttribute), executedMos);

            executedMos.Clear();
            pub.P3_1(executedMos, 1, 2);
            Assert.DoesNotContain(nameof(IntegrativeAttribute), executedMos);

            executedMos.Clear();
            pub.P3_2(executedMos, 3D, 5D);
            Assert.DoesNotContain(nameof(IntegrativeAttribute), executedMos);

            executedMos.Clear();
            pub.P3_3(executedMos, 2D, -1);
            Assert.DoesNotContain(nameof(IntegrativeAttribute), executedMos);

            executedMos.Clear();
            nested.Call("M1", executedMos);
            Assert.DoesNotContain(nameof(IntegrativeAttribute), executedMos);

            executedMos.Clear();
            nestedInner.Call("M2", executedMos);
            Assert.DoesNotContain(nameof(IntegrativeAttribute), executedMos);

            executedMos.Clear();
            await (Task)nestedInner.Call("M3", executedMos);
            Assert.DoesNotContain(nameof(IntegrativeAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask)nestedInnerDeeply.Call("M4", executedMos);
            Assert.DoesNotContain(nameof(IntegrativeAttribute), executedMos);
        }
    }
}
