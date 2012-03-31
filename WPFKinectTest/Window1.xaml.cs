///----------------------------------------------------------
///Kinect Depth Example
///Date: February 12th, 2012
///--------------------
///Authors:
/// - Jesus Dominguez:
/// 
/// - Angel Hernandez:
///     @mifulapirus
///     www.tupperbot.com
///-----------------------
///All code is based on the Kinect Explorer Example. 
///-----------------------------------------------------------------------------------------------
///Summary:
///This is just a simplification of the Explorer Example code that comes
///with the final version of the Kinect SDK released by Microsoft in February.
///This example shows the minimum code required to get the depth image drawn in a WPF program.
///It is for sure not a perfect and super safe code, but it is just intended to show how to do 
///this in a simple way.
///-----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace WPFKinectTest
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        FloorDetection floorDetection;
        FloorServer floorServer;

        //Declare some global variables
        private short[] pixelData;
        private byte[] depthFrame32;

        //The bitmap that will contain the actual converted depth into an image
        private WriteableBitmap outputBitmap;
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        
        //Format of the last Depth Image. 
        //This changes when you first run the program or whenever you minimize the window 
        private DepthImageFormat lastImageFormat;

        //XBox Kinects (default) are limited between 800mm and 4096mm.
        //int MinimumDistance = 800;
        //int MaximumDistance = 4096;

        //Declare our Kinect Sensor!
        KinectSensor kinectSensor;
        
        public Window1()
        {
            InitializeComponent();
            floorDetection = new FloorDetection();
            
            //Select the first kinect found
            kinectSensor = KinectSensor.KinectSensors[0];

            //Set up the depth stream to be the largest possible
            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);  
            //Initialize the Kinect Sensor
            kinectSensor.Start();
            kinectSensor.ElevationAngle = -20;
            
            //Subscribe to an event that will be triggered every time a new frame is ready
            kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);

            //Add key down handler so that user can check when it is flat
            this.KeyDown += new KeyEventHandler(grid1_KeyDown);
            this.Closing += new System.ComponentModel.CancelEventHandler(Window_Closing);

            floorServer = new FloorServer(floorDetection);
        }

 
        /// <summary>
        /// DepthImageReady:
        /// This function will be called every time a new depth frame is ready
        /// </summary>
        private void DepthImageReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame imageFrame = e.OpenDepthImageFrame())
            {
            	//We expect this to be always true since we are coming from a triggered event
                if (imageFrame != null)
                {
                    //Check if the format of the image has changed.
                    //This always happens when you run the program for the first time and every time you minimize the window
                    bool NewFormat = this.lastImageFormat != imageFrame.Format;

                    if (NewFormat)
                    {
                        //Update the image to the new format
                        this.pixelData = new short[imageFrame.PixelDataLength];
                        this.depthFrame32 = new byte[imageFrame.Width * imageFrame.Height * Bgr32BytesPerPixel];
                        
                        
                        //Create the new Bitmap
                        this.outputBitmap = new WriteableBitmap(
                           imageFrame.Width,
                           imageFrame.Height,
                           96,  // DpiX
                           96,  // DpiY
                           PixelFormats.Bgr32,
                           null);

                        this.kinectDepthImage.Source = this.outputBitmap;
                    }

                    //Copy the stream to its short version
                    imageFrame.CopyPixelDataTo(this.pixelData);


                    //Convert the pixel data into its RGB Version.
                    //Here is where the magic happens
                    byte[] convertedDepthBits = this.ConvertDepthFrame(this.pixelData, ((KinectSensor)sender).DepthStream);



                    //Copy the RGB matrix to the bitmap to make it visible
					this.outputBitmap.WritePixels(
                        new Int32Rect(0, 0, imageFrame.Width, imageFrame.Height), 
                        convertedDepthBits,
                        imageFrame.Width * Bgr32BytesPerPixel,
                        0);

                    //Update the Format
                    this.lastImageFormat = imageFrame.Format;


                }

                //Since we are coming from a triggered event, we are not expecting anything here, at least for this short tutorial.
                else { }
            }
        }


        /// <summary>
        /// ConvertDepthFrame:
        /// Converts the depth frame into its RGB version taking out all the player information and leaving only the depth.
        /// BASED ON CODE FROM: http://www.tupperbot.com/?p=133
        /// </summary>
        private byte[] ConvertDepthFrame(short[] depthFrame, DepthImageStream depthStream)
        {
            int[] realDepthArray = new int[depthFrame.Length];

            //Run through the depth frame making the correlation between the two arrays
            for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < this.depthFrame32.Length; i16++, i32 += 4)
            {
                //We don't care about player's information here, so we are just going to rule it out by shifting the value.
                int realDepth = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                realDepthArray[i16] = realDepth;

            }

            byte[] ourDepthFrame = floorDetection.execute(realDepthArray);

            return ourDepthFrame;
        }

        private void grid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                if (floorDetection.flatSurfaceDown)
                    Console.Write("FLAT!");
                else
                    Console.Write("Not flat!");
            }
            else if (e.Key == Key.C)
            {
                Console.WriteLine("Saving Calibration Image");
                floorDetection.savedFlag = false;
                
            } 
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Environment.Exit(1);
        }

   }
}