#if NET48 && MONO_COMPAT
using System.Diagnostics;
using System;
using System.IO;
using Xunit;
using System.Collections.Generic;

namespace Rougamo.Fody.Tests
{
    [CollectionDefinition("Mono48", DisableParallelization = true)]
    public class Mono48CollectionDefinition
    {
    }

    public static class MonoRunnerBridge
    {
        private static readonly string[] MonoCompatProjects =
        [
            Path.Combine("test", "TestAssemblies", "IndirectDependency2", "IndirectDependency2.csproj"),
            Path.Combine("test", "TestAssemblies", "IndirectDependency1", "IndirectDependency1.csproj"),
            Path.Combine("test", "TestAssemblies", "BasicUsage", "BasicUsage.csproj"),
            Path.Combine("test", "TestAssemblies", "ConfiguredMoUsage", "ConfiguredMoUsage.csproj"),
            Path.Combine("test", "TestAssemblies", "Issues", "Issues.csproj"),
            Path.Combine("test", "TestAssemblies", "PatternUsage", "PatternUsage.csproj"),
            Path.Combine("test", "TestAssemblies", "SignatureUsage", "SignatureUsage.csproj"),
            Path.Combine("test", "TestAssemblies", "MonoTest", "MonoTest.csproj"),
            Path.Combine("test", "TestAssemblies", "IndependentMos", "IndependentMos.csproj")
        ];
        private static readonly HashSet<string> WhiteListedFacts = new()
        {
            "Rougamo.Fody.Tests.IssueTest.Issue8Test",
        };

        private static readonly object RunnerLock = new();
        private static bool RunnerReady;
        public static void RunMonoFact(string factFullName)
        {
            if (Environment.GetEnvironmentVariable("ROUGAMO_MONO_RUNNER") == "1")
            {
                return;
            }

            var monoExe = ResolveMonoExePath();
            Assert.True(monoExe != null, "Mono executable not found while MonoCompat=true.");

            if (WhiteListedFacts.Contains(factFullName))
            {
                return;
            }

            var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
#if DEBUG
            const string configuration = "Debug";
#else
            const string configuration = "Release";
#endif
            var runnerProject = Path.Combine(root, ".tmp", "MonoRunner", "MonoRunner.csproj");
            var runnerExe = Path.Combine(root, ".tmp", "MonoRunner", "bin", configuration, "net48", "MonoRunner.exe");
            var monoOutDir = Path.Combine(root, ".tmp", "MonoCompat", "TestAssemblies", "bin");
            var testBinDir = Path.Combine(root, "test", "Rougamo.Fody.Tests", "bin", configuration, "net48");
            var sandboxDir = Path.Combine(root, ".tmp", "MonoRunSandbox", "net48");
            var testDll = Path.Combine(sandboxDir, "Rougamo.Fody.Tests.dll");
            var monoPath = sandboxDir;

            EnsureRunnerReady(root, runnerProject, runnerExe, monoOutDir, testBinDir, sandboxDir);

            var run = Run(
                monoExe!,
                sandboxDir,
                new[] { runnerExe, testDll, factFullName },
                ("MONO_PATH", monoPath));

            Assert.True(run.ExitCode == 0, $"Mono runner failed for '{factFullName}':\n" + run.Output);
        }

        private static string ResolveMonoExePath()
        {
            if (CanRunCommand("mono"))
            {
                return "mono";
            }

            var path = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrWhiteSpace(path))
            {
                var separator = Path.PathSeparator;
                foreach (var segment in path.Split([separator], StringSplitOptions.RemoveEmptyEntries))
                {
                    var dir = segment.Trim().Trim('"');
                    if (string.IsNullOrWhiteSpace(dir)) continue;

                    var monoExe = Path.Combine(dir, "mono.exe");
                    if (File.Exists(monoExe))
                    {
                        return monoExe;
                    }

                    var mono = Path.Combine(dir, "mono");
                    if (File.Exists(mono))
                    {
                        return mono;
                    }
                }
            }

            const string monoInProgramFiles = @"C:\Program Files\Mono\bin\mono.exe";
            if (File.Exists(monoInProgramFiles))
            {
                return monoInProgramFiles;
            }

