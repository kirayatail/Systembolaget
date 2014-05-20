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
        Dictionary<String, Image> compViz;
        Dictionary<Byte, Image> singleViz;
        
        // Limit for making comparison
        float distanceLimit = 50.0f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {
            InitializeComponent();

            tagDict = new Dictionary<byte,Point>();
            compViz = new Dictionary<string, Image>();
            singleViz = new Dictionary<byte, Image>();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
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
            String[] compKeys = inCompKeys(key);
            Image img = null;
            if (compKeys != null)
            {
                
                for (int i = 0; i < compKeys.Length; i++)
                {
                    img = this.compViz[compKeys[i]];
                    removeVisualization(img);
                    this.compViz.Remove(compKeys[i]);
                }
            }

            // Remove single if applicable
            if (this.singleViz.ContainsKey(key))
            {
                img = this.singleViz[key];
                removeVisualization(img);
                this.singleViz.Remove(key);
            }

            // Make new singles from tags without viz
            foreach (byte tag in this.tagDict.Keys)
            {
                if (inCompKeys(tag) == null && !this.singleViz.ContainsKey(tag))
                {
                    img = createVisualization(tag, tagDict[tag]);
                    this.singleViz.Add(tag, img);
                }
            }
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

        private Image createVisualization(String tagValue, Point pos)
        {
            Image img = new Image();
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri("/Resources/combineInfo.png", UriKind.Relative);
            img.Source = b;
            img.Height = 500;
            img.Width = 600;
            img.Tag = tagValue;


            Canvas.SetLeft(img, pos.X);
            Canvas.SetTop(img, pos.Y);

            can.Children.Add(img);
            b.EndInit();

            return img;
        }
        private Image createVisualization(byte tagValue, Point pos)
        {
            return createVisualization(tagValue.ToString(), pos);
        }

        private void removeVisualization(Image img)
        {
            this.can.Children.Remove(img);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Image img = createVisualization(1, new Point(200, 300));
            this.singleViz.Add(1, img);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // Destroyer
            Image img = (Image)this.singleViz[1];
            this.singleViz.Remove(1);
            removeVisualization(img);
        }
    }
}