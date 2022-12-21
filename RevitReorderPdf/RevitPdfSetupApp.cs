/*
   Copyright (C) 2018 Pheinex LLC

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>
 */

#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Diagnostics;
#endregion

namespace RevitReorderPdf
{
    class RevitPdfSetupApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            Logging.LogItem("Adding PDF Panel");
            AddPdfPanel(a);
            Logging.LogItem("PDF Panel added");

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        private void AddPdfPanel(UIControlledApplication uiControlledApplication)
        {
            try
            {
                var dllName = GetAssemblyName();
                var panelName = "PDF";
                var buttonName = string.Format("{0}Button", nameof(ReorderExistingPdfCommand));
                var buttonText = "Reorder\nExisting PDF";
                var buttonHint = "Reorder Existing PDF File Based on Sheet Index";
                var imageName = "RevitReorderPdf.Images.ReorderPdf_32x32.png";
                var className = typeof(ReorderExistingPdfCommand).FullName;

                var panel = uiControlledApplication.CreateRibbonPanel(panelName);
                
                var assemblyPath = GetAssemblyPath();

                PushButtonData pushButtonData = new PushButtonData(buttonName, buttonText, assemblyPath, className);
                PushButton pushButton = panel.AddItem(pushButtonData) as PushButton;
                pushButton.ToolTip = buttonHint;

                var image = NewBitmapImage(imageName);
                if (image != null) pushButton.LargeImage = image;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        private string GetAssemblyName()
        {
            return Path.GetFileName(GetAssemblyPath());
        }

        private string GetAssemblyPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }

        private BitmapImage NewBitmapImage(string imageName)
        {
            using (Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(imageName))
            {
                if (imageStream != null)
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = imageStream;
                    image.EndInit();
                    return image;
                }
                else
                {
                    return null;
                }
            }
        }
    }
    static class Logging
    {
        private const int MaxLogSize = 1048576;

        private static string loggingPath;

        private static string LoggingPath
        {
            get
            {
                if (loggingPath == null)
                {
                    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    loggingPath = Path.Combine(appDataPath, "Pheinex");
                    Debug.Print($"Log file path: {loggingPath}");
                }

                return loggingPath;
            }
        }
        public static void LogItem(string appName, string message)
        {
            try
            {
                Directory.CreateDirectory(LoggingPath);
                var filePath = Path.Combine(LoggingPath, $"{appName}.log");
                var append = true;
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length > MaxLogSize)
                    {
                        append = false;
                    }
                }

                var logMessage = $"{DateTime.Now}: {message}";
                using (var file = new StreamWriter(filePath, append))
                {
                    file.WriteLine(logMessage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.ToString());
            }
        }

        public static void LogItem(string message)
        {
            LogItem(Assembly.GetCallingAssembly().GetName(false).Name, message);
        }
    }
}
