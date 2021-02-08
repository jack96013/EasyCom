namespace EasyCom.CustomStr
{
    public class StrItemModel : DragableListModelBase
    {
        public StrItemModel(CustomStrData strList)
        {
            Data = strList;
        }

        private CustomStrData data = null;
        public CustomStrData Data
        {
            get {
                return data;
            }
            set
            {
                if (value != this.Data)
                    data = value;
                this.SetPropertyChanged(nameof(Data));
            }
        }

        private bool _checked = false;
        public bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                if (value != this._checked)
                    _checked = value;
                this.SetPropertyChanged(nameof(Checked));
            }
        }

        public bool PreviewMode { get; set; } = false;
    }


}
