using CadEye_WebVersion.ViewModels.Messages.SplashMessage;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ZWCAD;


namespace CadEye_WebVersion.Services.Zwcad
{
    public class ZwcadService : IZwcadService
    {
        public ZwcadService()
        {
            CreateInitialInstances();
            StartMonitoring();
        }


        public Dictionary<ZcadApplication, bool> Zwcads = new Dictionary<ZcadApplication, bool>();

        #region Zwcad 생성 및 감시
        private void CreateInitialInstances()
        {
            try
            {
                for (int i = 0; i < AppSettings.ZwcadThreads; i++)
                {
                    ZcadApplication _zwcad = new ZcadApplication();
                    _zwcad.Visible = false;
                    Zwcads.Add(_zwcad, false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ZWCAD Instance 생성 실패: {ex.Message}");
            }
        }

        private void StartMonitoring()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    lock (_lock)
                    {
                        int runningCount = Process.GetProcessesByName("ZWCAD").Length;
                        int toCreate = AppSettings.ZwcadThreads - runningCount;

                        for (int i = 0; i < toCreate; i++)
                        {
                            try
                            {
                                ZcadApplication _zwcad = new ZcadApplication();
                                _zwcad.Visible = false;
                                Zwcads.Add(_zwcad, false);
                                WeakReferenceMessenger.Default.Send(new SplashMessage($"Zwcad Instance Created by Monitor"));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"ZWCAD Monitoring 생성 실패: {ex.Message}");
                            }
                        }
                    }
                    await Task.Delay(500);
                }
            });
        }


        #endregion

        private object _lock = new object();
        public List<string> WorkFlow_Zwcad(string path)
        {
            try
            {
                List<string> sender = new List<string>();
                Thread staThread = new Thread(() =>
                {
                    ZcadApplication assigned = null;

                    lock (_lock)
                    {
                        while (true)
                        {
                            foreach (var zwcad in Zwcads)
                            {
                                if (!zwcad.Value)
                                {
                                    Zwcads[zwcad.Key] = true;
                                    assigned = zwcad.Key;
                                    break;
                                }
                            }
                            if (assigned != null)
                                break;
                        }
                    }


                    assigned.Visible = false;

                    try
                    {
                        sender = Cad_Text_Extrude(path, assigned);
                    }
                    finally
                    {
                        lock (_lock)
                        {
                            Zwcads[assigned] = false;
                            Debug.WriteLine("해제 성공");
                        }
                    }
                });
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join();
                return sender;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public List<string> Cad_Text_Extrude(string path, ZcadApplication _zwcad)
        {
            try
            {
                List<string> autocad_text = new List<string>();
                string ex = System.IO.Path.GetExtension(path);
                if (ex.ToUpper() == ".DWG" || ex.ToUpper() == "DXF") { }
                else return new List<string>();

                if (!System.IO.File.Exists(path))
                {
                    Debug.WriteLine("파일 오류: " + path);
                    return new List<string>();
                }

                var zwcad_in = _zwcad.Documents.Open(path, true);
                var layouts = zwcad_in.Layouts;

                if (layouts == null) { return new List<string>(); }


                Plot_Check(layouts);

                string PlotPaht = System.IO.Path.Combine(AppSettings.RepositoryPdfFolder, System.IO.Path.GetFileName(path));
                zwcad_in.Plot.PlotToFile(PlotPaht);

                var buffer1 = new ConcurrentBag<string>();

                Parallel.ForEach(zwcad_in.ModelSpace.Cast<object>(), obj_entity =>
                {
                    var zwcad_entity = obj_entity as ZcadEntity;
                    if (zwcad_entity == null) return;
                    string content = "";
                    string entityName = zwcad_entity.EntityName.ToUpper();

                    switch (entityName)
                    {
                        case "ACDBTEXT":
                            content = ((ZcadText)zwcad_entity).TextString;
                            content = Text_Convey(content);
                            break;
                        case "ACDBMTEXT":
                            content = ((ZcadMText)zwcad_entity).TextString;
                            content = Text_Convey(content);
                            break;
                        default:
                            return;
                    }

                    var newItems = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var item in newItems)
                        buffer1.Add(item);
                });

                autocad_text.AddRange(buffer1);

                Marshal.FinalReleaseComObject(zwcad_in.ModelSpace);
                Marshal.FinalReleaseComObject(zwcad_in.Layouts);
                Marshal.FinalReleaseComObject(zwcad_in.Plot);

                zwcad_in.Close();
                Marshal.FinalReleaseComObject(zwcad_in);

                return autocad_text;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cad_Text_Extrude : Error {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return new List<string>();
            }
        }

        public void Plot_Check(ZcadLayouts layouts)
        {
            try
            {
                var modelLayout = layouts.Item("Model");
                var plotters = modelLayout.GetPlotDeviceNames();
                string device = "";

                foreach (string plotter in plotters)
                {
                    if (plotter == "ZWCAD PDF(High Quality Print).pc5")
                    {
                        device = plotter;
                        break;
                    }
                }

                if (device == null)
                {
                    foreach (string plotter in plotters)
                    {
                        if (plotter == "DWG To PDF.pc5")
                        {
                            device = plotter;
                            break;
                        }
                    }
                }

                if (device == null)
                {
                    foreach (string plotter in plotters)
                    {
                        if (plotter == "ZWCAD PDF(General Documentation).pc5")
                        {
                            device = plotter;
                            break;
                        }
                    }
                }

                modelLayout.ConfigName = device;
                modelLayout.RefreshPlotDeviceInfo();
                modelLayout.CanonicalMediaName = "A1";
                modelLayout.PlotWithPlotStyles = true;
                modelLayout.CenterPlot = true;
                modelLayout.PlotRotation = ZcPlotRotation.zc0degrees;
                modelLayout.PlotType = ZcPlotType.zcExtents;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Plot_Check : Error {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }
        private static string Text_Convey(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            try
            {
                text = text.Replace("{", "").Replace("}", "");
                text = Regex.Replace(text, @"\\[^;]+;", "");
                text = text.Replace(@"\P", "\n");
                text = text.Trim();

                // <ref>...</ref> 안의 내용만 추출
                var match = Regex.Match(text, @"<\s*ref\s*>(.*?)<\s*/\s*ref\s*>",
                                        RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (match.Success)
                {
                    text = match.Groups[1].Value;
                }
                else
                {
                    text = string.Empty; // 혹은 원래 text 반환하고 싶으면 그대로 둬도 됨
                }

                return text.Trim();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[Error] {ex.Message}");
                Debug.WriteLine($"[StackTrace] {ex.StackTrace}");
                return text.Trim();
            }
        }
    }
}
