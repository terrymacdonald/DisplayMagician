using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using WindowsDisplayAPI;
using WindowsDisplayAPI.DisplayConfig;
using HeliosPlus.Shared;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Mosaic;

namespace HeliosPlus.Reporting
{
    internal class Program
    {
        private static StreamWriter _writer;
        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HeliosPlus");

        private static void Dump<T>(
            IEnumerable<T> items,
            string title,
            Tuple<Func<T, object>, string>[] actions = null,
            int deepIn = 0)
        {
            Console.WriteLine(title);
            _writer.WriteLine(title + new string('=', Console.BufferWidth - title.Length));
            var totalTime = TimeSpan.Zero;
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var item in items)
            {
                var actionResults = actions?.Select(tuple =>
                    new Tuple<string, object>(tuple.Item2, tuple.Item1.Invoke(item))).ToArray();
                stopWatch.Stop();
                WriteObject(item, 0, stopWatch.Elapsed, actionResults, deepIn);
                totalTime += stopWatch.Elapsed;
                stopWatch.Reset();
                stopWatch.Start();
            }

            stopWatch.Stop();
            totalTime += stopWatch.Elapsed;
            Console.Write(@"-- Elapsed: {0}", totalTime);
            _writer.WriteLine(@"-- Total Elapsed: {0}", totalTime);
            Console.WriteLine();
            _writer.WriteLine();
        }

