using EasyCom.General;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EasyCom
{
    internal class Updater
    {
        private MainWindow parentWindow = null;
        private PopupDialogHost PopupDialogHost = null;

#if DEBUG
        private Uri updateURI = new Uri("https://rebrand.ly/easycom_d");
#else
        private Uri updateURI = new Uri("https://rebrand.ly/easycom");
#endif
        private string UpdatePackageUri = ".\\Update.zip";
        private static string UpdateFilePath = Path.Combine(App.DataPath, "update");
        private enum InstallWay { Internal, External, Manual };
        public Updater(MainWindow parentWindow)
        {
            this.ParentWindow = parentWindow;
            this.PopupDialogHost = parentWindow.PopupDialogHost;
        }
        public void CheckNewVersion()
        {
            Task.Factory.StartNew(UpdateCheck);
        }

        private UpdateInfo updateInfo = null;

        public MainWindow ParentWindow { get => parentWindow; set => parentWindow = value; }

        private async void UpdateCheck()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                using (HttpResponseMessage response = await httpClient.GetAsync(updateURI))
                
                using (HttpContent content = response.Content)
                {
                    // ← httpcontent 轉為 string
                    string result = await content.ReadAsStringAsync();
                    // linqpad 顯示資料
                    if (result != null)
                    {
                        Debug.WriteLine(result);
                        DataAnalyze(result);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("[Update]" + e.ToString());
            }
        }

        private void DataAnalyze(string data)
        {
            try
            {
                JObject parent = JObject.Parse(data);

                updateInfo = new UpdateInfo();

                JProperty JVersion = parent.Property("Version",StringComparison.InvariantCulture);
                JProperty JRelaeaseDate = parent.Property("ReleaseDate", StringComparison.InvariantCulture);
                JProperty JDownloadPage = parent.Property("DownloadPage", StringComparison.InvariantCulture);
                JProperty JPackage = parent.Property("Package", StringComparison.InvariantCulture);
                JProperty JMD5 = parent.Property("md5", StringComparison.InvariantCulture);
                JProperty JChangeLog = parent.Property("ChangeLog", StringComparison.InvariantCulture);
                JProperty JInstallMethod = parent.Property("InstallMethod", StringComparison.InvariantCulture);
                JProperty JInstaller = parent.Property("Installer", StringComparison.InvariantCulture);

                 

                if (JVersion != null)
                {
                    updateInfo.FileVersion = Version.Parse((string)JVersion.Value);
                }
                if (JRelaeaseDate != null)
                {
                    updateInfo.ReleaseDate = (string)JRelaeaseDate.Value;
                }
                if (JDownloadPage != null)
                {
                    updateInfo.DownloadPage = new Uri((string)JDownloadPage.Value);
                }
                if (JPackage != null)
                {
                    updateInfo.PackageUri = new Uri((string)JPackage.Value);
                }
                if (JInstaller != null)
                {
                    updateInfo.Installer = (string)JInstaller.Value;
                }
                if (JMD5 != null)
                {
                    updateInfo.FileMd5 = (string)JMD5.Value;
                }
                if (JChangeLog != null)
                {
                    updateInfo.ChangeLog = (string)JChangeLog.Value;
                }
                if (JInstallMethod != null)
                {
                    updateInfo.InstallMethod = (int)JInstallMethod.Value;
                }

                updateInfo.CurrentVersion = Version.Parse(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);

                if (updateInfo.FileVersion.CompareTo(updateInfo.CurrentVersion) == 1)
                {
                    //有更←
                    ParentWindow.Dispatcher.Invoke(() => { ParentWindow.Panel_UpdateNotify.Visibility = Visibility.Visible; });
                }
                else
                {
                    ParentWindow.Dispatcher.Invoke(() => { ParentWindow.Panel_UpdateNotify.Visibility = Visibility.Hidden; });


                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public void startUpdateCheck()
        {
            ParentWindow.Dispatcher.Invoke(() => {
                PageUpdateDialog dialog_Update = new PageUpdateDialog();
                dialog_Update.UpdateInfo = updateInfo;
                PopupDialogHost.CurrentDialog = dialog_Update.PopupDialog;
                PopupDialogHost.CurrentDialog.SetOnConfirm(Dialog_Update_Confirm, null); ;
                PopupDialogHost.Show();
            });
        }

        private void Dialog_Update_Confirm(object sender,object arg)
        {

            if (updateInfo.InstallMethod == (int)InstallWay.Manual)
            {
                //Open Web Browser
                System.Diagnostics.Process.Start("explorer.exe", updateInfo.DownloadPage.ToString());
            }
            else
                Task.Factory.StartNew(DownloadUpdateFile);
        }

        private async void DownloadUpdateFile()
        {
            PopupDialogHost.CurrentDialog.OnConfirm=Dialog_Update_Confirm;
            PageUpdaterDialog dialog_Updater = null;
            
            WebClient client = new WebClient();
            Debug.WriteLine("Intital Update fin");
            ParentWindow.Dispatcher.Invoke(() => {
                dialog_Updater = new PageUpdaterDialog();
                dialog_Updater.downloadInfo = new PageUpdaterDialog.DownloadInfo();
                dialog_Updater.CurrentStatue = PageUpdaterDialog.Statue.Download;
                dialog_Updater.downloadInfo.CurrentFileName = "EasyCom 更新包";
                dialog_Updater.downloadInfo.CurrentProcess = 1;
                dialog_Updater.downloadInfo.TotalProcess = 1;
                PopupDialogHost.CurrentDialog = dialog_Updater.PopupDialog;
                PopupDialogHost.CurrentDialog.SetOnCancel(DownloadUpdateFileCancel, client);
                PopupDialogHost.Show();
            });


            if (File.Exists(UpdatePackageUri))
            {
                File.Delete(UpdatePackageUri);
            }
            Stopwatch timer = new Stopwatch();
            long lastBytes = 0;
            int measurements = 0, maxDataPoints = 2;
            var dataPoints = new double[maxDataPoints];


            double TotalElapsed = 0;
            double TotalBytes = 0;
            using (client)
            {
                Debug.WriteLine(updateInfo.PackageUri);

                client.DownloadProgressChanged += ((s, e) => {
                    timer.Stop();
                    TotalElapsed += timer.ElapsedMilliseconds;
                    if (TotalElapsed > 1000)
                    {
                        TotalBytes += e.BytesReceived - lastBytes;
                        lastBytes = e.BytesReceived;
                        measurements = 0;
                        double downloadSpeed = TotalBytes / (TotalElapsed / 1000);
                        dialog_Updater.downloadInfo.Speed = downloadSpeed;
                        Debug.WriteLine("{0:N2} {1:N2} {2:N2}B/S", TotalElapsed, TotalBytes, downloadSpeed);
                        TotalElapsed = 0;
                        TotalBytes = 0;
                    }

                    timer.Restart();

                    dialog_Updater.downloadInfo.DownloadPercent = e.ProgressPercentage;
                    ParentWindow.Dispatcher.Invoke(() => { dialog_Updater.OnUpdate(); });

                });

                client.DownloadFileCompleted += ((s, e) => {
                    if (e.Cancelled)
                    {
                        UpdateCancel();
                    }
                    else
                    {
                        ParentWindow.Dispatcher.Invoke(() => {
                            dialog_Updater.CurrentStatue = PageUpdaterDialog.Statue.Check;
                            dialog_Updater.Label_StatueDescription.Content = "檢查檔案";
                            dialog_Updater.ProgressBar_Progress.Value = 0;

                            bool result = CheckFile(updateInfo.FileMd5, UpdatePackageUri);
                            dialog_Updater.ProgressBar_Progress.Value = 100;

                            string FinishMessage = null;
                            if (!result)
                            {
                                FinishMessage = "檔案校驗錯誤";
                            }
                            else if (updateInfo.InstallMethod == 0)
                            {
                                UnZip();
                            }

                            ReportResult(result, FinishMessage);
                        });
                    }
                });
                client.DownloadFileAsync(updateInfo.PackageUri, UpdatePackageUri);
            }

            try
            {

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            //System.AppDomain.CurrentDomain.FriendlyName
        }

        private void UnZip()
        {

            if (!Directory.Exists(UpdateFilePath))
            {
                Directory.CreateDirectory(UpdateFilePath);
            }
            else
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(UpdateFilePath);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                //Directory.Delete(UpdateFilePath);
            }



            // Ensures that the last character on the extraction path
            // is the directory separator char.
            // Without this, a malicious zip file could try to traverse outside of the expected
            // extraction path.
            if (!UpdateFilePath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                UpdateFilePath += Path.DirectorySeparatorChar;

            using (ZipArchive archive = ZipFile.OpenRead(UpdatePackageUri))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Gets the full path to ensure that relative segments are removed.
                    string destinationPath = Path.GetFullPath(Path.Combine(UpdateFilePath, entry.FullName));
                    Debug.WriteLine(destinationPath);

                    // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                    // are case-insensitive.
                    if (!destinationPath.EndsWith("\\") && destinationPath.StartsWith(UpdateFilePath, StringComparison.Ordinal))
                    {
                        string parentPath = Path.GetDirectoryName(destinationPath);
                        if (!Directory.Exists(parentPath))
                            Directory.CreateDirectory(parentPath);
                        entry.ExtractToFile(destinationPath);
                    }
                }
            }
        }

        private void DownloadUpdateFileCancel(object sender,object arg)
        {
            Debug.WriteLine("Cancel");
            ((WebClient)arg).CancelAsync();
        }

        private bool CheckFile(string JsonMd5, string FileUri)
        {
            string md5str = null;
            using (MD5 md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(FileUri))
                {
                    var hash = md5.ComputeHash(stream);
                    md5str = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            //updateInfo.FileMd5
            if (md5str.CompareTo(updateInfo.FileMd5) == 0)
            {
                return true;
            }
            return false;
        }

        private void ReportResult(bool success, string message)
        {
            PopupDialogHost.Clear();
            PageUpdateFinishDialog dialog_UpdateFinish = new PageUpdateFinishDialog();

            PopupDialogHost.CurrentDialog = dialog_UpdateFinish.PopupDialog;

            if (success)
            {
                dialog_UpdateFinish.currentStatue = PageUpdateFinishDialog.Statue.Success;
                dialog_UpdateFinish.currentStatue = PageUpdateFinishDialog.Statue.Fail;
                dialog_UpdateFinish.PackIcon_Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckCircleOutline;
                dialog_UpdateFinish.Label_Statue.Content = "成功";
                dialog_UpdateFinish.Label_StatueDescription.Content = "下載完成，按下\"是\"?軟體將會關閉並開始安裝!";
                dialog_UpdateFinish.PopupDialog.SetOnConfirm(InstallPackage,null);
                dialog_UpdateFinish.PopupDialog.SetOnCancel((s,e)=> { UpdateCancel(); },null);
                PopupDialogHost.Show(dialog_UpdateFinish.PopupDialog);
            }
            else
            {
                dialog_UpdateFinish.currentStatue = PageUpdateFinishDialog.Statue.Fail;
                dialog_UpdateFinish.PackIcon_Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Close;
                dialog_UpdateFinish.Label_Statue.Content = "失敗";
                dialog_UpdateFinish.Label_StatueDescription.Content = message + "\n是否要重試?";
                dialog_UpdateFinish.PopupDialog.SetOnConfirm((s,e) => { ParentWindow.Dispatcher.Invoke(() => { Dialog_Update_Confirm(null,null); }); },null);
                dialog_UpdateFinish.PopupDialog.SetOnCancel((s, e) => { UpdateCancel(); }, null);
            }

            //PopupDialogHost.OnConfirm = Dialog_UpdateFinish_Confirm;
            dialog_UpdateFinish.PopupDialog.SetOnCancel((sender,arg) => { ParentWindow.Dispatcher.Invoke(() => { PopupDialogHost.Close(); }); },null);
            PopupDialogHost.Show(dialog_UpdateFinish.PopupDialog);
        }

        private void Dialog_UpdateFinish_Confirm()
        {

        }

        private void UpdateCancel()
        {
            if (File.Exists(UpdatePackageUri))
            {
                File.Delete(UpdatePackageUri);
            }
        }

        private void InstallPackage(object sender,object arg)
        {
            string ExeFileName = System.AppDomain.CurrentDomain.FriendlyName;
#if DEBUG
            //ExeFileName = "test.exe";
#else
            Debug.WriteLine("Mode=Release"); 
#endif
            Debug.WriteLine(ExeFileName);
            /*
            string command = string.Format(CultureInfo.InvariantCulture,"/C \"timeout 3&&" +
                "del /f \"{0}\"&&" +
                "ren \".\\Update.bin\" \"{0}\"&&" +
                "start \"\" \"{0}\" -if \"", ExeFileName);
            */
            string TargetPath = App.ExePath;
            if (!App.ExePath.EndsWith("\\"))
                TargetPath += "\\";
            //string command = string.Format(CultureInfo.InvariantCulture, "/c \"{0}\" \"{1}\" \"{2}\"", Path.Combine(UpdateFilePath, updateInfo.Installer), ExeFileName, TargetPath);
            //string command = string.Format(CultureInfo.InvariantCulture, "/c pause");
            string command = string.Format(CultureInfo.InvariantCulture, "\"{0}\" \"{1}\"", ExeFileName, TargetPath);
            Debug.WriteLine(Path.Combine(UpdateFilePath, updateInfo.Installer));
            Debug.WriteLine(command);
            /*
            Process InstallProcess = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = command;
            InstallProcess.StartInfo = startInfo;
            InstallProcess.Start();
            */

            Process InstallProcess = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.FileName = updateInfo.Installer;
            startInfo.Arguments = command;
            startInfo.WorkingDirectory = UpdateFilePath;
            InstallProcess.StartInfo = startInfo;
            InstallProcess.Start();

            Application.Current.Shutdown();
        }

        public class UpdateInfo
        {
            private Version currentVersion = null;
            private Version fileVersion = null;
            private string releaseDate = null;
            private Uri downloadPage = null;
            private Uri packageUri = null;
            private string fileMd5 = null;
            private string changeLog = null;
            private int installMethod = 0;
            private string installer = null;

            public Version CurrentVersion { get => currentVersion; set => currentVersion = value; }
            public Version FileVersion { get => fileVersion; set => fileVersion = value; }
            public string ReleaseDate { get => releaseDate; set => releaseDate = value; }
            public Uri DownloadPage { get => downloadPage; set => downloadPage = value; }
            public Uri PackageUri { get => packageUri; set => packageUri = value; }
            public string FileMd5 { get => fileMd5; set => fileMd5 = value; }
            public string ChangeLog { get => changeLog; set => changeLog = value; }
            public int InstallMethod { get => installMethod; set => installMethod = value; }
            public string Installer { get => installer; set => installer = value; }

            public UpdateInfo()
            {

            }
        }

    }




}
