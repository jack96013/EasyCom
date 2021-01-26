using EasyCom.CustomStr;
using EasyCom.General;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;


namespace EasyCom
{
    /// <summary>
    /// Dialog_BaudrateCustom.xaml 的互動邏輯
    /// </summary>
    public partial class PageBaudrateCustomDialog : Page,IPopupDialog
    {
        public PopupDialog PopupDialog { get; set; }
        public Action<uint> onAccept;
        public Action onCancel;
        public uint BaudrateToShow{ get; set; } = 0; //0=don't show baudrate

        public PageBaudrateCustomDialog()
        {
            InitializeComponent();
            PopupDialog = new PopupDialog(this);
        }




        private void DialogOnClose()
        {

        }

        

        private void DialogOnCancel()
        {
            onCancel?.Invoke();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex regex = new Regex(@"^\+?[1-9][0-9]*$");

            if (regex.IsMatch(((TextBox)sender).Text))
            {
                Button_Accept.IsEnabled = true;
            }
            else
            {
                Button_Accept.IsEnabled = false;
                Debug.WriteLine("clear");
            }
        }

        private void Button_Accept_Click(object sender, RoutedEventArgs e)
        {
            onAccept?.Invoke(Convert.ToUInt32(TextBox_Baud.Text));
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            onCancel?.Invoke();
        }

        private void TextBox_Baud_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                if (Button_Accept.IsEnabled == true)
                {
                    Button_Accept.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                }
            }
        }

        public void OnLoaded()
        {
            if (BaudrateToShow != 0)
            {
                TextBox_Baud.Text = BaudrateToShow.ToString();
                BaudrateToShow = 0; //clear
            }

        }

        public void OnClose()
        {
            
        }
    }
}
