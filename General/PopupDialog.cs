using System;
using System.Windows.Controls;

namespace EasyCom.General
{
    public class PopupDialog
    {
        public PopupDialogHost PopUpDialogHost {get;set;} = null;
        public double OpacityPercent { get; set; } = 0.25;
        public bool AllowClickMaskToClose { get; set; } = true;

        public Action<object, object> OnOpen { get; set; } = null;
        public object OnOpenArg { get; set; } = null;
        public Action<object, object> OnClose { get; set; } = null;
        public object OnCloseArg { get; set; } = null;

        public Action<object, object> OnCancel { get; set; } = null;
        public object OnCancelArg { get; set; } = null;
        public Action<object, object> OnConfirm { get; set; } = null;
        public object OnConfirmArg { get; set; } = null;
        
        public bool ScaleAnimationEnable { get; set; } = true;

        public int ScaleAnimationSpeed { get; set; } = 100;
        public IPopupDialog Page { get; set; }

        public PopupDialog(IPopupDialog page)
        {
            Page = page;
        }

        public void OnConfirmInvoke()
        {
            OnConfirm?.Invoke(Page,OnConfirmArg);
        }
        public void OnCancelInvoke()
        {
            OnCancel?.Invoke(Page,OnCancelArg);
        }
        public void OnOpenInvoke()
        {

            OnOpen?.Invoke(Page,OnOpenArg);
        }
        public void OnCloseInvoke()
        {
            OnClose?.Invoke(Page,OnCloseArg);
        }

        public void SetOnConfirm(Action<object,object> action,object arg)
        {
            OnConfirm = action;
            OnConfirmArg = arg;
        }
        public void SetOnCancel(Action<object,object> action,object arg)
        {
            OnCancel = action;
            OnCancelArg = arg;
        }
        public void SetOnOpen(Action<object,object> action,object arg)
        {
            OnOpen = action;
            OnOpenArg = arg;
        }
        public void SetOnClose(Action<object,object> action,object arg)
        {
            OnClose = action;
            OnCloseArg = arg;
        }

        public void Close()
        {
            PopUpDialogHost.Close(this);
        }
        public void Show()
        {
            PopUpDialogHost.Show(this);
        }
    }
}
