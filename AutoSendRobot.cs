using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCom
{
    public class AutoSendRobot : IDisposable
    {
        private ConnectionTabData Tab { get;}
        private uint alreadySendAmounts = 0;
        public uint AlreadySendAmounts { get => alreadySendAmounts; }
        private CancellationTokenSource cancellationTokenSource;

        private Task task;
        public Action<object,object> DataChanged;

        public AutoSendRobot(ConnectionTabData tab)
        {
            Tab = tab;
        }

        public void Start()
        {
            if (!Tab.Connected)
                return;
            Tab.ToolBarSetting.SendAutoSenderEnable = true;
            Tab.ToolBarSetting.SendAutoSenderCurrentAmount = 0;
            if (task != null && !task.IsCompleted)
            {
                Stop();
            }
            if (task != null)
            {
                
                task.Dispose();
                task = null;
            }
            
            CreateRobot();
            task.Start();
            Tab.ToolBarSetting.SendAutoSenderEnable = true;
            
        }
        public void Stop()
        {
            cancellationTokenSource?.Cancel();
            Tab.ToolBarSetting.SendAutoSenderEnable = false;
        }

        public bool IsRunning()
        {
            return task.Status == TaskStatus.Running;
        }
        private void CreateRobot()
        {

            cancellationTokenSource = new CancellationTokenSource();
            task = new Task(()=> {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (Tab.Connected)
                    {
                        bool successful = Tab.SendData(Tab.ToolBarSetting.SendText,DateTime.Now,false);
                        if (successful)
                        {
                            Tab.ToolBarSetting.SendAutoSenderCurrentAmount++;
                        }
                        else
                            break;
                        
                        bool selected = false;
                        App.Current.Dispatcher.InvokeAsync(()=> {
                            selected = Tab.IsSelected;
                            if (selected)
                            {
                                DataChanged?.Invoke(this, null);
                            }
                        });
                        
                        if (Tab.ToolBarSetting.SendAutoSenderAmount!= 0 && Tab.ToolBarSetting.SendAutoSenderCurrentAmount >= Tab.ToolBarSetting.SendAutoSenderAmount)
                            break;

                        Thread.Sleep((int)Tab.ToolBarSetting.SendAutoSenderInterval);
                    }

                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                
                Tab.ToolBarSetting.SendAutoSenderEnable = false;
                App.Current.Dispatcher.InvokeAsync(() => {
                    DataChanged?.Invoke(this, null);
                });
            });
        }

        public void Dispose()
        {
            task.Dispose();
        }
    }
}
