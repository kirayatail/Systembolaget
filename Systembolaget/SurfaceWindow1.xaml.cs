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
        double distanceLimit = 50.0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {
            InitializeComponent();


            //Creates the image  for the visualization
            

            tagDict = new Dictionary<byte,Point>();
            compViz = new Dictionary<String, Image>();
            singleViz = new Dictionary<byte, Image>();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
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

            if(this.singleViz.ContainsKey(key))
            {
                removeVisualization(singleViz[key]);
                singleViz.Remove(key);
            }


            // Remove all compounds
            String[] compKeys = inCompKeys(key);
            if (compKeys != null)
            {
                for (int i = 0; i < compKeys.Length; i++)
                {
                    removeVisualization(this.compViz[compKeys[i]]);
                    this.compViz.Remove(compKeys[i]);
                }
            }

            // Make new compounds if possible
            foreach (byte other in tagDict.Keys)
            {
                if (key != other && (distance(key, other) <=this.distanceLimit))
                {
                    String compKey = makeCompKey(key, other);
                    Image img = createVisualization(compKey, midPoint(key, other));
                    this.compViz.Add(compKey, img);
                }
            }
            

            // Remove single viz's if applicable
            foreach (byte single in this.singleViz.Keys)
            {
                if (inCompKeys(single) != null)
                {
                    removeVisualization(this.singleViz[single]);
                    this.singleViz.Remove(single);
                }
            }

            // Make/move single viz otherwise
            foreach (byte tag in this.tagDict.Keys)
            {
                if (inCompKeys(tag) == null && !this.singleViz.ContainsKey(tag))
                {
                    Image img = createVisualization(tag, tagDict[tag]);
                    this.singleViz.Add(tag, img);
                }
            }

        }

        private String makeCompKey(byte tagA, byte tagB)
        {
            // Compound keys are of the form "3x4"
            // Numbers always in ascending order
            if (tagA > tagB)
            {
                return tagB + "x" + tagA;
            }
            else
            {
                return tagA + "x" + tagB;
            }
        }

        private Point midPoint(byte tagA, byte tagB)
        {
            Point result = new Point(0,0);
            result.X = (tagDict[tagA].X + tagDict[tagB].X) / 2;
            result.Y = (tagDict[tagA].Y + tagDict[tagB].Y) / 2;

            return result;
        }

        private double distance(byte tagA, byte tagB)
        {
            Vector diff = Point.Subtract(tagDict[tagA], tagDict[tagB]);

            return diff.Length;
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
    }
}