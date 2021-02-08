using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EasyCom.General
{
    public class PopupDialogHost
    {
        public Frame DialogFrame { get; set; } = null;
        public Panel ContentPanel { get; set; } = null;
        private Storyboard ScaleIn { get; set; } = null;
        private Storyboard ScaleOut { get; set; } = null;

        private PopupDialog dialog = null;

        public PopupDialog CurrentDialog {
            get
            {
                return dialog;
            }
            set
            {
                if (value != null)
                {
                    dialog = value;
                    dialog.PopUpDialogHost = this;
                }
            }
        }

        /*public IPopupDialog Dialog
        {
            get
            {
                return _dialog;
            }
            set
            {
                if (value != _dialog)
                    Clear();
                _dialog = value;
                _dialog.popupDialogHost = this;
                _dialog.onLoaded();
            }
        }*/

        public PopupDialogHost(Frame DialogFrame, Panel ContentPanel)
        {
            this.DialogFrame = DialogFrame;
            this.ContentPanel = ContentPanel;
            Panel.SetZIndex(this.DialogFrame, 1);

            ((Panel)DialogFrame.Parent).MouseDown += DialogFrameParentClick;
            PopUpAnimInit();
        }

        private void PopUpAnimInit()
        {
            Duration defaultDuration = new Duration(TimeSpan.FromSeconds(1));
            ScaleIn = new Storyboard();
            ScaleOut = new Storyboard();
            ScaleTransform Orig = new ScaleTransform();
            Orig.ScaleX = 0.2;
            Orig.ScaleY = 0.2;
            this.DialogFrame.RenderTransform = Orig;
            this.DialogFrame.RenderTransformOrigin = new Point(0.5, 0.5);

            //ScaleIn
            DoubleAnimation AnimInX = new DoubleAnimation();
            AnimInX.From = 0;
            AnimInX.To = 1;
            AnimInX.Duration = defaultDuration;
            Storyboard.SetTarget(AnimInX, DialogFrame);
            Storyboard.SetTargetProperty(AnimInX, new PropertyPath("RenderTransform.ScaleX"));

            DoubleAnimation AnimInY = new DoubleAnimation();
            AnimInY.From = 0;
            AnimInY.To = 1;
            AnimInY.Duration = defaultDuration;
            Storyboard.SetTarget(AnimInY, DialogFrame);
            Storyboard.SetTargetProperty(AnimInY, new PropertyPath("RenderTransform.ScaleY"));

            ScaleIn.Children.Add(AnimInX);
            ScaleIn.Children.Add(AnimInY);

            //ScaleOut
            DoubleAnimation AnimOutX = new DoubleAnimation();
            AnimOutX.To = 0;
            AnimOutX.Duration = defaultDuration;
            Storyboard.SetTarget(AnimOutX, DialogFrame);
            Storyboard.SetTargetProperty(AnimOutX, new PropertyPath("RenderTransform.ScaleX"));

            DoubleAnimation AnimOutY = new DoubleAnimation();
            AnimOutY.To = 0;
            AnimOutY.Duration = defaultDuration;
            Storyboard.SetTarget(AnimOutY, DialogFrame);
            Storyboard.SetTargetProperty(AnimOutY, new PropertyPath("RenderTransform.ScaleY"));

            ScaleOut.Children.Add(AnimOutX);
            ScaleOut.Children.Add(AnimOutY);
        }

        public void Show()
        {
            Show(CurrentDialog);
        }
        public void Show(PopupDialog dialog)
        {
            if (dialog != CurrentDialog)
            {
                Close(CurrentDialog);
            }
            CurrentDialog = dialog;
            
            foreach (DoubleAnimation animation in ScaleIn.Children)
            {
                animation.Duration = new Duration(CurrentDialog.ScaleAnimationTime);
            }
            foreach (DoubleAnimation animation in ScaleOut.Children)
            {
                animation.Duration = new Duration(CurrentDialog.ScaleAnimationTime);
            }

            CurrentDialog.WindowStatus = PopupDialog.Status.Show;
            CurrentDialog.Page.OnLoaded();
            CurrentDialog.OnOpenInvoke();
            ShowFrame();
            ScaleIn.Begin();
        }

        private void ShowFrame()
        {
            this.DialogFrame.Content = CurrentDialog.Page;
            ContentPanel.IsHitTestVisible = false;
            ContentPanel.Opacity = CurrentDialog.OpacityPercent;
            DialogFrame.Visibility = Visibility.Visible;
        }

        public void Close()
        {
            Close(CurrentDialog);
            
        }
        public void Close(PopupDialog dialog)
        {
            if (CurrentDialog == dialog)
            {
                if (CurrentDialog != null)
                {
                    CurrentDialog.Page.OnClose();
                    CurrentDialog.OnCloseInvoke();
                    CurrentDialog.WindowStatus = PopupDialog.Status.Close;
                    CloseFrame();
                }
                /*
                ScaleOut.Begin();
                ScaleOut.Completed += (s,e) => {
                    Debug.WriteLine(s.ToString());
                    ContentPanel.IsHitTestVisible = true;
                    ContentPanel.Opacity = 1;
                    this.DialogFrame.Content = null;
                    DialogFrame.Visibility = Visibility.Hidden;
                };
                */
            }
        }

        private void CloseFrame()
        {
            this.DialogFrame.Content = null;
            ContentPanel.IsHitTestVisible = true;
            ContentPanel.Opacity = 1;
            DialogFrame.Visibility = Visibility.Hidden;
        }

        public void Clear()
        {
            if (ContentPanel.IsHitTestVisible)
            {
                Close();
            }
        }

        private void DialogFrameParentClick(object s, MouseButtonEventArgs e)
        {
            if (CurrentDialog != null && CurrentDialog.AllowClickMaskToClose)
            {
                Point point = e.GetPosition(DialogFrame);
                double X = point.X;
                double Y = point.Y;
                
                Point contentPont = ContentPanel.TranslatePoint(new Point(0, 0), DialogFrame);

                if ((X < 0 && X > contentPont.X) || (Y < 0 && Y > contentPont.Y) || X > DialogFrame.ActualWidth || Y > DialogFrame.ActualHeight)
                {
                    CurrentDialog.OnCancelInvoke();
                    Close();
                }
            }
        }
        /// <summary>
        /// Replace Dialog and without trig any event
        /// </summary>
        /// <param name="dialog"></param>
        public void ReplaceDialog(PopupDialog dialog)
        {
            CloseFrame();
            
            if (dialog == null)
                return;
            if (dialog.WindowStatus == PopupDialog.Status.Close)
                return;
            //ScaleIn.Begin();
            CurrentDialog = dialog;
            ChangeAnimationDuration(CurrentDialog.ScaleAnimationTime);
            ScaleIn.Begin();
            ScaleIn.Seek(CurrentDialog.ScaleAnimationTime);
            

            ShowFrame();
        }

        private void ChangeAnimationDuration(TimeSpan span)
        {
            foreach (DoubleAnimation animation in ScaleIn.Children)
            {
                animation.Duration = new Duration(span);
            }
            foreach (DoubleAnimation animation in ScaleOut.Children)
            {
                animation.Duration = new Duration(span);
            }
        }
    }
}
