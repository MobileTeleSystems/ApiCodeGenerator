#nullable disable
#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1204 // Static elements should appear before instance elements

namespace ApiCodeGenerator.MSBuild.Tests;

/// <summary>
/// This class is designed for testing MSBuild task code.
/// Copy the code from the task (build/Console.targets) into the nested SelectGeneratorRuntime class and run the test.
/// After making changes, copy the code back.
/// </summary>
public class SelectGeneratorRuntimeTaskTest
{
    [Theory]
    [TestCaseSource(nameof(PositiveCases))]
    public void PositiveTest(TK data)
    {
        var log = new Mock<ITaskLoggingHelper>();
        var task = new SelectGeneratorRuntime(log.Object, data);

        var res = task.Execute();

        log.VerifyNoOtherCalls();
        Assert.True(res);
        Assert.AreEqual(data.AcgToolDir, task.AcgToolDir);
        Assert.AreEqual(data.AcgNswagToolDir, task.AcgNswagToolDir);
    }

    [Theory]
    [TestCaseSource(nameof(NegativeCases))]
    public void NegativeTest(string err, TK data)
    {
        var log = new Mock<ITaskLoggingHelper>();
        var task = new SelectGeneratorRuntime(log.Object, data);

        var res = task.Execute();

        Assert.False(res);
        var invoke = log.Invocations.Single(i => i.Method.Name == nameof(ITaskLoggingHelper.LogErrorFromException));
        var ex = (Exception)invoke.Arguments.First();
        Assert.AreEqual(err, ex.Message);
    }

    public static IEnumerable<TestCaseData> PositiveCases()
    {
        TaskItem expAcgRuntime = "net8.0";
        TaskItem expNswagRuntime = "Net80";
        yield return new TestCaseData(
            new TK
            {
                NetRuntimes = [
                    @"Microsoft.NETCore.App 2.1.30 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]",
                    "Microsoft.NETCore.App 8.0.8 [/usr/share/dotnet/shared/Microsoft.NETCore.App]",
                    "Microsoft.NETCore.App 10.0.1 [/usr/share/dotnet/shared/Microsoft.NETCore.App]"],
                AcgRuntimes = ["net6.0", expAcgRuntime, "net9.0"],
                NswagRuntimes = [expNswagRuntime, "Net90", "Net100"],
                AcgToolDir = expAcgRuntime,
                AcgNswagToolDir = expNswagRuntime,
            })
        .SetName("Positive(Equals)");

        expAcgRuntime = "net10.0";
        expNswagRuntime = "Net100";
        yield return new TestCaseData(
            new TK
            {
                NetRuntimes = [
                    "Microsoft.NETCore.App 8.0.8 [/usr/share/dotnet/shared/Microsoft.NETCore.App]",
                    "Microsoft.NETCore.App 10.0.1 [/usr/share/dotnet/shared/Microsoft.NETCore.App]"],
                AcgRuntimes = ["net6.0", "net8.0", "net9.0", expAcgRuntime],
                NswagRuntimes = ["Net80", "Net90", expNswagRuntime],
                AcgToolDir = expAcgRuntime,
                AcgNswagToolDir = expNswagRuntime,
            })
        .SetName("Positive(Equals .NET 10)");

        expAcgRuntime = "net9.0";
        expNswagRuntime = "Net90";
        yield return new TestCaseData(
            new TK
            {
                NetRuntimes = ["Microsoft.NETCore.App 10.0.1 [/usr/share/dotnet/shared/Microsoft.NETCore.App]"],
                AcgRuntimes = ["net6.0", "net8.0", expAcgRuntime],
                NswagRuntimes = ["Net80", expNswagRuntime],
                AcgToolDir = expAcgRuntime,
                AcgNswagToolDir = expNswagRuntime,
            })
            .SetName("Positive(Major NET 10)");

        expAcgRuntime = "net6.0";
        expNswagRuntime = "Net60";
        yield return new TestCaseData(
            new TK
            {
                NetRuntimes = ["Microsoft.NETCore.App 7.0.1 [/usr/share/dotnet/shared/Microsoft.NETCore.App]"],
                AcgRuntimes = ["net9.0", "net8.0", expAcgRuntime],
                NswagRuntimes = ["Net80", expNswagRuntime],
                AcgToolDir = expAcgRuntime,
                AcgNswagToolDir = expNswagRuntime,
            })
            .SetName("Positive(Major)");
    }

