using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace MediatR.Benchmarks
{
    internal sealed class DotTraceDiagnoserAttribute : Attribute, IConfigSource
    {
        public DotTraceDiagnoserAttribute()
        {
            var manualConfig = ManualConfig.CreateEmpty();
            manualConfig.Add(new DotTraceDiagnoser());
            Config = manualConfig;
        }

        public IConfig Config { get; }
    }

    internal sealed class DotTraceDiagnoser : IDiagnoser
    {
        private Process _process;
        private string _saveLocation;

        public DotTraceDiagnoser()
        {
            _saveLocation = $"C:\\temp\\MyProject\\{DateTimeOffset.Now.UtcDateTime:yyyyMMddTHHmmss}.bench.dtp";
        }

        /// <inheritdoc />
        public RunMode GetRunMode(BenchmarkCase benchmarkCase) => RunMode.ExtraRun;

        /// <inheritdoc />
        public void Handle(HostSignal signal, DiagnoserActionParameters parameters)
        {
            switch (signal)
            {
                case HostSignal.BeforeActualRun:
                    try
                    {
                        var dotTracePath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            @"JetBrains\Installations\dotTrace192\ConsoleProfiler.exe");
                        var startInfo = new ProcessStartInfo(
                            dotTracePath,
                            $"attach {parameters.Process.Id} --save-to={_saveLocation} --profiling-type=Sampling")
                        {
                            RedirectStandardError
                                = true,
                            RedirectStandardOutput = true,
                            WindowStyle = ProcessWindowStyle.Normal,
                            UseShellExecute = false,
                        };
                        Console.WriteLine(startInfo.FileName);
                        Console.WriteLine(startInfo.Arguments);
                        _process = new Process
                        {
                            StartInfo = startInfo
                        };
                        _process.ErrorDataReceived += (sender, eventArgs) => Console.Error.WriteLine(eventArgs.Data);
                        _process.OutputDataReceived += (sender, eventArgs) => Console.WriteLine(eventArgs.Data);
                        _process.Start();
                        _process.BeginErrorReadLine();
                        _process.BeginOutputReadLine();
                        _process.Exited += (sender, args) => { _process.Dispose(); };
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.StackTrace);
                        throw;
                    }
                    break;
                case HostSignal.AfterActualRun:
                    break;
                case HostSignal.BeforeAnythingElse:
                    break;
                case HostSignal.AfterAll:
                    break;
                case HostSignal.SeparateLogic:
                    break;
                case HostSignal.BeforeProcessStart:
                    break;
                case HostSignal.AfterProcessExit:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(signal), signal, null);
            }
        }

        /// <inheritdoc />
        public IEnumerable<Metric> ProcessResults(DiagnoserResults results) => Enumerable.Empty<Metric>();

        /// <inheritdoc />
        public void DisplayResults(ILogger logger) { }

        /// <inheritdoc />
        public IEnumerable<ValidationError> Validate(ValidationParameters validationParameters) =>
            Enumerable.Empty<ValidationError>();

        /// <inheritdoc />
        public IEnumerable<string> Ids => new[] { nameof(DotTraceDiagnoser) };

        /// <inheritdoc />
        public IEnumerable<IExporter> Exporters => Enumerable.Empty<IExporter>();

        public IEnumerable<IAnalyser> Analysers { get; } = Enumerable.Empty<IAnalyser>();
    }
}