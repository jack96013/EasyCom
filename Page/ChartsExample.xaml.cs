using LiveCharts;
using LiveCharts.Wpf;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyCom
{
    /// <summary>
    /// ChartsExample.xaml 的互動邏輯
    /// </summary>
    public partial class ChartsExample : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }
        public ChartsExample()
        {
            InitializeComponent();


            SeriesCollection = new SeriesCollection()
            {
                new LineSeries
                {
                    Values = new ChartValues<float> { 3, 5, 7, 4 }
                }
            };
            
            DataContext = this;
        }
    }
}