        private static void Main(string[] args)
        {
            _writer = new StreamWriter(new FileStream(
                string.Format("HeliosPlus.Reporting.{0}.log", Process.GetCurrentProcess().Id),
                FileMode.CreateNew));

            try
            {
                Dump(DisplayAdapter.GetDisplayAdapters(), "WindowsDisplayAPI.DisplayAdapter.GetDisplayAdapters()");
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            try
            {
                Dump(Display.GetDisplays(), "WindowsDisplayAPI.Display.GetDisplays()", new[]
                {
                    new Tuple<Func<Display, object>, string>(display => display.GetPossibleSettings(),
                        "GetPossibleSettings()")
                });
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            try
            {
                Dump(UnAttachedDisplay.GetUnAttachedDisplays(),
                    "WindowsDisplayAPI.UnAttachedDisplay.GetUnAttachedDisplays()");
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            try
            {
                Dump(PathDisplayAdapter.GetAdapters(),
                    "WindowsDisplayAPI.DisplayConfig.PathDisplayAdapter.GetAdapters()",
                    new[]
                    {
                        new Tuple<Func<PathDisplayAdapter, object>, string>(adapter => adapter.ToDisplayAdapter(),
                            "ToDisplayAdapter()")
                    });
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            try
            {
                Dump(PathDisplaySource.GetDisplaySources(),
                    "WindowsDisplayAPI.DisplayConfig.PathDisplaySource.GetDisplaySources()", new[]
                    {
                        new Tuple<Func<PathDisplaySource, object>, string>(source => source.ToDisplayDevices(),
                            "ToDisplayDevices()")
                    });
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            try
            {
                Dump(PathDisplayTarget.GetDisplayTargets(),
                    "WindowsDisplayAPI.DisplayConfig.PathDisplayTarget.GetDisplayTargets()", new[]
                    {
                        new Tuple<Func<PathDisplayTarget, object>, string>(target => target.ToDisplayDevice(),
                            "ToDisplayDevice()")
                    });
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            try
            {
                if (PathInfo.IsSupported)
                {
                    Dump(PathInfo.GetActivePaths(), "WindowsDisplayAPI.DisplayConfig.PathInfo.GetActivePaths()", null,
                        2);
                }
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            try
            {
                Dump(LogicalGPU.GetLogicalGPUs(), "NvAPIWrapper.GPU.LogicalGPU.GetLogicalGPUs()", null, 1);
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            try
            {
                Dump(PhysicalGPU.GetPhysicalGPUs(), "NvAPIWrapper.GPU.PhysicalGPU.GetPhysicalGPUs()");
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            try
            {
                Dump(NvAPIWrapper.Display.Display.GetDisplays(), "NvAPIWrapper.Display.Display.GetDisplays()", new[]
                {
                    new Tuple<Func<NvAPIWrapper.Display.Display, object>, string>(
                        display => display.GetSupportedViews(),
                        "GetSupportedViews()")
                });
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            try
            {
                Dump(NvAPIWrapper.Display.UnAttachedDisplay.GetUnAttachedDisplays(),
                    "NvAPIWrapper.Display.UnAttachedDisplay.GetUnAttachedDisplays()");
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            try
            {
                Dump(NvAPIWrapper.Display.PathInfo.GetDisplaysConfig(),
                    "NvAPIWrapper.Display.PathInfo.GetDisplaysConfig()",
                    null, 3);
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            try
            {
                Dump(GridTopology.GetGridTopologies(), "NvAPIWrapper.Mosaic.GridTopology.GetGridTopologies()", null, 3);
            }
            catch (Exception e)
            {
                WriteException(e);
            }


            try
            {
                Dump(ProfileItem.LoadAllProfiles(), "HeliosPlus.Shared.Profile.GetAllProfiles()", null, 99);
            }
            catch (Exception e)
            {
                WriteException(e);
            }

            _writer.Flush();
            _writer.Close();
            _writer.Dispose();

            Console.WriteLine(@"Done, press enter to exit.");
            Console.ReadLine();
        }

        private static void WriteException(Exception ex)
        {
            _writer.WriteLine("{0} - Error: {1}", ex.GetType().Name, ex.Message);
        }

        private static void WriteObject(
            object obj,
            int padding = 0,
            TimeSpan elapsed = default(TimeSpan),
            Tuple<string, object>[] extraProperties = null,
            int deepIn = 0)
        {
            try
            {
                if (padding == 0)
                {
                    if (!elapsed.Equals(TimeSpan.Zero))
                    {
                        var elapsedFormated = string.Format("_{0}_", elapsed);
                        _writer.WriteLine(elapsedFormated +
                                          new string('_', Console.BufferWidth - elapsedFormated.Length));
                    }
                    else
                    {
                        _writer.WriteLine(new string('_', Console.BufferWidth));
                    }

                    _writer.WriteLine("({0}) {{", obj.GetType().Name);
                }

                if (obj.GetType().IsValueType || obj is string)
                {
                    _writer.WriteLine(new string(' ', padding * 3 + 2) + obj);
                }
                else if (obj is IEnumerable)
                {
                    var i = 0;

                    foreach (var arrayItem in (IEnumerable) obj)
                    {
                        _writer.WriteLine(new string(' ', padding * 3 + 2) + "[{0}]: ({1}) {{", i,
                            arrayItem?.GetType().Name);
                        WriteObject(arrayItem, padding + 1, default(TimeSpan), null, deepIn - 1);
                        _writer.WriteLine(new string(' ', padding * 3 + 2) + "},");
                        i++;
                    }
                }
                else
                {
                    foreach (var propertyInfo in obj.GetType().GetProperties().OrderBy(info => info.Name))
                    {
                        if (propertyInfo.CanRead)
                        {
                            object value;

                            try
                            {
                                value = propertyInfo.GetValue(obj);
                            }
                            catch (TargetInvocationException ex)
                            {
                                value = ex.InnerException?.GetType().ToString();
                            }
                            catch (Exception ex)
                            {
                                value = ex.GetType().ToString();
                            }

                            if (deepIn > 0 &&
                                value != null &&
                                !value.GetType().IsValueType &&
                                value.GetType() != typeof(string))
                            {
                                _writer.WriteLine(new string(' ', padding * 3 + 2) + "{0}: ({1}) {{", propertyInfo.Name,
                                    propertyInfo.PropertyType.Name);
                                WriteObject(value, padding + 1, default(TimeSpan), null, deepIn - 1);
                                _writer.WriteLine(new string(' ', padding * 3 + 2) + "},");
                            }
                            else
                            {
                                _writer.WriteLine(
                                    $"{new string(' ', padding * 3 + 2)}{propertyInfo.Name}: {value ?? "[NULL]"},");
                            }
                        }
                    }
                }

                if (extraProperties != null)
                {
                    foreach (var extraProperty in extraProperties)
                    {
                        _writer.WriteLine(new string(' ', padding * 3 + 2) + "{0}: ({1}) {{", extraProperty.Item1,
                            extraProperty.Item2?.GetType().Name);
                        WriteObject(extraProperty.Item2, padding + 1, default(TimeSpan), null, deepIn);
                        _writer.WriteLine(new string(' ', padding * 3 + 2) + "},");
                    }
                }

                if (padding == 0)
                {
                    _writer.WriteLine("};");
                    _writer.WriteLine(new string('_', Console.BufferWidth));
                }
            }
            catch (Exception ex)
            {
                WriteException(ex);
            }
        }
    }
}