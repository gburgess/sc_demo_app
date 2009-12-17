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
using System.Xml;
using System.Diagnostics;

namespace SC_Demo_App
{
    /// <summary>
    /// Interaction logic for dragDrop.xaml
    /// </summary>
    public partial class dragDrop : SurfaceWindow
    {

        private const int DragThreshold = 15;

        // List to store the input devices those do not need do the dragging check.
        private List<InputDevice> ignoredDeviceList = new List<InputDevice>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public dragDrop()
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

        #region ShoppingList Drag Drop Code

        /// <summary>
        /// Handles the PreviewContactDown event for the ShoppingList.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShoppingListPreviewContactDown(object sender, ContactEventArgs e)
        {
            ignoredDeviceList.Remove(e.Device);
            InputDeviceHelper.ClearDeviceState(e.Device);

            InputDeviceHelper.InitializeDeviceState(e.Device);
        }

        /// <summary>
        /// Handles the PreviewContactChanged event for the ShoppingList.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShoppingListPreviewContactChanged(object sender, ContactEventArgs e)
        {
            // If this is a contact whose state has been initialized when its down event happens
            if (InputDeviceHelper.GetDragSource(e.Device) != null)
            {
                StartDragDrop(ShoppingList, e);
            }
        }

        /// <summary>
        /// Handles the PreviewContactUp event for the ShoppingList.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShoppingListPreviewContactUp(object sender, ContactEventArgs e)
        {
            ignoredDeviceList.Remove(e.Device);
            InputDeviceHelper.ClearDeviceState(e.Device);
        }

        /// <summary>
        /// Handles the PreviewMouseLeftButtonDown event for the ShoppingList.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShoppingListPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ignoredDeviceList.Remove(e.Device);
            InputDeviceHelper.ClearDeviceState(e.Device);

            InputDeviceHelper.InitializeDeviceState(e.Device);
        }

        /// <summary>
        /// Handles the PreviewMouseMove event for the ShoppingList.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShoppingListPreviewMouseMove(object sender, MouseEventArgs e)
        {
            // If this is a mouse whose state has been initialized when its down event happens
            if (InputDeviceHelper.GetDragSource(e.Device) != null)
            {
                StartDragDrop(ShoppingList, e);
            }
        }

