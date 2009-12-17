using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using System.Diagnostics;
using SC_Demo_App;
using System.Xml;

namespace SC_Demo_App
{
    /// <summary>
    /// Interaction logic for MultipleLibraryBarsFromCode.xaml
    /// </summary>
    public partial class MultipleLibraryBarsFromCode : SurfaceWindow
    {

        private const int DragThreshold = 15;

        // List to store the input devices those do not need do the dragging check.
        private List<InputDevice> ignoredDeviceList = new List<InputDevice>();


        Dictionary<DispatcherTimer, ScatterViewItem> myDictionary = new Dictionary<DispatcherTimer, ScatterViewItem>();
        const int _countDown = 10;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MultipleLibraryBarsFromCode()
        {
            InitializeComponent();

            // Add handlers for Application activation events
            AddActivationHandlers();





            //SurfaceButton sb = new SurfaceButton();
            //sb.Name = "cButton";
            //sb.Content = "CButton";
            //this.myStackPanel.Children.Add(sb);



        }


        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for Application activation events
            RemoveActivationHandlers();
        }

        /// <summary>
        /// Adds handlers for Application activation events.
        /// </summary>
        private void AddActivationHandlers()
        {
            // Subscribe to surface application activation events
            ApplicationLauncher.ApplicationActivated += OnApplicationActivated;
            ApplicationLauncher.ApplicationPreviewed += OnApplicationPreviewed;
            ApplicationLauncher.ApplicationDeactivated += OnApplicationDeactivated;
        }

        /// <summary>
        /// Removes handlers for Application activation events.
        /// </summary>
        private void RemoveActivationHandlers()
        {
            // Unsubscribe from surface application activation events
            ApplicationLauncher.ApplicationActivated -= OnApplicationActivated;
            ApplicationLauncher.ApplicationPreviewed -= OnApplicationPreviewed;
            ApplicationLauncher.ApplicationDeactivated -= OnApplicationDeactivated;
        }

        /// <summary>
        /// This is called when application has been activated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationActivated(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when application is in preview mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationPreviewed(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        ///  This is called when application has been deactivated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationDeactivated(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        private void addLibraryBar_ContactDown(object sender, ContactEventArgs e)
        {
            //SurfaceButton _bt = (SurfaceButton)sender;
            //_bt.po
            string stamp = Guid.NewGuid().ToString(); //guid to stamp the control name with
            stamp = stamp.Replace("-", ""); //remove the - from the guid


            //get the sender contact so we can get the orientation
            double o = e.Contact.GetOrientation(this);
            //Point p = e.Contact.GetPosition();

            //create the LibraryBar
            LibraryBar lb = new LibraryBar();
            lb.Width = 860;
            lb.Height = 250;
            lb.Rows = 1;

            //need a binging item to get the xml resource out of the resource dictionary
            Binding items = new Binding();
            items.Source = FindResource("Source"); //Source is the XML data in the resource dict
            //items.XPath = "Entry"; //can use Xpath here to get the xml needed
            lb.SetBinding(ItemsControl.ItemsSourceProperty, items);
            lb.ItemTemplate = (DataTemplate)this.FindResource("BarItemTemplate"); //set the item template from the resource dict


            //lb.AddHandler(SurfaceDragDrop.DragCompletedEvent,(OnShoppingListDragCompleted));
           
            //lb.PreviewContactDown  += OnShoppingListPreviewContactDown;
            //lb.PreviewContactChanged += OnShoppingListPreviewContactChanged;
            //lb.PreviewContactUp += OnShoppingListPreviewContactUp;
            //lb.PreviewMouseLeftButtonDown += OnShoppingListPreviewMouseLeftButtonDown;
            //lb.PreviewMouseMove += OnShoppingListPreviewMouseMove;
            //lb.PreviewMouseLeftButtonUp += OnShoppingListPreviewMouseLeftButtonUp;

           

            //register the library bar so we can find it later
            //NameScope.GetNameScope(this).RegisterName("fm1_anotherLb" + stamp, lb); //must call Registername to be able to use FindName later

            //Must create a new scatterview item to be added to the named scatter view in the XAML.
            ScatterViewItem svi = new ScatterViewItem();
            svi.Style = (Style)this.FindResource("LibraryControlInScatterViewItemContentStyle"); //Get the style for the scatterview item when using a surface library control (provided in SDK eg)
            svi.Orientation = o + 90;
            svi.Center = new Point(400, 400);
            svi.Content = lb;

            //register the scatterview item so we can find it later
            //NameScope.GetNameScope(this).RegisterName("fm1_anotherSV" + stamp, svi); //must call Registername to be able to use FindName later
            NameScope.GetNameScope(this).RegisterName("fm1_anotherSV", svi); //temp perm name

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tag = _countDown; //store the count down in the tag of the timer so we can access it later
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            //add the new timer and listview to the dictionary so we can get at it later
            myDictionary.Add(timer, svi);
            //add the library bar to the screen
            this.myScatterView.Items.Add(svi);
        }



