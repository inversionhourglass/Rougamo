using BasicUsage;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
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

        [Fact]
        public async Task DiscovererTest()
        {
            var instance = GetInstance(nameof(DiscoveryUseCase));
            var recording = (List<string>)instance.Recording;
            var type = (Type)instance.GetType();

            recording.Clear();
            instance.Name = "10";
            Assert.Equal(DiscoveryUseCase.Expected_Name, recording);

            recording.Clear();
            var salary = (int)instance.Salary;
            Assert.Equal(DiscoveryUseCase.Expected_Salary, recording);
            Assert.Equal(0, salary);

            recording.Clear();
            instance.GetNameSalary();
            Assert.Equal(DiscoveryUseCase.Expected_GetNameSalary, recording);

            recording.Clear();
            instance.GetSalaryName();
            Assert.Equal(DiscoveryUseCase.Expected_GetSalaryName, recording);

            recording.Clear();
            await (Task<int>)instance.GetRawSalaryAsync();
            Assert.Equal(DiscoveryUseCase.Expected_GetRawSalaryAsync, recording);

            recording.Clear();
            await (Task<string>)instance.RandomStringAsync();
            Assert.Equal(DiscoveryUseCase.Expected_RandomStringAsync, recording);

            recording.Clear();
            instance.RandomInt();
            Assert.Equal(DiscoveryUseCase.Expected_RandomInt, recording);
        }
    }
}
