using BasicUsage.Attributes;
using Rougamo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class ForceSyncUseCase
    {
        [ForceSync(Order = 1, ForceSync = ForceSync.None)]
        [ForceSync(Order = 2, ForceSync = ForceSync.None)]
        [ForceSync(Order = 3, ForceSync = ForceSync.None)]
        public async ValueTask<string[]> VPassedAAA(List<string> actualExecution)
        {
            await Task.Yield();
            return ["AAA", "AAA", "AAA"];
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnExit)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnExit)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnExit)]
        public Task<string[]> PassedAAS(List<string> actualExecution)
        {
            return Task.FromResult<string[]>(["AAS", "AAS", "AAS"]);
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnSuccess)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnSuccess)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnSuccess)]
        public static ValueTask<string[]> SVPassedASA(List<string> actualExecution)
        {
            return new(["ASA", "ASA", "ASA"]);
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnSuccess | ForceSync.OnExit)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnSuccess | ForceSync.OnExit)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnSuccess | ForceSync.OnExit)]
        public static async Task<string[]> SPassedASS(List<string> actualExecution)
        {
            await Task.Yield();
            return ["ASS", "ASS", "ASS"];
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.All)]
        [ForceSync(Order = 2, ForceSync = ForceSync.All)]
        [ForceSync(Order = 3, ForceSync = ForceSync.All)]
        public async Task<string[]> PassedSSS(List<string> actualExecution)
        {
            await Task.Yield();
            return ["SSS", "SSS", "SSS"];
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnEntry | ForceSync.OnSuccess)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnEntry | ForceSync.OnSuccess)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnEntry | ForceSync.OnSuccess)]
        public ValueTask<string[]> VPassedSSA(List<string> actualExecution)
        {
            return new(["SSA", "SSA", "SSA"]);
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnEntry | ForceSync.OnExit)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnEntry | ForceSync.OnExit)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnEntry | ForceSync.OnExit)]
        public static Task<string[]> SPassedSAS(List<string> actualExecution)
        {
            return Task.FromResult<string[]>(["SAS", "SAS", "SAS"]);
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnEntry)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnEntry)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnEntry)]
        public static async ValueTask<string[]> SVPassedSAA(List<string> actualExecution)
        {
            await Task.Yield();
            return ["SAA", "SAA", "SAA"];
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.None)]
        [ForceSync(Order = 2, ForceSync = ForceSync.None)]
        [ForceSync(Order = 3, ForceSync = ForceSync.None)]
        public async Task FailedAAA(List<string> actualExecution, List<string> expectedExecution)
        {
            expectedExecution.Add("AAA");
            expectedExecution.Add("AAA");
            expectedExecution.Add("AAA");
            await Task.Yield();
            throw new NotImplementedException();
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnExit)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnExit)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnExit)]
        public ValueTask VFailedAAS(List<string> actualExecution, List<string> expectedExecution)
        {
            expectedExecution.Add("AAS");
            expectedExecution.Add("AAS");
            expectedExecution.Add("AAS");
            throw new NotImplementedException();
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnException)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnException)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnException)]
        public static Task SFailedASA(List<string> actualExecution, List<string> expectedExecution)
        {
            expectedExecution.Add("ASA");
            expectedExecution.Add("ASA");
            expectedExecution.Add("ASA");
            throw new NotImplementedException();
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnException | ForceSync.OnExit)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnException | ForceSync.OnExit)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnException | ForceSync.OnExit)]
        public static async ValueTask SVFailedASS(List<string> actualExecution, List<string> expectedExecution)
        {
            expectedExecution.Add("ASS");
            expectedExecution.Add("ASS");
            expectedExecution.Add("ASS");
            await Task.Yield();
            throw new NotImplementedException();
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.All)]
        [ForceSync(Order = 2, ForceSync = ForceSync.All)]
        [ForceSync(Order = 3, ForceSync = ForceSync.All)]
        public async ValueTask VFailedSSS(List<string> actualExecution, List<string> expectedExecution)
        {
            expectedExecution.Add("SSS");
            expectedExecution.Add("SSS");
            expectedExecution.Add("SSS");
            await Task.Yield();
            throw new NotImplementedException();
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnEntry | ForceSync.OnException)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnEntry | ForceSync.OnException)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnEntry | ForceSync.OnException)]
        public Task FailedSSA(List<string> actualExecution, List<string> expectedExecution)
        {
            expectedExecution.Add("SSA");
            expectedExecution.Add("SSA");
            expectedExecution.Add("SSA");
            throw new NotImplementedException();
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnEntry | ForceSync.OnExit)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnEntry | ForceSync.OnExit)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnEntry | ForceSync.OnExit)]
        public static ValueTask SVFailedSAS(List<string> actualExecution, List<string> expectedExecution)
        {
            expectedExecution.Add("SAS");
            expectedExecution.Add("SAS");
            expectedExecution.Add("SAS");
            throw new NotImplementedException();
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnEntry)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnEntry)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnEntry)]
        public static async Task SFailedSAA(List<string> actualExecution, List<string> expectedExecution)
        {
            expectedExecution.Add("SAA");
            expectedExecution.Add("SAA");
            expectedExecution.Add("SAA");
            await Task.Yield();
            throw new NotImplementedException();
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.None)]
        [ForceSync(Order = 2, ForceSync = ForceSync.All)]
        [ForceSync(Order = 3, ForceSync = ForceSync.None)]
        public async Task<string[]> PassedAAA_SSS_AAA(List<string> actualExecution)
        {
            await Task.Yield();
            return ["AAA", "SSS", "AAA"];
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.All)]
        [ForceSync(Order = 2, ForceSync = ForceSync.None)]
        [ForceSync(Order = 3, ForceSync = ForceSync.All)]
        public async Task<string[]> PassedSSS_AAA_SSS(List<string> actualExecution)
        {
            await Task.Yield();
            return ["SSS", "AAA", "SSS"];
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnSuccess | ForceSync.OnExit)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnEntry | ForceSync.OnExit)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnEntry | ForceSync.OnSuccess)]
        public static async Task<string[]> SPassedASS_SAS_SSA(List<string> actualExecution)
        {
            await Task.Yield();
            return ["SSA", "SAS", "ASS"];
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnEntry)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnSuccess)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnExit)]
        public static async Task<string[]> SPassedSAA_ASA_AAS(List<string> actualExecution)
        {
            await Task.Yield();
            return ["AAS", "ASA", "SAA"];
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.None)]
        [ForceSync(Order = 2, ForceSync = ForceSync.All)]
        [ForceSync(Order = 3, ForceSync = ForceSync.All)]
        public async ValueTask VFailedAAA_SSS_SSS(List<string> actualExecution, List<string> expectedExecution)
        {
            expectedExecution.Add("SSS");
            expectedExecution.Add("SSS");
            expectedExecution.Add("AAA");
            await Task.Yield();
            throw new NotImplementedException();
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.All)]
        [ForceSync(Order = 2, ForceSync = ForceSync.All)]
        [ForceSync(Order = 3, ForceSync = ForceSync.None)]
        public async ValueTask VFailedSSS_SSS_AAA(List<string> actualExecution, List<string> expectedExecution)
        {
            expectedExecution.Add("AAA");
            expectedExecution.Add("SSS");
            expectedExecution.Add("SSS");
            await Task.Yield();
            throw new NotImplementedException();
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnExit)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnException | ForceSync.OnExit)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnException)]
        public static async ValueTask SVFailedAAS_ASS_ASA(List<string> actualExecution, List<string> expectedExecution)
        {
            expectedExecution.Add("ASA");
            expectedExecution.Add("ASS");
            expectedExecution.Add("AAS");
            await Task.Yield();
            throw new NotImplementedException();
        }

        [ForceSync(Order = 1, ForceSync = ForceSync.OnEntry)]
        [ForceSync(Order = 2, ForceSync = ForceSync.OnEntry | ForceSync.OnException)]
        [ForceSync(Order = 3, ForceSync = ForceSync.OnEntry | ForceSync.OnExit)]
        public static async ValueTask SVFailedSAA_SSA_SAS(List<string> actualExecution, List<string> expectedExecution)
        {
            expectedExecution.Add("SAS");
            expectedExecution.Add("SSA");
            expectedExecution.Add("SAA");
            await Task.Yield();
            throw new NotImplementedException();
        }
    }
}