        void timer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer _timer = (DispatcherTimer)sender;

            foreach (KeyValuePair<DispatcherTimer, ScatterViewItem> kvp in myDictionary)
            {
                if (_timer == kvp.Key)
                {
                    int temp_c = Convert.ToInt16(_timer.Tag);

                    ScatterViewItem _svi = kvp.Value;

                    LibraryBar _lb = (LibraryBar)_svi.Content;
                    
                    //TextBox _tb1 = (TextBox)_lv.Items[0]; //find the associated text box.

                    bool b = _lb.IsAnyContactOver;// this is the event to check to make sure no action needs to reset the countdown timer
                    Debug.Print("IsAnyContactCaptured: " + _lb.IsAnyContactCaptured.ToString());
                    Debug.Print("IsAnyContactCapturedWithin: " + _lb.IsAnyContactCapturedWithin.ToString());
                    Debug.Print("IsAnyContactCapturedWithin: " + _lb.IsAnyContactCapturedWithin.ToString());
                    Debug.Print("IsAnyContactDirectlyOver: " + _lb.IsAnyContactDirectlyOver.ToString());
                    Debug.Print("IsAnyContactOver: " + _lb.IsAnyContactOver.ToString());
                    Debug.Print("IsMouseCaptured: " + _lb.IsMouseCaptured.ToString());
                    if (b)
                        temp_c = _countDown;

                    temp_c--;
                    kvp.Key.Tag = temp_c;//update the tag in the dictionary.

                    if (temp_c == 0)
                    {
                        this.myScatterView.Items.Remove(_svi); //remove the listview
                        _timer.Stop();//stop the dispatcher
                    }
                    //_tb1.Text = temp_c.ToString();

                }
            }

        }

        //#region ShoppingList Drag Drop Code

        ///// <summary>
        ///// Handles the PreviewContactDown event for the ShoppingList.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void OnShoppingListPreviewContactDown(object sender, ContactEventArgs e)
        //{
        //    ignoredDeviceList.Remove(e.Device);
        //    InputDeviceHelper.ClearDeviceState(e.Device);

        //    InputDeviceHelper.InitializeDeviceState(e.Device);
        //}

        ///// <summary>
        ///// Handles the PreviewContactChanged event for the ShoppingList.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void OnShoppingListPreviewContactChanged(object sender, ContactEventArgs e)
        //{
        //    // If this is a contact whose state has been initialized when its down event happens
        //    if (InputDeviceHelper.GetDragSource(e.Device) != null)
        //    {
        //        StartDragDrop(fm1_anotherSV, e);
        //    }
        //}

        ///// <summary>
        ///// Handles the PreviewContactUp event for the ShoppingList.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void OnShoppingListPreviewContactUp(object sender, ContactEventArgs e)
        //{
        //    ignoredDeviceList.Remove(e.Device);
        //    InputDeviceHelper.ClearDeviceState(e.Device);
        //}

        ///// <summary>
        ///// Handles the PreviewMouseLeftButtonDown event for the ShoppingList.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void OnShoppingListPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    ignoredDeviceList.Remove(e.Device);
        //    InputDeviceHelper.ClearDeviceState(e.Device);

        //    InputDeviceHelper.InitializeDeviceState(e.Device);
        //}

        ///// <summary>
        ///// Handles the PreviewMouseMove event for the ShoppingList.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void OnShoppingListPreviewMouseMove(object sender, MouseEventArgs e)
        //{
        //    // If this is a mouse whose state has been initialized when its down event happens
        //    if (InputDeviceHelper.GetDragSource(e.Device) != null)
        //    {
        //        StartDragDrop(fm1_anotherSV, e);
        //    }
        //}

        ///// <summary>
        ///// Handles the PreviewMouseLeftButtonUp event for the ShoppingList.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void OnShoppingListPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    ignoredDeviceList.Remove(e.MouseDevice);
        //    InputDeviceHelper.ClearDeviceState(e.Device);
        //}

        //private void OnShoppingListDragCompleted(object sender, SurfaceDragCompletedEventArgs e)
        //{
        //    XmlElement draggingData = (XmlElement)e.Cursor.Data;

        //    SurfaceListBoxItem sourceListBoxItem = null;
        //    foreach (object item in fm1_anotherSV.Items)
        //    {
        //        if (((XmlElement)item).OuterXml == draggingData.OuterXml)
        //        {
        //            sourceListBoxItem = fm1_anotherSV.ItemContainerGenerator.ContainerFromItem(item) as SurfaceListBoxItem;
        //        }
        //    }

        //    if (sourceListBoxItem != null)
        //    {
        //        sourceListBoxItem.Opacity = 1.0;
        //    }
        //}

        ///// <summary>
        ///// Try to start Drag-and-drop for a listBox.
        ///// </summary>
        ///// <param name="sourceListBox"></param>
        ///// <param name="e"></param>
        //private void StartDragDrop(ListBox sourceListBox, InputEventArgs e)
        //{
        //    // Check whether the input device is in the ignore list.
        //    if (ignoredDeviceList.Contains(e.Device))
        //    {
        //        return;
        //    }

        //    InputDeviceHelper.InitializeDeviceState(e.Device);

        //    Vector draggedDelta = InputDeviceHelper.DraggedDelta(e.Device, (UIElement)sourceListBox);

        //    // If this input device has moved more than Threshold pixels horizontally,
        //    // put it to the ignore list and never try to start drag-and-drop with it.
        //    if (Math.Abs(draggedDelta.X) > DragThreshold)
        //    {
        //        ignoredDeviceList.Add(e.Device);
        //        return;
        //    }

        //    // If this contact has moved less than Threshold pixels vertically 
        //    // then this is not a drag-and-drop yet.
        //    if (Math.Abs(draggedDelta.Y) < DragThreshold)
        //    {
        //        return;
        //    }

        //    ignoredDeviceList.Add(e.Device);

        //    // try to start drag-and-drop,
        //    // verify that the cursor the contact was placed at is a ListBoxItem
        //    DependencyObject downSource = InputDeviceHelper.GetDragSource(e.Device);
        //    Debug.Assert(downSource != null);

        //    SurfaceListBoxItem draggedListBoxItem = GetVisualAncestor<SurfaceListBoxItem>(downSource);
        //    Debug.Assert(draggedListBoxItem != null);

        //    // Get Xml source.
        //    XmlElement data = draggedListBoxItem.Content as XmlElement;

        //    // Data should be copied, because the Stack rejects data of the same instance.
        //    data = data.Clone() as XmlElement;

        //    // Create a new ScatterViewItem as cursor visual.
        //    ScatterViewItem cursorVisual = new ScatterViewItem();
        //    cursorVisual.Style = (Style)FindResource("ScatterItemStyle");
        //    cursorVisual.Content = data;

        //    IEnumerable<InputDevice> devices = null;

        //    ContactEventArgs contactEventArgs = e as ContactEventArgs;
        //    if (contactEventArgs != null)
        //    {
        //        devices = MergeContacts(draggedListBoxItem.ContactsCapturedWithin, contactEventArgs.Contact);
        //    }
        //    else
        //    {
        //        devices = new List<InputDevice>(new InputDevice[] { e.Device });
        //    }

        //    if (!SurfaceDragDrop.BeginDragDrop(fm1_anotherSV, draggedListBoxItem, cursorVisual, data, devices, DragDropEffects.Copy))
        //    {
        //        return;
        //    }

        //    // Reset the input device's state.
        //    InputDeviceHelper.ClearDeviceState(e.Device);
        //    ignoredDeviceList.Remove(e.Device);

        //    draggedListBoxItem.Opacity = 0.5;
        //    e.Handled = true;
        //}

        ///// <summary>
        ///// Attempts to get an ancestor of the passed-in element with the given type.
        ///// </summary>
        ///// <typeparam name="T">Type of ancestor to search for.</typeparam>
        ///// <param name="descendent">Element whose ancestor to find.</param>
        ///// <param name="ancestor">Returned ancestor or null if none found.</param>
        ///// <returns>True if found, false otherwise.</returns>
        //private static T GetVisualAncestor<T>(DependencyObject descendent) where T : class
        //{
        //    T ancestor = null;
        //    DependencyObject scan = descendent;
        //    ancestor = null;

        //    while (scan != null && ((ancestor = scan as T) == null))
        //    {
        //        scan = VisualTreeHelper.GetParent(scan);
        //    }

        //    return ancestor;
        //}

        ///// <summary>
        ///// Merges the remaining contacts on the drag source besides the contact that is down.
        ///// </summary>
        ///// <param name="existContacts"></param>
        ///// <param name="extraContact"></param>
        ///// <returns></returns>
        //private static IEnumerable<InputDevice> MergeContacts(IEnumerable<Contact> existContacts, Contact extraContact)
        //{
        //    var result = new List<InputDevice> { extraContact };

        //    foreach (Contact contact in existContacts)
        //    {
        //        if (contact != extraContact)
        //        {
        //            result.Add(contact);
        //        }
        //    }

        //    return result;
        //}

        //#endregion
    }
}