using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EasyCom.Connection
{
    public class ProcessHandle
    {
        static internal ArrayList myProcessArray = new ArrayList();
        private static Process myProcess;

        public ProcessHandle()
        {
            string strFile = "\\Device\\Serial12";
            return;
            ArrayList a = getFileProcesses(strFile);
            foreach (Process p in a)
            {
                Console.WriteLine(p.ProcessName);
            }
        }

        private static ArrayList getFileProcesses(string strFile)
        {
            myProcessArray.Clear();
            Process[] processes = Process.GetProcesses();
            int i = 0;
            foreach (Process myProcess in processes)
            {
                
                Console.WriteLine(myProcess.ProcessName);
                if (myProcess.ProcessName == "sscom5.13.1")
                {
                    var hwnd = myProcess.Handle;
                    Console.WriteLine(hwnd);
                }
                bool hasExited=false;
                try {
                    hasExited = myProcess.HasExited;
                }
                catch (Exception e)
                {
                    hasExited = true;
                }
                if (!hasExited)
                {
                    try
                    {
                        
                        //ProcessModuleCollection modules = myProcess.Modules;
                        //int j = 0;
                        //for (j = 0; j <= modules.Count - 1; j++)
                        //{
                        //    //Console.WriteLine("\t\t" + modules[j]);
                        //    if ((modules[j].FileName.ToLower().CompareTo(strFile.ToLower()) == 0))
                        //    {
                        //        myProcessArray.Add(myProcess);
                                
                        //        break; // TODO: might not be correct. Was : Exit For 
                        //    }
                        //}
                    }
                    catch (Exception exception)
                    {
                    }
                    //MsgBox(("Error : " & exception.Message)) 
                }
            }
            return myProcessArray;
        }
    }

    
}