    public static IEnumerable<TestCaseData> NegativeCases()
    {
        yield return new TestCaseData(
            "Invalid .NET runtime data format: ''. Expected: 'Microsoft.NETCore.App 8.0.8 [/usr/share/dotnet/shared/Microsoft.NETCore.App]'",
            new TK
            {
                NetRuntimes = [string.Empty],
            })
        .SetName("Negative(Invalid NetRuntime format)");

        yield return new TestCaseData(
            "Invalid .NET runtime data format: 'asd dsd sdf'. Expected: 'Microsoft.NETCore.App 8.0.8 [/usr/share/dotnet/shared/Microsoft.NETCore.App]'",
            new TK
            {
                NetRuntimes = ["asd dsd sdf"],
            })
        .SetName("Negative(Invalid NetRuntime format. 2)");

        yield return new TestCaseData(
            "Unknown version format: 'asd'.",
            new TK
            {
                NetRuntimes = ["Microsoft.NETCore.App 8.0.8 [/usr/share/dotnet/shared/Microsoft.NETCore.App]"],
                AcgRuntimes = ["asd"],
                NswagRuntimes = ["Net80"],
            })
        .SetName("Negative(Invalid AcgRuntime format.)");

        yield return new TestCaseData(
            "Unknown version format: 'netasd'.",
            new TK
            {
                NetRuntimes = ["Microsoft.NETCore.App 8.0.8 [/usr/share/dotnet/shared/Microsoft.NETCore.App]"],
                AcgRuntimes = ["netasd"],
                NswagRuntimes = ["Net80"],
            })
        .SetName("Negative(Invalid AcgRuntime format. 2)");

        yield return new TestCaseData(
            "Unknown version format: 'asd'.",
            new TK
            {
                NetRuntimes = ["Microsoft.NETCore.App 8.0.8 [/usr/share/dotnet/shared/Microsoft.NETCore.App]"],
                AcgRuntimes = ["net8.0"],
                NswagRuntimes = ["asd"],
            })
        .SetName("Negative(Invalid AcgRuntime format.)");

        yield return new TestCaseData(
            "Unknown version format: 'Netasd'.",
            new TK
            {
                NetRuntimes = ["Microsoft.NETCore.App 8.0.8 [/usr/share/dotnet/shared/Microsoft.NETCore.App]"],
                AcgRuntimes = ["net8.0"],
                NswagRuntimes = ["Netasd"],
            })
        .SetName("Negative(Invalid AcgRuntime format. 2)");

        yield return new TestCaseData(
            "No compatible runtime version found.",
            new TK
            {
                NetRuntimes = ["Microsoft.NETCore.App 8.0.8 [/usr/share/dotnet/shared/Microsoft.NETCore.App]"],
                AcgRuntimes = ["net9.0"],
                NswagRuntimes = ["Net90"],
            })
        .SetName("Negative(No compatible runtime version found)");
    }

    public class TK
    {
        public TaskItem[] NetRuntimes { get; set; }

        public TaskItem[] AcgRuntimes { get; set; }

        public TaskItem[] NswagRuntimes { get; set; }

        public TaskItem AcgToolDir { get; set; }

        public TaskItem AcgNswagToolDir { get; set; }
    }

    public interface ITaskLoggingHelper
    {
        public void LogErrorFromException(Exception exception, bool showStackTrace, bool showDetail, string file);
    }

    private class SelectGeneratorRuntime
    {
        public SelectGeneratorRuntime(ITaskLoggingHelper log)
        {
            Log = log;
        }

        public SelectGeneratorRuntime(ITaskLoggingHelper log, TK testCase)
        {
            Log = log;
            NetRuntimes = testCase.NetRuntimes;
            AcgRuntimes = testCase.AcgRuntimes;
            NswagRuntimes = testCase.NswagRuntimes;
        }

