using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace EasyCom
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        private static string[] StartUpArgs;
        private bool OpenConsoleWhenNoArgument = false;
        private bool OpenConsoleWhenHasArgument_Default = false;
        private bool showConsole;

        public bool UpdateFinish { get; set; } = false;

        private static string DataDirectoryName = "data";
        public static string ExePath { get; } = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static string DataPath { get; } = System.IO.Path.Combine(ExePath, DataDirectoryName);

        private Dictionary<string,string> commandData = new Dictionary<string, string>();
        public Dictionary<string, string> CommandData { get => commandData; set => commandData = value; }
        public bool ShowConsole { get => showConsole; set => showConsole = value; }

        public App()
        {
            //AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                //ConsoleHelper.Hide();
                StartUpArgs = e.Args;
            }

            if (e != null && e.Args.Length > 0)
            {
                App.Current.StartupUri = new System.Uri("MainWindow/MainWindow.xaml", System.UriKind.RelativeOrAbsolute);
                ShowConsole = OpenConsoleWhenHasArgument_Default;
                ArgumentHandler(e.Args);
            }
            else
            {
                //onsoleHelper.ShowWindow(ConsoleHelper.GetConsoleWindow(), 5);
                App.Current.StartupUri = new System.Uri("MainWindow/MainWindow.xaml", System.UriKind.RelativeOrAbsolute);
                ShowConsole = OpenConsoleWhenNoArgument;

            }

            if (!ShowConsole)
                ConsoleHelper.Hide();

        }
        public void ArgumentHandler(string[] Args)
        {
            if (Args != null)
            {
                
                string KeyTemp = null;
                foreach (string arg in Args)
                {
                    bool isLast = Array.LastIndexOf(Args, arg) == Args.Length - 1;
                    //If the string is key
                    if (arg.IndexOf("-", 0, arg.Length, StringComparison.Ordinal) == 0)
                    {

                        string command = arg.Substring(1, arg.Length - 1);
                        Debug.WriteLine("Key=" + command);
                        //use -s to control easycom by console
                        if (command == "s")
                        {
                            ShowConsole = true;
                        }
                        else if (command == "c")
                        {

                        }
                        else if (command == "updated")
                        {
                            UpdateFinish = true;
                        }
                        else
                        {
                            //the key's value is still not Completed (KeyTemp is the last key)
                            if (KeyTemp != null)
                            {
                                CommandData.Add(KeyTemp, null);
                            }
                            if (isLast)
                                CommandData.Add(command, null);
                            else
                                KeyTemp = command;
                        }
                    }
                    //If the string is value
                    else
                    {
                        Debug.WriteLine("Value=" + arg);
                        if (KeyTemp != null)
                        {
                            CommandData.Add(KeyTemp, arg);
                            KeyTemp = null;
                        }
                    }
                }
            }
        }
    }


}
