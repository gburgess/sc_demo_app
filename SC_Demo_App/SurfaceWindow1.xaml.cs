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

namespace SC_Demo_App
{


    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1 : SurfaceWindow
    {

        Dictionary<DispatcherTimer, ScatterViewItem> myDictionary = new Dictionary<DispatcherTimer, ScatterViewItem>();
        const int _countDown = 200;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {
            InitializeComponent();

            // Add handlers for Application activation events
            AddActivationHandlers();
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

        private void SurfaceButton_ContactDown(object sender, ContactEventArgs e)
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
            lb.Width = 800;
            lb.Height = 200;
            lb.Rows = 1;
            lb.FontSize = 12;

            //need a binging item to get the xml resource out of the resource dictionary
            Binding items = new Binding();
            items.Source = FindResource("Source"); //Source is the XML data in the resource dict
            //items.XPath = "Entry"; //can use Xpath here to get the xml needed
            lb.SetBinding(ItemsControl.ItemsSourceProperty, items);
            lb.ItemTemplate = (DataTemplate)this.FindResource("BarItemTemplate"); //set the item template from the resource dict



            //register the library bar so we can find it later
            //NameScope.GetNameScope(this).RegisterName("fm1_anotherLb" + stamp, lb); //must call Registername to be able to use FindName later

            //Must create a new scatterview item to be added to the named scatter view in the XAML.
            ScatterViewItem svi = new ScatterViewItem();
            svi.Style = (Style)this.FindResource("LibraryControlInScatterViewItemContentStyle"); //Get the style for the scatterview item when using a surface library control (provided in SDK eg)
            //svi.Orientation = o + 90;
           
            svi.Content = lb;

            //register the scatterview item so we can find it later
            NameScope.GetNameScope(this).RegisterName("fm1_anotherSV" + stamp, svi); //must call Registername to be able to use FindName later

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tag = _countDown; //store the count down in the tag of the timer so we can access it later
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            //add the new timer and listview to the dictionary so we can get at it later
            myDictionary.Add(timer, svi);

            //work out the orinetation the user clicked from and the library bar to the correct side of the surface at the correct orientation.
            if (o <= 180)
            {
                svi.Orientation = 180;
                svi.Center = new Point(550, 150);
                //svi.Margin = new Thickness(458,284,44,0);
                svi.ClipToBounds = true;
             
                //add the library bar to the screen
                this.libraryScatterViewTop.Items.Add(svi);
                
            }
            else
            {
                svi.Orientation = 0;
                svi.Center = new Point(550, 600);
                svi.ClipToBounds = true;
                //add the library bar to the screen
                this.libraryScatterViewTop.Items.Add(svi); //bot
            }

            
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
                    bool b2 = _svi.IsAnyContactCaptured;// this detects if the scatter item is being dragged

                    Debug.Print("IsAnyContactCaptured: " + _lb.IsAnyContactCaptured.ToString());
                    Debug.Print("IsAnyContactCapturedWithin: " + _lb.IsAnyContactCapturedWithin.ToString());
                    Debug.Print("IsAnyContactCapturedWithin: " + _lb.IsAnyContactCapturedWithin.ToString());
                    Debug.Print("IsAnyContactDirectlyOver: " + _lb.IsAnyContactDirectlyOver.ToString());
                    Debug.Print("IsAnyContactOver: " + _lb.IsAnyContactOver.ToString());
                    Debug.Print("IsMouseCaptured: " + _lb.IsMouseCaptured.ToString());
                    Debug.Print("IsFocused: " + _lb.IsFocused.ToString());
                    Debug.Print("_svi.IsAnyContactCaptured: " + _svi.IsAnyContactCaptured.ToString());
                    Debug.Print("_svi.IsAnyContactOver: " + _svi.IsAnyContactOver.ToString());
                    
                        
                    if (b ^ b2)
                        temp_c = _countDown;

                    temp_c--;
                    kvp.Key.Tag = temp_c;//update the tag in the dictionary.

                    if (temp_c == 0)
                    {
                        //work out which scatter view the library is in depending on the scatter view items orientation
                        if (_svi.Orientation == 0)
                        { //bot
                            this.libraryScatterViewTop.Items.Remove(_svi); //remove the listview
                        }
                        else
                        {
                            this.libraryScatterViewTop.Items.Remove(_svi); //remove the listview
                        }
                        
                        _timer.Stop();//stop the dispatcher
                    }
                    //_tb1.Text = temp_c.ToString();

                }
            }

        }


    }
}