        public ITaskLoggingHelper Log { get; }

        public TaskItem[] NetRuntimes { get; set; }

        public TaskItem[] AcgRuntimes { get; set; }

        public TaskItem[] NswagRuntimes { get; set; }

        public TaskItem AcgToolDir { get; set; }

        public TaskItem AcgNswagToolDir { get; set; }

        private bool Success { get; set; } = true;

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1130:Use lambda syntax", Justification = "For compatible")]
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1008:Opening parenthesis should be spaced correctly", Justification = "Autoformat.")]
        internal bool Execute()
        {
            // Start AcgGetTools task code
            var netRuntimeRegex = new System.Text.RegularExpressions.Regex(@"^Microsoft\.NETCore\.App\s(\S+)\s\[[^\]]+\]$");
            Func<string, int> parseMajorVersion = delegate (string versionString)
            {
                Exception innerEx = null;
                try
                {
                    if (char.IsDigit(versionString.FirstOrDefault()))
                    {
                        return int.Parse(versionString.Substring(0, versionString.IndexOf('.')));
                    }

                    if (versionString.StartsWith("net"))
                    {
                        var pos = versionString.IndexOf('.');
                        if (pos > 3)
                        {
                            return int.Parse(versionString.Substring(3, pos - 3));
                        }
                    }
                    else if (versionString.StartsWith("Net"))
                    {
                        return int.Parse(versionString.Substring(3)) / 10;
                    }
                }
                catch (FormatException ex)
                {
                    innerEx = ex;
                }

                throw new FormatException("Unknown version format: '" + versionString + "'.", innerEx);
            };

            Func<string, int> parseNetRuntimeMajorVersion = delegate (string versionString)
            {
                var match = netRuntimeRegex.Match(versionString);
                if (match.Success)
                {
                    return parseMajorVersion(match.Groups[1].Value);
                }

                throw new FormatException(
                    string.Format(
                        "Invalid .NET runtime data format: '{0}'. Expected: 'Microsoft.NETCore.App 8.0.8 [/usr/share/dotnet/shared/Microsoft.NETCore.App]'",
                        versionString));
            };

            try
            {
                var runtimes = NetRuntimes
                    .Select(r => parseNetRuntimeMajorVersion(r.ItemSpec))
                    .OrderByDescending(v => v)
                    .ToArray();

                var map = AcgRuntimes
                    .Join(
                        NswagRuntimes,
                        l => parseMajorVersion(l.ItemSpec),
                        r => parseMajorVersion(r.ItemSpec),
                        (l, r) =>
                        {
                            var v = parseMajorVersion(l.ItemSpec);
                            return new
                            {
                                Version = v,
                                Acg = l,
                                Nswag = r,
                                Net = runtimes.Contains(v) ? v : runtimes.FirstOrDefault(r => r > v),
                            };
                        })
                    .OrderByDescending(i => i.Version);

                var selected =
                    map.FirstOrDefault(i => i.Version == i.Net)
                    ?? map.FirstOrDefault(i => i.Version < i.Net);

                if (selected != null)
                {
                    AcgToolDir = selected.Acg;
                    AcgNswagToolDir = selected.Nswag;

                    return true;
                }

                AcgToolDir = null;
                AcgNswagToolDir = null;
                Log.LogErrorFromException(new Exception("No compatible runtime version found."), false, false, null);
                return false;
            }
            catch (FormatException ex)
            {
                Log.LogErrorFromException(ex, false, false, null);
                Success = false;
            }

            // End AcgGetTools task code

            // The code below adds RoslynCodeFactory. Do not copy to the Task
            return Success;
        }
    }

    public class TaskItem
    {
        public TaskItem(string val)
        {
            ItemSpec = val;
        }

        public string ItemSpec { get; }

        public static implicit operator TaskItem(string itemSpec)
        {
            return new TaskItem(itemSpec);
        }

        public override string ToString() => ItemSpec;
    }
}

#pragma warning restore SA1201 // Elements should appear in the correct order
#pragma warning restore SA1202 // Elements should be ordered by access
#pragma warning restore SA1204 // Static elements should appear before instance elements