        /// <summary>
        /// Handles the PreviewMouseLeftButtonUp event for the ShoppingList.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShoppingListPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ignoredDeviceList.Remove(e.MouseDevice);
            InputDeviceHelper.ClearDeviceState(e.Device);
        }

        private void OnShoppingListDragCompleted(object sender, SurfaceDragCompletedEventArgs e)
        {
            XmlElement draggingData = (XmlElement)e.Cursor.Data;

            SurfaceListBoxItem sourceListBoxItem = null;
            foreach (object item in ShoppingList.Items)
            {
                if (((XmlElement)item).OuterXml == draggingData.OuterXml)
                {
                    sourceListBoxItem = ShoppingList.ItemContainerGenerator.ContainerFromItem(item) as SurfaceListBoxItem;
                }
            }

            if (sourceListBoxItem != null)
            {
                sourceListBoxItem.Opacity = 1.0;
            }
        }

        /// <summary>
        /// Try to start Drag-and-drop for a listBox.
        /// </summary>
        /// <param name="sourceListBox"></param>
        /// <param name="e"></param>
        private void StartDragDrop(LibraryBar sourceListBox, InputEventArgs e)
        {
            // Check whether the input device is in the ignore list.
            if (ignoredDeviceList.Contains(e.Device))
            {
                return;
            }

            InputDeviceHelper.InitializeDeviceState(e.Device);

            Vector draggedDelta = InputDeviceHelper.DraggedDelta(e.Device, (UIElement)sourceListBox);

            // If this input device has moved more than Threshold pixels horizontally,
            // put it to the ignore list and never try to start drag-and-drop with it.
            if (Math.Abs(draggedDelta.X) > DragThreshold)
            {
                ignoredDeviceList.Add(e.Device);
                return;
            }

            // If this contact has moved less than Threshold pixels vertically 
            // then this is not a drag-and-drop yet.
            if (Math.Abs(draggedDelta.Y) < DragThreshold)
            {
                return;
            }

            ignoredDeviceList.Add(e.Device);

            // try to start drag-and-drop,
            // verify that the cursor the contact was placed at is a ListBoxItem
            DependencyObject downSource = InputDeviceHelper.GetDragSource(e.Device);
            Debug.Assert(downSource != null);

            //SurfaceListBoxItem draggedListBoxItem = GetVisualAncestor<SurfaceListBoxItem>(downSource);
            LibraryBarItem draggedListBoxItem = GetVisualAncestor<LibraryBarItem>(downSource);
            Debug.Assert(draggedListBoxItem != null);

            // Get Xml source.
            XmlElement data = draggedListBoxItem.Content as XmlElement;

            // Data should be copied, because the Stack rejects data of the same instance.
            data = data.Clone() as XmlElement;

            // Create a new ScatterViewItem as cursor visual.
            ScatterViewItem cursorVisual = new ScatterViewItem();
            cursorVisual.Style = (Style)FindResource("ScatterItemStyle");
            cursorVisual.Content = data;

            IEnumerable<InputDevice> devices = null;

            ContactEventArgs contactEventArgs = e as ContactEventArgs;
            if (contactEventArgs != null)
            {
                devices = MergeContacts(draggedListBoxItem.ContactsCapturedWithin, contactEventArgs.Contact);
            }
            else
            {
                devices = new List<InputDevice>(new InputDevice[] { e.Device });
            }

            if (!SurfaceDragDrop.BeginDragDrop(ShoppingList, draggedListBoxItem, cursorVisual, data, devices, DragDropEffects.Copy))
            {
                return;
            }

            // Reset the input device's state.
            InputDeviceHelper.ClearDeviceState(e.Device);
            ignoredDeviceList.Remove(e.Device);

            draggedListBoxItem.Opacity = 0.5;
            e.Handled = true;
        }

        /// <summary>
        /// Attempts to get an ancestor of the passed-in element with the given type.
        /// </summary>
        /// <typeparam name="T">Type of ancestor to search for.</typeparam>
        /// <param name="descendent">Element whose ancestor to find.</param>
        /// <param name="ancestor">Returned ancestor or null if none found.</param>
        /// <returns>True if found, false otherwise.</returns>
        private static T GetVisualAncestor<T>(DependencyObject descendent) where T : class
        {
            T ancestor = null;
            DependencyObject scan = descendent;
            ancestor = null;

            while (scan != null && ((ancestor = scan as T) == null))
            {
                scan = VisualTreeHelper.GetParent(scan);
            }

            return ancestor;
        }

        /// <summary>
        /// Merges the remaining contacts on the drag source besides the contact that is down.
        /// </summary>
        /// <param name="existContacts"></param>
        /// <param name="extraContact"></param>
        /// <returns></returns>
        private static IEnumerable<InputDevice> MergeContacts(IEnumerable<Contact> existContacts, Contact extraContact)
        {
            var result = new List<InputDevice> { extraContact };

            foreach (Contact contact in existContacts)
            {
                if (contact != extraContact)
                {
                    result.Add(contact);
                }
            }

            return result;
        }

        #endregion
    }

    /// <summary>
    /// Template selector class for the items in the ShoppingList.
    /// </summary>
    public class ShoppingListTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Template for the item selected as the starting item.
        /// </summary>
        public DataTemplate StartingItemTemplate { get; set; }

        /// <summary>
        /// Template for items that are not the starting item.
        /// </summary>
        public DataTemplate NormalItemTemplate { get; set; }

        /// <summary>
        /// Selects data templates for items in the ShoppingList.
        /// If an item has the content of "Age of Empires 3", which is the first item in the item list, the StartingItemTemplate will be used.
        /// Otherwise, the NormalItemTemplate will be used.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            XmlElement data = item as XmlElement;
            return (data != null && data.GetAttribute("Name") == "Age Of Empires 3") ? StartingItemTemplate : NormalItemTemplate;
        }
    }
}