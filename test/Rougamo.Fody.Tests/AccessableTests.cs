using BasicUsage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Rougamo.Fody.Tests
{
    [Collection(nameof(BasicUsage))]
    public class AccessableTests : TestBase
    {
        //#if NET461
        //        public AccessableTests() : base("../../Release/net461/BasicUsage.dll")
        //#elif NET48
        //                public AccessableTests() : base("../../Release/net48/BasicUsage.dll")
        //#elif NETCOREAPP3_1
        //                public AccessableTests() : base("../../Release/netcoreapp3.1/BasicUsage.dll")
        //#elif NET6_0
        //                public AccessableTests() : base("../../Release/net6.0/BasicUsage.dll")
        //#elif NET7_0
        //                public AccessableTests() : base("../../Release/net7.0/BasicUsage.dll")
        //#elif NET8_0
        //                public AccessableTests() : base("../../Release/net8.0/BasicUsage.dll")
        //#endif
        public AccessableTests() : base("BasicUsage.dll")
        {
        }

        protected override string RootNamespace => "BasicUsage";

        [Fact]
        public async Task MethodPropertyTest()
        {
            var instance = GetInstance(nameof(AccessableUseCase));
            var staticInstance = GetStaticInstance(nameof(AccessableUseCase));
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
    }
}