            const string monoInProgramFilesX86 = @"C:\Program Files (x86)\Mono\bin\mono.exe";
            if (File.Exists(monoInProgramFilesX86))
            {
                return monoInProgramFilesX86;
            }

            return null;
        }

        private static bool CanRunCommand(string command)
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                if (process == null) return false;
                process.WaitForExit(3000);
                return process.HasExited;
            }
            catch
            {
                return false;
            }
        }

        private static void EnsureRunnerReady(string root, string runnerProject, string runnerExe, string monoOutDir, string testBinDir, string sandboxDir)
        {
#if DEBUG
            const string configuration = "Debug";
#else
            const string configuration = "Release";
#endif
            if (RunnerReady) return;

            lock (RunnerLock)
            {
                if (RunnerReady) return;

                EnsureMonoRunnerProject(runnerProject);
                var build = Run("dotnet", root, new[] { "build", runnerProject, "-c", configuration, "-f", "net48" });
                Assert.True(build.ExitCode == 0, "Build Mono runner failed:\n" + build.Output);
                Assert.True(File.Exists(runnerExe), "Mono runner exe not found after build: " + runnerExe);

                Directory.CreateDirectory(monoOutDir);
                var monoOutNet48Dir = Path.Combine(monoOutDir, configuration, "net48");
                Directory.CreateDirectory(monoOutNet48Dir);
                var monoOutArg = monoOutDir.Replace('\\', '/') + "/";
                foreach (var relativeProject in MonoCompatProjects)
                {
                    var project = Path.Combine(root, relativeProject);
                    var buildMonoProject = Run(
                        "dotnet",
                        root,
                        new[]
                        {
                            "build",
                            project,
                            "-c",
                            configuration,
                            "-f",
                            "net48",
                            "-p:MonoCompat=true",
                            $"-p:BaseOutputPath={monoOutArg}",
                            "-nr:false",
                            "-m:1"
                        });
                    Assert.True(buildMonoProject.ExitCode == 0, $"Build mono-compat project failed: {relativeProject}\n" + buildMonoProject.Output);
                }
                var monoBasicUsageDll = Path.Combine(monoOutDir, configuration, "net48", "BasicUsage.dll");
                Assert.True(File.Exists(monoBasicUsageDll), "Mono-compat BasicUsage dll not found after build: " + monoBasicUsageDll);

                // Copy original net48 test output to isolated sandbox.
                CopyDirectory(testBinDir, sandboxDir, overwrite: true);

                // Overlay mono-compat TestAssemblies to ensure Mono loads compatible metadata first.
                CopyDirectory(monoOutNet48Dir, sandboxDir, overwrite: true);

                var sandboxTestDll = Path.Combine(sandboxDir, "Rougamo.Fody.Tests.dll");
                Assert.True(File.Exists(sandboxTestDll), "Sandbox test dll not found: " + sandboxTestDll);
                RunnerReady = true;
            }
        }

        private static void CopyDirectory(string sourceDir, string targetDir, bool overwrite)
        {
            Assert.True(Directory.Exists(sourceDir), "Directory not found: " + sourceDir);
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var targetFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, targetFile, overwrite);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                var targetSubDir = Path.Combine(targetDir, Path.GetFileName(directory));
                CopyDirectory(directory, targetSubDir, overwrite);
            }
        }

        private static (int ExitCode, string Output) Run(string fileName, string workingDirectory, string[] args, params (string Key, string Value)[] envVars)
        {
            var start = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = BuildArguments(args),
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            foreach (var envVar in envVars)
            {
                start.Environment[envVar.Key] = envVar.Value;
            }

            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (process.ExitCode, output);
        }

        private static string BuildArguments(string[] args)
        {
            static string Quote(string arg)
            {
                if (string.IsNullOrEmpty(arg)) return "\"\"";
                if (arg.IndexOfAny([' ', '\t', '"']) < 0) return arg;
                return "\"" + arg.Replace("\"", "\\\"") + "\"";
            }

            return string.Join(" ", Array.ConvertAll(args, Quote));
        }

        private static void EnsureMonoRunnerProject(string runnerProject)
        {
            var runnerDir = Path.GetDirectoryName(runnerProject)!;
            Directory.CreateDirectory(runnerDir);

            if (!File.Exists(runnerProject))
            {
                File.WriteAllText(
                    runnerProject,
                    """
                    <Project Sdk="Microsoft.NET.Sdk">
                      <PropertyGroup>
                        <TargetFramework>net48</TargetFramework>
                        <OutputType>Exe</OutputType>
                        <LangVersion>latest</LangVersion>
                      </PropertyGroup>
                    </Project>
                    """);
            }

            var programFile = Path.Combine(runnerDir, "Program.cs");
            if (!File.Exists(programFile))
            {
                File.WriteAllText(
                    programFile,
                    """
                    using System;
                    using System.IO;
                    using System.Linq;
                    using System.Reflection;
                    using System.Threading.Tasks;

                    public static class Program
                    {
                        public static int Main(string[] args)
                        {
                            try
                            {
                                Environment.SetEnvironmentVariable("ROUGAMO_MONO_RUNNER", "1");
                                if (args.Length < 2)
                                {
                                    Console.Error.WriteLine("Usage: MonoRunner <testDllPath> <factFullName>");
                                    return 2;
                                }

                                var testDllPath = Path.GetFullPath(args[0]);
                                var factFullName = args[1];
                                if (!File.Exists(testDllPath))
                                {
                                    Console.Error.WriteLine("Test dll not found: " + testDllPath);
                                    return 2;
                                }

                                var lastDot = factFullName.LastIndexOf('.');
                                if (lastDot <= 0 || lastDot >= factFullName.Length - 1)
                                {
                                    Console.Error.WriteLine("Invalid fact full name: " + factFullName);
                                    return 2;
                                }

                                var typeName = factFullName.Substring(0, lastDot);
                                var methodName = factFullName.Substring(lastDot + 1);

                                var assembly = Assembly.LoadFrom(testDllPath);
                                var type = assembly.GetType(typeName, throwOnError: false);
                                if (type == null)
                                {
                                    Console.Error.WriteLine("Type not found: " + typeName);
                                    return 3;
                                }

                                var candidates = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                                                     .Where(m => m.Name == methodName)
                                                     .ToArray();
                                if (candidates.Length == 0)
                                {
                                    Console.Error.WriteLine("Fact not found: " + factFullName);
                                    return 3;
                                }

                                MethodInfo? factMethod = null;
                                foreach (var candidate in candidates)
                                {
                                    var hasFactAttribute = candidate.GetCustomAttributes(inherit: true)
                                                                    .Any(a => a.GetType().Name == "FactAttribute");
                                    if (hasFactAttribute)
                                    {
                                        factMethod = candidate;
                                        break;
                                    }
                                }
                                if (factMethod == null)
                                {
                                    Console.Error.WriteLine("Fact not found: " + factFullName);
                                    return 3;
                                }

                                object? instance = null;
                                if (!factMethod.IsStatic)
                                {
                                    instance = Activator.CreateInstance(type, nonPublic: true);
                                }

                                var result = factMethod.Invoke(instance, null);
                                if (result is Task task)
                                {
                                    task.GetAwaiter().GetResult();
                                }

                                return 0;
                            }
                            catch (TargetInvocationException ex) when (ex.InnerException != null)
                            {
                                Console.Error.WriteLine(ex.InnerException);
                                return 1;
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine(ex);
                                return 1;
                            }
                        }
                    }
                    """);
            }
        }
    }

    public class MonoTests
    {
        private static readonly WeavedAssembly Assembly;

        static MonoTests()
        {
            Assembly = new("MonoTest");
        }

        [Fact]
        public void MonoStackTraceTest()
        {
            var instance = Assembly.GetInstance("MonoCase`1", false, t => t.MakeGenericType(typeof(int)));
            var stackTrace = (string)instance.CatchAndGetStackTrace();

            Assert.False(string.IsNullOrWhiteSpace(stackTrace));
            Assert.Contains("ThrowCore", stackTrace);
        }
    }
}
#endif
