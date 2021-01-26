namespace EasyCom.CustomStr
{
    public class DragableListModelBase : ViewModelBase
    {
        public DragableListModelBase()
        {
        }

        private bool dragIndicatorUp = false;
        public bool DragIndicatorUp
        {
            get { return dragIndicatorUp; }
            set
            {
                dragIndicatorUp = value;
                this.SetPropertyChanged("dragIndicatorUp");
            }
        }

        private bool dragIndicatorDown = false;
        public bool DragIndicatorDown
        {
            get { return dragIndicatorDown; }
            set
            {
                dragIndicatorDown = value;
                this.SetPropertyChanged("DragIndicatorDown");
            }
        }
        public void ClearIndicator()
        {
            DragIndicatorUp = false;
            DragIndicatorDown = false;
        }

        private bool moveInIndicator = false;
        public bool MoveInIndicator
        {
            get{
                return moveInIndicator;
            }
            set
            {
                moveInIndicator = value;
                this.SetPropertyChanged("moveInIndicator");
            }
        }
    }


}
