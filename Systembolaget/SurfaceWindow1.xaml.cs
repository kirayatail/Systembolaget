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
        Dictionary<Byte, Double> oriDict;
        Dictionary<String, Image> compViz;
        Dictionary<Byte, Image> singleViz;
        
        // Limit for making comparison
        double distanceLimit = 400.0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {
            InitializeComponent();

            tagDict = new Dictionary<byte,Point>();
            oriDict = new Dictionary<byte, double>();
            compViz = new Dictionary<String, Image>();
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

        private TransformedBitmap getTransformedBitmap(BitmapImage bi, double angle)
        {
            TransformedBitmap tb = new TransformedBitmap();
            tb.BeginInit();
            tb.Source = bi;
            tb.Transform = new RotateTransform(angle);
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
            // discarding 0 as it's probably invalid.
            if (key == 0)
                return;


            Point value = new Point();
            double orientation = 0;
            value = args.TouchDevice.GetPosition(this);
            orientation = args.TouchDevice.GetOrientation(this);
            Console.WriteLine("Updating tag with value " + key + " and position X:" + value.X + " Y:" + value.Y);
            if (this.tagDict.ContainsKey(key))
            {
                tagDict[key] = value;
                oriDict[key] = orientation;
            }
            else
            {
                tagDict.Add(key, value);
                oriDict.Add(key, orientation);
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
                    Console.WriteLine("Making a new compound with tag " + compKey);
                    Image img = createVisualization(compKey, midPoint(key, other), twoPointOrientation(key,other));
                    this.compViz.Add(compKey, img);
                }
            }
            

            // Remove single viz's if applicable
            byte[] singleKeys = new byte[this.singleViz.Keys.Count];
            this.singleViz.Keys.CopyTo(singleKeys,0);
            foreach (byte single in singleKeys)
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
                    Image img = createVisualization(tag, tagDict[tag], oriDict[tag]);
                    this.singleViz.Add(tag, img);
                }
            }

        }

        private double twoPointOrientation(byte key, byte other)
        {
            byte a; byte b;

            if (key > other)
            {
                a = key;
                b = other;
            }
            else
            {
                a = other;
                b = key;
            }
            Vector reference = new Vector(1,0);
            Vector result = Point.Subtract(tagDict[a], tagDict[b]);
            return Vector.AngleBetween(reference, result);
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

            // Disregard 0
            if (key == 0)
                return;

            
            if (this.tagDict.ContainsKey(key))
            {
                tagDict.Remove(key);
                oriDict.Remove(key);
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
                    img = createVisualization(tag, tagDict[tag], oriDict[tag]);
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

        private Image createVisualization(String tagValue, Point pos, double orientation)
        {
            String path = "";

            if (tagValue.Equals("20"))
            {
                path = "Resources/productInfo.png";
            }
            else if (tagValue.Equals("48"))
            {
                path = "Resources/productInfo.png";
            }
            else if (tagValue.Equals("20x48"))
            {
                path = "Resources/combineInfo.png";
            }
            else
                return null;

            Console.WriteLine("Creating viz with path: " + path);
            Image img = new Image();

            // double angle = 90 * (Math.Floor(orientation / 90));

            img.Source = createBitmap(path);
            RotateTransform rt = new RotateTransform(orientation);
            img.RenderTransform = rt;
            img.Height = 300;
            img.Width = 400;
            img.Tag = tagValue;

            Point rotationVec = new Point(-200, -350);
            Matrix m = Matrix.Identity;
            m.Rotate(orientation);
            rotationVec = m.Transform(rotationVec);

            Canvas.SetLeft(img, pos.X+rotationVec.X);
            Canvas.SetTop(img, pos.Y+rotationVec.Y);

            can.Children.Add(img);

            return img;
        }
        private Image createVisualization(byte tagValue, Point pos, double orientation)
        {
            return createVisualization(tagValue.ToString(), pos, orientation);
        }

        private void removeVisualization(Image img)
        {
            if(img != null)
            this.can.Children.Remove(img);
        }
    }
}