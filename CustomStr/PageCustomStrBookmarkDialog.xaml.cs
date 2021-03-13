using EasyCom.General;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyCom.CustomStr
{
    /// <summary>
    /// PageCustomStrManager.xaml 的互動邏輯
    /// </summary>
    public partial class PageCustomStrBookmarkDialog : Page, IPopupDialog
    {
        public PopupDialog PopupDialog { get; }
        private List<CustomStrTab> TabList = null;

        private ListBoxViewModel<TabItemModel> tabListBoxViewModel = new ListBoxViewModel<TabItemModel>();
        private ListBoxViewModel<StrItemModel> strListBoxViewModel = new ListBoxViewModel<StrItemModel>();

        private const uint autoScrollThreshold = 10;

        private enum EditMode { EditTab, EditString, AddTab, AddString };
        private EditMode editMode = EditMode.EditTab;

        public PageCustomStrBookmarkDialog(List<CustomStrTab> tabList)
        {
            TabList = tabList;
            InitializeComponent();
            PopupDialog = new PopupDialog(this);
            PopupDialog.AllowClickMaskToClose = true;

            ListBox_Tab.SelectionChanged += ListBox_Tab_SelectionChanged;

            ListBox_Tab.PreviewMouseMove += ListBox_PreviewMouseMove;
            ListBox_Str.PreviewMouseMove += ListBox_PreviewMouseMove;

            ListBox_Tab.Drop += (sender, e) => { ListBox_Drop<TabItemModel>(sender, e); };
            ListBox_Str.Drop += (sender, e) => { ListBox_Drop<StrItemModel>(sender, e); };

            ListBox_Tab.SelectionChanged += ListBox_Tab_SelectionChanged;
            ListBox_Str.SelectionChanged += ListBox_Str_SelectionChanged;

            ListBox_Tab.PreviewDragOver += (sender, e) => { ListBox_DragOver<TabItemModel>(sender, e); };
            ListBox_Str.PreviewDragOver += (sender, e) => { ListBox_DragOver<StrItemModel>(sender, e); };

            ListBox_Tab.GotFocus += (sender, e) => { ItemInfoUpdate(EditMode.EditTab); };
            ListBox_Str.GotFocus += (sender, e) => { ItemInfoUpdate(EditMode.EditString); };

            this.ListBox_Tab.DataContext = tabListBoxViewModel;
            this.ListBox_Tab.ItemsSource = tabListBoxViewModel.ModelCollection;
            this.ListBox_Str.DataContext = strListBoxViewModel;
            this.ListBox_Str.ItemsSource = strListBoxViewModel.ModelCollection;

            this.CheckBoxTabAll.Click += CheckBoxTabAll_Click;
            this.CheckBoxStrAll.Click += CheckBoxStrAll_Click;

            this.ButtonTabAdd.Click += ButtonTabAdd_Click;
            this.ButtonTabDelete.Click += ButtonTabDelete_Click;

            this.ButtonStrAdd.Click += ButtonStrAdd_Click;
            this.ButtonStrDelete.Click += ButtonStrDelete_Click;


            this.ButtonEditConfirm.Click += ButtonEditConfirm_Click;
            this.ButtonEditCancel.Click += ButtonEditCancel_Click;

            this.TextBox_Name.TextChanged += TextBox_Name_TextChanged;
            this.TextBox_Description.TextChanged += TextBox_Description_TextChanged;
            this.TextBox_Command.TextChanged += TextBox_Command_TextChanged;


        }

        private void TextBox_Command_TextChanged(object sender, TextChangedEventArgs e)
        {
            InfoChangeCheck();
        }

        private void TextBox_Description_TextChanged(object sender, TextChangedEventArgs e)
        {
            InfoChangeCheck();
        }

        private void TextBox_Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            InfoChangeCheck();
        }

        private void ButtonEditCancel_Click(object sender, RoutedEventArgs e)
        {
            if (editMode == EditMode.EditTab || editMode == EditMode.EditString)
            {
                ItemInfoUpdate(editMode);
            }
        }

        private void ButtonEditConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (editMode == EditMode.EditString || editMode == EditMode.EditTab)
            {
                if (editMode == EditMode.EditTab)
                {
                    Object tab = ListBox_Tab.SelectedItem;
                    if (tab != null)
                    {
                        TabItemModel item = (TabItemModel)tab;

                        item.Data.Name = TextBox_Name.Text;
                        item.Data.Description = TextBox_Description.Text;
                        item.SetPropertyChanged("Data");
                    }
                }
                else
                {
                    Object str = ListBox_Str.SelectedItem;
                    if (str != null)
                    {
                        StrItemModel item = (StrItemModel)str;
                        item.Data.Name = TextBox_Name.Text;
                        item.Data.Description = TextBox_Description.Text;
                        item.Data.Text = TextBox_Command.Text;
                        item.SetPropertyChanged("Data");
                    }
                }
            }
            else
            {
                if (editMode == EditMode.AddTab)
                {
                    CustomStrTab newTab = new CustomStrTab(TextBox_Name.Text);
                    newTab.Description = TextBox_Description.Text;
                    TabList.Add(newTab);
                    TabItemModel newModel = new TabItemModel(newTab);
                    tabListBoxViewModel.ModelCollection.Add(newModel);
                    ListBox_Tab.SelectedItem = newModel;
                }
                else
                {
                    CustomStrData newStr = new CustomStrData(TextBox_Name.Text, TextBox_Description.Text, TextBox_Command.Text);
                    Object tab = ListBox_Tab.SelectedItem;
                    if (tab != null)
                    {
                        TabItemModel item = (TabItemModel)tab;
                        item.Data.StrList.Add(newStr);
                        TabStrRefresh(item.Data.StrList);
                        ListBox_Str.SelectedIndex = item.Data.StrList.Count - 1;
                    }
                }
            }
        }

        private void InfoChangeCheck()
        {
            bool changed = false;
            bool isNull = false;
            if (editMode == EditMode.EditTab || editMode == EditMode.EditString)
            {
                string Name = null;
                string Description = null;
                string Command = null;


                if (editMode == EditMode.EditTab)
                {
                    Object tab = ListBox_Tab.SelectedItem;
                    if (tab != null)
                    {
                        CustomStrTab tabData = ((TabItemModel)tab).Data;

                        Name = tabData.Name;
                        Description = tabData.Description;
                    }
                    else isNull = true;
                }
                else
                {
                    Object str = ListBox_Str.SelectedItem;
                    if (str != null)
                    {
                        CustomStrData strData = ((StrItemModel)str).Data;
                        Name = strData.Name;
                        Description = strData.Description;
                        Command = strData.Text;
                    }
                    else isNull = true;
                }
                if (!isNull)
                {
                    if (TextBox_Name.Text != Name)
                    {
                        changed = true;
                    }
                    else if (TextBox_Description.Text != Description)
                    {
                        changed = true;
                    }
                    else if (editMode == EditMode.EditString && TextBox_Command.Text != Command)
                    {
                        changed = true;
                    }
                }
                if (changed)
                {
                    ButtonEditConfirm.IsEnabled = (TextBox_Name.Text.Length != 0);
                    ButtonEditCancel.IsEnabled = true;
                }
                else
                {
                    ButtonEditConfirm.IsEnabled = false;
                    ButtonEditCancel.IsEnabled = false;
                }
            }
            else
            {
                ButtonEditConfirm.IsEnabled = (TextBox_Name.Text.Length != 0);
                ButtonEditCancel.IsEnabled = true;
            }

        }


        private void ButtonStrDelete_Click(object sender, RoutedEventArgs e)
        {
            List<StrItemModel> removeItemList = new List<StrItemModel>();
            foreach (StrItemModel item in strListBoxViewModel.ModelCollection)
            {
                if (item.Checked)
                {
                    removeItemList.Add(item);
                }
            }
            foreach (StrItemModel item in removeItemList)
            {
                strListBoxViewModel.ModelCollection.Remove(item);
            }
        }
        private void ButtonStrAdd_Click(object sender, RoutedEventArgs e)
        {
            ItemInfoUpdate(EditMode.AddString);
        }

        private void ButtonTabDelete_Click(object sender, RoutedEventArgs e)
        {
            List<TabItemModel> removeItemList = new List<TabItemModel>();
            foreach (TabItemModel item in tabListBoxViewModel.ModelCollection)
            {
                if (item.Checked)
                {
                    removeItemList.Add(item);
                }
            }
            foreach (TabItemModel item in removeItemList)
            {
                tabListBoxViewModel.ModelCollection.Remove(item);
            }
        }

        private void ButtonTabAdd_Click(object sender, RoutedEventArgs e)
        {
            ItemInfoUpdate(EditMode.AddTab);
        }

        private void CheckBoxTabAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItemModel item in tabListBoxViewModel.ModelCollection)
            {
                item.Checked = CheckBoxTabAll.IsChecked.Value;
            }
        }

        private void CheckBoxStrAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (StrItemModel item in strListBoxViewModel.ModelCollection)
            {
                item.Checked = CheckBoxTabAll.IsChecked.Value;
            }
        }

        private void ListBox_Tab_DragOver(object sender, DragEventArgs e)
        {

        }
        private void ListBox_Str_DragOver(object sender, DragEventArgs e)
        {
            ListBox_DragOver<StrItemModel>((ListBox)sender, e);
        }

        //FIXME: When command drag to tab and drag back,tab indicator not recovered;
        private void ListBox_DragOver<TItem>(object sender, DragEventArgs e) where TItem : DragableListModelBase
        {
            ListBox listbox = (ListBox)sender;
            ListBoxViewModel<TItem> viewModel = (ListBoxViewModel<TItem>)listbox.DataContext;

            Point pos = e.GetPosition(listbox);
            if (listbox.ActualHeight - pos.Y <= autoScrollThreshold)
            {
                //下捲
                Border border = (Border)VisualTreeHelper.GetChild(listbox, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 1);
            }
            else if (pos.Y <= autoScrollThreshold)
            {
                //上捲
                Border border = (Border)VisualTreeHelper.GetChild(listbox, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 1);
            }

            HitTestResult result = VisualTreeHelper.HitTest(listbox, pos);
            ListBoxItem listBoxItem;

            if (result != null)
            {
                listBoxItem = FindVisualParent<ListBoxItem>(result.VisualHit);
            }
            else
            {
                Console.WriteLine("Clear Indi");
                viewModel.LastSelectModel?.ClearIndicator();
                viewModel.LastSelectModel = null;
                return;
            }

            if (listBoxItem == null)
            {
                Console.WriteLine("Clear Indi2");
                viewModel.LastSelectModel?.ClearIndicator();
                //viewModel.LastSelectModel = null;
                return;
            }

            TItem target = (TItem)listBoxItem.Content;
            DragableListModelBase source; //  = e.Data.GetData(typeof(TItem)) as TItem
            if (Type.GetType(e.Data.GetFormats()[0]).BaseType == typeof(DragableListModelBase))
            {
                source = e.Data.GetData(e.Data.GetFormats()[0]) as DragableListModelBase;
            }
            else
            {
                return;
            }

            if (target == source)
            {
                viewModel.LastSelectModel?.ClearIndicator();
                viewModel.LastSelectModel = null;
                if (source.GetType() == typeof(StrItemModel))
                {
                    if (tabListBoxViewModel.LastSelectModel != null)
                    {
                        tabListBoxViewModel.LastSelectModel.MoveInIndicator = false;
                    }
                }
                return;
            }
            else
            {
                if (source.GetType() == target.GetType())
                {
                    if (target != viewModel.LastSelectModel)
                    {
                        viewModel.LastSelectModel?.ClearIndicator();
                        if (source.GetType() == typeof(StrItemModel))
                        {
                            Console.WriteLine("trig");
                            TabItemModel lastModel = tabListBoxViewModel.LastSelectModel;
                            Console.WriteLine(lastModel);
                            if (lastModel != null)
                                lastModel.MoveInIndicator = false;
                        }
                    }
                    viewModel.LastSelectModel = target;

                    int targetIndex = viewModel.ModelCollection.IndexOf(target);
                    int sourceIndex = viewModel.ModelCollection.IndexOf((TItem)source);

                    if (targetIndex > sourceIndex)
                    {
                        target.DragIndicatorDown = true;
                    }
                    else
                    {
                        target.DragIndicatorUp = true;
                    }

                    //if (source.GetType() == typeof(StrItemModel))
                    //{
                    //    if (tabListBoxViewModel.LastSelectModel != null)
                    //    {
                    //        tabListBoxViewModel.LastSelectModel.MoveInIndicator = false;
                    //    }
                    //}

                }
                //command move to other tab
                else if (source.GetType() == typeof(StrItemModel) && target.GetType() == typeof(TabItemModel))
                {
                    
                    strListBoxViewModel.LastSelectModel?.ClearIndicator();
                    strListBoxViewModel.LastSelectModel = (StrItemModel)source;

                    if (tabListBoxViewModel.LastSelectModel != null)
                    {
                        tabListBoxViewModel.LastSelectModel.MoveInIndicator = false;
                    }

                    tabListBoxViewModel.LastSelectModel = (TabItemModel)(DragableListModelBase)target;
                    Console.WriteLine("Move to other tab"+ tabListBoxViewModel.LastSelectModel.Data.Name);
                    tabListBoxViewModel.LastSelectModel.MoveInIndicator = true;
                }
                //tab move to other string (x)
                else
                {
                    Console.WriteLine("X");
                    tabListBoxViewModel.LastSelectModel?.ClearIndicator();
                    tabListBoxViewModel.LastSelectModel = (TabItemModel)source;
                    e.Effects = DragDropEffects.None;

                }


            }
        }


        private void ListBox_Str_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemInfoUpdate(EditMode.EditString);
        }

        private void ListBox_Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemInfoUpdate(EditMode.EditTab);
            TabItemModel selectedItem = (TabItemModel)this.ListBox_Tab.SelectedItem;
            if (selectedItem != null)
            {
                TabStrRefresh(selectedItem.Data.StrList);
                //selectedItem.Checked = true;
            }


        }

        private void ItemInfoUpdate(EditMode mode)
        {

            bool addItem = false;
            bool isNull = false;
            switch (mode)
            {
                case EditMode.EditTab:
                    TabItemModel tab = (TabItemModel)this.ListBox_Tab.SelectedItem;
                    if (tab != null)
                    {
                        this.TextBox_Command.Text = null;
                        this.TextBox_Command.Visibility = Visibility.Hidden;
                        this.TextBox_Name.Text = tab.Data.Name;
                        this.TextBox_Description.Text = tab.Data.Description;
                    }
                    else
                        isNull = true;
                    break;
                case EditMode.EditString:
                    StrItemModel str = (StrItemModel)this.ListBox_Str.SelectedItem;
                    if (str != null)
                    {
                        this.TextBox_Command.Text = str.Data.Text;
                        this.TextBox_Command.Visibility = Visibility.Visible;
                        this.TextBox_Name.Text = str.Data.Name;
                        this.TextBox_Description.Text = str.Data.Description;
                    }
                    else
                        isNull = true;
                    break;
                case EditMode.AddTab:
                    addItem = true;

                    this.TextBox_Command.Text = "";
                    this.TextBox_Command.Visibility = Visibility.Hidden;
                    this.TextBox_Name.Text = "New Tab";
                    this.TextBox_Description.Text = "無";
                    break;
                case EditMode.AddString:
                    addItem = true;
                    this.TextBox_Command.Text = "";
                    this.TextBox_Command.Visibility = Visibility.Visible;
                    this.TextBox_Name.Text = "New String";
                    this.TextBox_Description.Text = "無";
                    break;
            }
            if (addItem)
            {
                TextBox_Name.Focus();
                EditApplyIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.AddThick;
                EditCancelIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseThick;
            }
            else
            {
                EditApplyIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckBold;
                EditCancelIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowRotateLeft;
            }
            if (!isNull)
                editMode = mode;

            InfoChangeCheck();
        }

        public void OnLoaded()
        {
            TabListRefresh();
        }

        public void TabListRefresh()
        {
           
            ObservableCollection<TabItemModel> TabModelCollection = tabListBoxViewModel.ModelCollection;
            TabItemModel selectionItem = ListBox_Tab.SelectedItem as TabItemModel;
            TabModelCollection.Clear();
            foreach (CustomStrTab tab in TabList)
            {
                TabModelCollection.Add(new TabItemModel(tab));
            }
            if (TabModelCollection.IndexOf(selectionItem) != -1)
                ListBox_Tab.SelectedItem = selectionItem;
            else
                ListBox_Tab.SelectedIndex = 0;
        }


        private void DataUpdate()
        {
            /*
            List<CustomStrData> strList = lastChooseTab.Tab.StrList;
            strList.Clear();
            foreach (CustomStrDataItem data in dataCollection)
            {
                strList.Add(data.Data);
            }
            */

        }


        public void TabStrRefresh(List<CustomStrData> strList)
        {
            ObservableCollection<StrItemModel> StrModelCollection = strListBoxViewModel.ModelCollection;
            StrModelCollection.Clear();
            foreach (CustomStrData str in strList)
            {
                StrModelCollection.Add(new StrItemModel(str));
            }
        }


        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {

                var pos = e.GetPosition((IInputElement)sender);
                var element = e.OriginalSource as FrameworkElement;
                if (element is CheckBox)
                    return;

                HitTestResult result = VisualTreeHelper.HitTest((ListBox)sender, pos);
                if (result == null)
                {
                    return;
                }

                var tabItem = VisualTreeHelper.GetParent(result.VisualHit);
                bool findit = false;
                while (!findit)
                {
                    tabItem = VisualTreeHelper.GetParent(tabItem);
                    if (tabItem == null)
                        return;
                    else if (tabItem is ListBoxItem)
                    {
                        findit = true;
                    }
                    else if (tabItem.GetType() == typeof(ItemsPresenter))
                        return;
                }
                //((ListBoxItem)tabItem).Content is TabItemModel
                if (true)
                {
                    DragDrop.DoDragDrop((DependencyObject)sender, new DataObject(((ListBoxItem)tabItem).Content), DragDropEffects.Move);
                }
            }
        }

        private void ListBox_Drop<TItem>(Object sender, DragEventArgs e) where TItem : DragableListModelBase
        {
            ListBox listbox = (ListBox)sender;

            Point pos = e.GetPosition(listbox);
            HitTestResult result = VisualTreeHelper.HitTest(listbox, pos);
            if (result == null)
            {
                return;
            }

            //查詢目標資料
            var listBoxItem = FindVisualParent<ListBoxItem>(result.VisualHit);
            if (listBoxItem == null)
            {
                return;
            }

            //查詢後設資料

            object target = listBoxItem.Content;

            DragableListModelBase source = null;
            if (Type.GetType(e.Data.GetFormats()[0]).BaseType == typeof(DragableListModelBase))
            {
                source = e.Data.GetData(e.Data.GetFormats()[0]) as DragableListModelBase;
            }
            else
                return;

            ListBoxViewModel<TItem> viewModel = (ListBoxViewModel<TItem>)listbox.DataContext;

            if (source.GetType() == target.GetType())
            {
                viewModel.LastSelectModel?.ClearIndicator();
                MoveItem<ObservableCollection<TItem>, TItem>(viewModel.ModelCollection, source, target);
            }
            //command move to other tab
            else if (source.GetType() == typeof(StrItemModel) && target.GetType() == typeof(TabItemModel))
            {
                if (tabListBoxViewModel.LastSelectModel != null)
                {
                    tabListBoxViewModel.LastSelectModel.MoveInIndicator = false;
                }
                List<StrItemModel> moveCommandsList = new List<StrItemModel>();
                moveCommandsList.Add((StrItemModel)ListBox_Str.SelectedItem);
                MoveItemToTab((TabItemModel)ListBox_Tab.SelectedItem, (TabItemModel)target, moveCommandsList);

                TabItemModel selectedItem = (TabItemModel)this.ListBox_Tab.SelectedItem;
                if (selectedItem != null)
                {
                    TabStrRefresh(selectedItem.Data.StrList);
                    //selectedItem.Checked = true;
                }
            }
            //tab move to other string (x)
            else
            {

            }
        }

        private void MoveItemToTab(TabItemModel sourceTab, TabItemModel targetTab, List<StrItemModel> strItems)
        {
            if (sourceTab is null || targetTab is null || strItems is null)
                return;
            foreach (StrItemModel item in strItems)
            {
                sourceTab.Data.StrList.Remove(item.Data);
                targetTab.Data.StrList.Add(item.Data);
            }
        }

        private static void MoveItem<T, I>(T collection, object _source, object _target) where T : Collection<I>
        {
            I source = (I)_source;
            I target = (I)_target;
            int sourceIndex = collection.IndexOf(source);
            int targetIndex = collection.IndexOf(target);
            if (targetIndex != -1 && sourceIndex != -1 && targetIndex != sourceIndex)
            {
                collection.Remove(source);
                int newTargetIndex = collection.IndexOf(target);
                if (targetIndex > sourceIndex)
                    collection.Insert(newTargetIndex + 1, source);
                else
                    collection.Insert(newTargetIndex, source);
            }
        }

        //根據子元素查詢父元素
        public static T FindVisualParent<T>(DependencyObject obj) where T : class
        {
            while (obj != null)
            {
                if (obj is T)
                    return obj as T;

                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }

        public void OnClose()
        {
            TabList.Clear();

            foreach (TabItemModel tab in tabListBoxViewModel.ModelCollection)
                TabList.Add(tab.Data);
        }

    }

    public class ListBoxViewModel<TItemModel> : ViewModelBase where TItemModel : class
    {
        public ListBoxViewModel()
        {
            ModelCollection = new ObservableCollection<TItemModel>();
        }

        public ObservableCollection<TItemModel> ModelCollection
        { get; }

        public TItemModel LastSelectModel { get; set; } = null;
    }
}
