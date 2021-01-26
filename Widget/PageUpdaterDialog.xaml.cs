using EasyCom.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyCom
{
    /// <summary>
    /// Dialog_Downloading.xaml 的互動邏輯
    /// </summary>
    public partial class PageUpdaterDialog : Page,IPopupDialog
    {
        public PopupDialog PopupDialog { get; }
        public DownloadInfo downloadInfo;
        public enum Statue{Download,Check};
        public Statue _currentStatue=Statue.Download;
        
        public Statue CurrentStatue
        {
            get { return _currentStatue; }
            set {
                if (value != _currentStatue)
                {
                    ApplyStatue(value);
                }
                _currentStatue = value;

            }
        }

        public void ApplyStatue(Statue statue)
        {
            System.Diagnostics.Debug.WriteLine("ApplySettings");
            this.PackIcon_StatueIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Download;
            
            this.Label_StatueDescription.Content = "";
            this.Label_Progress.Content = "";
            this.Label_Speed.Content = "";
            this.Label_Percent.Content = "";
            this.ProgressBar_Progress.Value = 0;

            if (statue == Statue.Download)
            {
                this.PackIcon_StatueIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Download;
                this.Label_Statue.Content = "下載中";
                this.Label_Speed.Visibility = Visibility.Visible;
            }
            else if (statue == Statue.Check)
            {
                this.PackIcon_StatueIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.FileFindOutline;
                this.Label_Statue.Content = "檢查中";
                this.Label_Speed.Visibility = Visibility.Hidden;
            }

        }
        public PageUpdaterDialog()
        {
            InitializeComponent();
            PopupDialog = new PopupDialog(this);
            PopupDialog.AllowClickMaskToClose = false;
            this.ProgressBar_Progress.ValueChanged += ProgressBar_Progress_ValueChanged;
            this.Button_Cancel.Click += Button_Cancel_Click;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            PopupDialog.OnCancelInvoke();
            PopupDialog.Close();
        }

        private void ProgressBar_Progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Label_Percent.Content = string.Format("{0:N0}%", this.ProgressBar_Progress.Value);
        }

        public void OnUpdate()
        {
            if (CurrentStatue == Statue.Download)
            {
                this.Label_StatueDescription.Content = downloadInfo.CurrentFileName;
                this.Label_Progress.Content = string.Format("{0} / {1}", downloadInfo.CurrentProcess, downloadInfo.TotalProcess);


                this.ProgressBar_Progress.Value = downloadInfo.DownloadPercent;


                StringBuilder stringBuilder = new StringBuilder();

                if (downloadInfo.Speed < 1000)
                {
                    stringBuilder.Append(String.Format("{0:N2}", downloadInfo.Speed));
                    stringBuilder.Append("B/s");
                }
                else if (downloadInfo.Speed < 1000000)
                {
                    stringBuilder.Append(String.Format("{0:N2}", downloadInfo.Speed / 1000));
                    stringBuilder.Append("KB/s");
                }
                else
                {
                    stringBuilder.Append(String.Format("{0:N2}", downloadInfo.Speed / 1000000));
                    stringBuilder.Append("MB/s");
                }
                this.Label_Speed.Content = stringBuilder.ToString();

            }
            

                
        }

        public void OnLoaded()
        {
            OnUpdate();
        }

        public void OnClose()
        {

        }

        public class DownloadInfo
        {
            public string CurrentFileName = "";
            public uint TotalProcess = 0;
            public uint CurrentProcess = 0;
            public double Speed = 0;
            public int DownloadPercent;
            public DownloadInfo()
            {

            }
        }
    }


}
