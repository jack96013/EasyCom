using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyCom
{
    /// <summary>
    /// CloseableHeader.xaml 的互動邏輯
    /// </summary>
    public partial class CloseableHeader : UserControl
    {
        public CloseableHeader()
        {
            InitializeComponent();
        }
    }

    public class ConnectionTabItem : TabItem
    {
        //https://www.codeproject.com/Articles/84213/How-to-add-a-Close-button-to-a-WPF-TabItem
        public CloseableHeader closeableTabHeader;
        private Action<Object> onCloseMethod;
        private bool available;
        public ConnectionTabData ConnectionTabData { get; }

        public ConnectionTabItem(ConnectionTabData connectionTabData)
        {
            // Create an instance of the usercontrol
            closeableTabHeader = new CloseableHeader();
            // Assign the usercontrol to the tab header
            this.Header = closeableTabHeader;
            closeableTabHeader.button_close.MouseEnter +=
            new MouseEventHandler(button_close_MouseEnter);
            closeableTabHeader.button_close.MouseLeave +=
               new MouseEventHandler(button_close_MouseLeave);

            closeableTabHeader.button_close.Click +=
               new RoutedEventHandler(button_close_Click);

            closeableTabHeader.label_TabTitle.SizeChanged +=
               new SizeChangedEventHandler(label_TabTitle_SizeChanged);

            this.Available = false;
            this.ConnectionTabData = connectionTabData;
        }

        public string Title
        {
            set
            {
                closeableTabHeader.label_TabTitle.Content = value;
            }
            get
            {
                return (string)closeableTabHeader.label_TabTitle.Content;
            }
        }

        public Action<Object> onClose
        {
            set
            {
                onCloseMethod = (Action<Object>)value;
            }
            get
            {
                return onCloseMethod; 
            }
        }
        public bool Available
        {
            set
            {
                this.available = value;
                if (available)
                    closeableTabHeader.availableIndicator.Fill = Brushes.Green;
                else
                    closeableTabHeader.availableIndicator.Fill = Brushes.Red;
            }
            get
            {
                return available;
            }
        }
        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Visible;
        }
        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Hidden;
        }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Visible;
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!this.IsSelected)
            {
                ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Hidden;
            }
        }
        void button_close_MouseEnter(object sender, MouseEventArgs e)
        {
            ((CloseableHeader)this.Header).button_close.Foreground = Brushes.Red;
        }
        // Button MouseLeave - When mouse is no longer over button - change color back to black
        void button_close_MouseLeave(object sender, MouseEventArgs e)
        {
            ((CloseableHeader)this.Header).button_close.Foreground = Brushes.Black;
        }
        // Button Close Click - Remove the Tab - (or raise
        // an event indicating a "CloseTab" event has occurred)
        void button_close_Click(object sender, RoutedEventArgs e)
        {
            onCloseMethod(this);
        }
        // Label SizeChanged - When the Size of the Label changes
        // (due to setting the Title) set position of button properly
        void label_TabTitle_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //((CloseableHeader)this.Header).button_close.Margin = new Thickness(
            //((CloseableHeader)this.Header).label_TabTitle.ActualWidth + 5, 3, 4, 0);
        }


    }
}
