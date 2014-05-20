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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

namespace Systembolaget
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1 : SurfaceWindow
    {

        Dictionary<Byte,Point> tagDict;
        Dictionary<String, object> compViz;
        Dictionary<Byte, object> singleViz;
        private String singleTable = "/Resources/singleTable.png";
        private String prodInfo = "/Resources/productInfo.png";

        
        // Limit for making comparison
        float distanceLimit = 50.0f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {
            InitializeComponent();
            tagDict = new Dictionary<byte,Point>();
            compViz = new Dictionary<string, object>();
            singleViz = new Dictionary<byte, object>();
            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            Image image = new Image();
            image.Width = 500;
            image.Height = 500;
            image.Source = createBitmap(singleTable);

            Canvas.SetLeft(image, 10);
            Canvas.SetTop(image, 10);

            MainCanvas.Children.Add(image);


        }

        private BitmapImage createBitmap(String path) {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            return bi;
        }

        private TransformedBitmap getTransformedBitmap(BitmapImage bi, int angle)
        {
            TransformedBitmap tb = new TransformedBitmap();
            tb.BeginInit();
            tb.Source = bi;
            tb.Transform = new RotateTransform(90);
            tb.EndInit();
            return tb;
        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        private void tagUpdate(object sender, TouchEventArgs args)
        {
            byte key = (byte)args.TouchDevice.GetTagData().Value;
            Point value = new Point();
            value = args.TouchDevice.GetPosition(this);
            if (this.tagDict.ContainsKey(key))
            {
                tagDict[key] = value;
            }
            else
            {
                tagDict.Add(key, value);
            }

            // Make new compounds if possible

            // Compound keys are of the form "3x4"

            // Remove single viz's if applicable

            // Make/move single viz otherwise

        }

        private void tagRemoved(object sender, TouchEventArgs args)
        {
            byte key = (byte)args.TouchDevice.GetTagData().Value;
            if (this.tagDict.ContainsKey(key))
            {
                tagDict.Remove(key);
            }

            // Remove compounds the key was used for

            // Remove single if applicable

            // Make new singles from tags without viz
        }

        private String[] inCompKeys(byte b)
        {
            int count = 0;
            foreach (String key in compViz.Keys)
            {
                if (key.Contains(b.ToString()))
                    count++;
            }
            if (count == 0)
                return null;

            String[] result = new String[count];
            int i = 0;
            foreach (String key in compViz.Keys)
            {
                if (key.Contains(b.ToString()))
                {
                    result[i++] = key;
                }
            }
            return result;
        }
    }
}