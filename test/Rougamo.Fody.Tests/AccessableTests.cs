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
