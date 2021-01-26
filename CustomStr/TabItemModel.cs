namespace EasyCom.CustomStr
{
    public class TabItemModel : DragableListModelBase
    {
        public TabItemModel(CustomStrTab tab)
        {
            Data = tab;
        }
        

        private CustomStrTab data = null;
        public CustomStrTab Data
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
                this.SetPropertyChanged("Checked");
            }
        }

        
        public bool PreviewMode { get; set; } = false;
    }


}
