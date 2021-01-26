using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCom.General
{
    class ConsoleThread
    {
        private Thread t;
        private ControlCommandHandle commandHandle=null;

        public ControlCommandHandle CommandHandle { get => commandHandle; set => commandHandle = value; }
        private MainWindow mainWindow;
        public ConsoleThread(MainWindow mainWindow)
        {
            this.t = new Thread(new ThreadStart(run));
            this.t.IsBackground = true;
            this.mainWindow = mainWindow;
        }
        public void Start()
        {
            t.Start();
        }
        private void run()
        {
            //呼叫出Console視窗，這邊記得要先呼叫出來，再針對它來做其他的設定，以免出錯喔!!
            ConsoleHelper.Show();

            //設定Console視窗的大小，注意，這邊的寬和高指的是文字的行數和列數，而不是畫面上的點喔!!
            //Console.SetWindowSize(80, 24);

            //設定Console視窗的起始位置
            //Console.SetWindowPosition(0, 0);

            //設定Console視窗的標題

            Console.Title = "EasyCom Console";


            //設定文字前景色
            Console.ForegroundColor = ConsoleColor.Green;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);


            Console.WriteLine(String.Format(CultureInfo.CurrentCulture, "{0} Ver:{1}", fvi.ProductName.ToString(CultureInfo.CurrentCulture), fvi.ProductVersion.ToString(CultureInfo.CurrentCulture)));
            Console.WriteLine("Ready");
            //可以再次設定前景色，之後的文字就會以新的顏色列印

            string input = string.Empty;
            while (true)
            {
                input = Console.ReadLine();
                
                CommandSpilt(input);
            }
            mainWindow.Dispatcher.Invoke(()=> { App.Current.Shutdown(); });
            

        }

        public void CommandSpilt(string command)
        {
            int index = command.IndexOf(" ");
            string key=null, value = null;
            if (index != -1)
            {
                key = command.Substring(0, index);
                value = command.Substring(index + 1, command.Length - index - 1);
            }
            else
            {
                key = command;
            }
            if(key!=null&&CommandHandle!=null)
                CommandHandle.CommandParse(key,value);
        }
    }
}
