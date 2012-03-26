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
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.IO;

namespace WPFKinectTest
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        //Declare some global variables
        private short[] pixelData;
        private byte[] depthFrame32;

        //Server handling client requests asking for flat surface
        System.Net.Sockets.TcpListener Server;

        //Keep a variable of whether or not the pathway is flat
        Boolean flatSurface = true;

        //Guard client connected
        Boolean clientConnected = false;

        //Maximum Error allowed between depth array and calibration image
        int MaximumError = 100;

        //The bitmap that will contain the actual converted depth into an image
        private WriteableBitmap outputBitmap;
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        
        //Format of the last Depth Image. 
        //This changes when you first run the program or whenever you minimize the window 
        private DepthImageFormat lastImageFormat;
        
        //Identify each color layer on the R G B
        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        //Best Angle of kinect camera to calculate distance from point to camera lense
        //We initialize it to be -361 because we don't know it yet
        int bestAngle = -361;

        //Depth map loaded by test file
        int[,] providedDepthMap;

        //XBox Kinects (default) are limited between 800mm and 4096mm.
        int MinimumDistance = 800;
        int MaximumDistance = 4096;

        Boolean savedFlag = true;
        Boolean depthMapLoaded = false;
        //height from camera lense to floor
        double heightOfCamera = 660; //in mm 

        //Marc Andre's static depth map
        int[,] marcSimulatedDepthArray = new int[640, 480];

        //Declare our Kinect Sensor!
        KinectSensor kinectSensor;
        
        public Window1()
        {
            InitializeComponent();
            
            //Select the first kinect found
            kinectSensor = KinectSensor.KinectSensors[0];

            //Set up the depth stream to be the largest possible
            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);  
            //Initialize the Kinect Sensor
            kinectSensor.Start();
            kinectSensor.ElevationAngle = -20;
            
            //Subscribe to an event that will be triggered every time a new frame is ready
            kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);
            //Read the elevation value of the Kinect and assign it to the slider so it doesn't look weird when the program starts 
            slider1.Value = kinectSensor.ElevationAngle;
            

            //Build Marc Andre's static depth map
            buildMarcSimulatedDepthMap();
           // checkIfFlatFloorTest(); //For testing purposes

            //Add key down handler so that user can check when it is flat
            this.KeyDown += new KeyEventHandler(grid1_KeyDown);

            Thread thread = new Thread(new ThreadStart(AcceptClients));
            thread.Start();
        }


        private void AcceptClients()
        {
            Server = new System.Net.Sockets.TcpListener(IPAddress.Any, 1234);
            Console.WriteLine("\nWaiting for Clients");
            Server.Start();
            System.Net.Sockets.TcpClient chatConnection = Server.AcceptTcpClient();
      //      Thread thread = new Thread(new ThreadStart(communicateWithClient(chatConnection));
      //      thread.Start();
            Console.WriteLine("Someone connected!");
            StreamReader inputStream = new System.IO.StreamReader(chatConnection.GetStream());
            StreamWriter outputStream = new System.IO.StreamWriter(chatConnection.GetStream());
            Console.WriteLine("Reader stream");
            while (true)
            {
                String input = inputStream.ReadLine();
                if (input != null)
                {
                    if (input.Contains("a"))
                    {
                        Console.WriteLine("Received request: " + flatSurface);
                        outputStream.WriteLine("" + flatSurface);
                        outputStream.Flush();
                    }
                }
                else
                {
                    Console.Write(".");
                }

            }

        }

        private void communicateWithClient(System.Net.Sockets.TcpClient chatConnection)
        {
           
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
 
            byte[] ourDepthFrame = checkIfFlatFloor(realDepthArray);

            return ourDepthFrame;
        }

    

        public byte[] convertDiffToColor(int[,] diffDepthArray)
        {
          
            byte[] colorDepthArray = new byte[4 * 640 * 480];

            for (int x = 0; x < 640; x++)
            {
                for (int y = 0; y < 480; y++)
                {
                    int diff = diffDepthArray[x, y];
                    if (marcSimulatedDepthArray[x, y] == -2)
                    {
                        colorDepthArray[4 * (x + y * 640) + RedIndex] = 0;
                        colorDepthArray[4 * (x + y * 640) + GreenIndex] = 255;
                        colorDepthArray[4 * (x + y * 640) + BlueIndex] = 0;
                    }
                    else if (Math.Abs(diff) < MaximumError)
                    {
                        colorDepthArray[4 * (x + y * 640) + RedIndex] = 255;
                        colorDepthArray[4 * (x + y * 640) + GreenIndex] = 255;
                        colorDepthArray[4 * (x + y * 640) + BlueIndex] = 255;
                    }
                    else if (diff < -MaximumError)
                    {
                        colorDepthArray[4 * (x + y * 640) + RedIndex] = 255;
                        colorDepthArray[4 * (x + y * 640) + GreenIndex] = 0;
                        colorDepthArray[4 * (x + y * 640) + BlueIndex] = 0;
                    }
                    else if (diff > MaximumError)
                    {
                        colorDepthArray[4 * (x + y * 640) + RedIndex] = 0;
                        colorDepthArray[4 * (x + y * 640) + GreenIndex] = 0;
                        colorDepthArray[4 * (x + y * 640) + BlueIndex] = 255;
                    }
                }
            }

            return colorDepthArray;
        }


        //If you move the wheel of your mouse after the slider got the focus, you will move the motor of the kinect.
        //We have to be very careful doing this since the kinect might get unresponsive if we send this command too fast.
        private void slider1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //Calculate the new value based on the wheel movement
            if (e.Delta > 0) { slider1.Value = slider1.Value + 5; }
            else { slider1.Value = slider1.Value - 5; }
            //Send the new elevation value to our Kinect
            kinectSensor.ElevationAngle = (int)slider1.Value;
        }

        //Here we save an image from the kinect camera so that we may run
        //automated tests later
        private void saveDepthMapToFile(int[] depthArray)
        {
            //Here we write the depth map to a file for testing purposes
            TextWriter tw = new StreamWriter("TESTINGFILE.txt");
            
            //Write the height, then the best angle
            tw.WriteLine(heightOfCamera);
            tw.WriteLine(bestAngle);

            for (int i = 0; i < 640; i++)
                for (int j = 0; j < 480; j++)
                    tw.WriteLine(depthArray[i+j*640]);

            // close the stream
            tw.Close();
            savedFlag = true;
            providedDepthMap = provideTestingDepthMapFromFile();
        }


             private void buildMarcSimulatedDepthMap()
        {
            int Width = 640;
            int Height = 480;

            int heightKinect = 660;
            double angleKinect = 71;

            double verticalWide = 42;
            double horizontalWide = 57;

            int MaxDistance = 4096;
            int radius = 400;

            int[,] depthArray = new int[640, 480];

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    double currentVertAngle = angleKinect - (verticalWide / 2.0) * ((j - 240) / 240.0);
                    double radVertAngle = currentVertAngle * Math.PI / 180.0;
                    double calculatedDistance = heightKinect / Math.Cos(radVertAngle);

                    double currentHorAngle = -(horizontalWide / 2.0) + i / 640.0 * horizontalWide;
                    double radHorAngle = currentHorAngle / 180.0 * Math.PI;
                    double finalDistance = calculatedDistance / Math.Cos(radHorAngle);

                    depthArray[i, j] = (int)finalDistance;
                }
            }
                for (int x = 0; x < Width; x++)
                {
                    for (int j = 240; j < Height; j++)
                    {
                        double currentVertAngle = angleKinect - (verticalWide / 2.0) * ((j - 240) / 240.0);
                        double radVertAngle = currentVertAngle * Math.PI / 180.0;
                        double calculatedDistance = heightKinect / Math.Cos(radVertAngle);

                        double currentHorAngle = -(horizontalWide / 2.0) + x / 640.0 * horizontalWide;
                        double radHorAngle = currentHorAngle / 180.0 * Math.PI;
                        double finalDistance = calculatedDistance / Math.Cos(radHorAngle);
                        double horDistance = finalDistance * Math.Sin(radHorAngle);
                        int finalValue = (int)horDistance;

                        if (Math.Abs(Math.Abs(finalValue) - radius) < 3)
                        {
                            depthArray[x, j] = -2;
                        }
                    }
                }

            
            marcSimulatedDepthArray = depthArray;
        }


        private int[,] provideTestingDepthMapFromFile()
        {
            //Testing Purposes
          //  TextReader txtReader = new StreamReader("FlatFloorCalibrationFile.txt");
            TextReader txtReader = new StreamReader("TESTINGFILE.txt");
            txtReader.ReadLine();
            txtReader.ReadLine();
            int[,] providedDepthMap = new int[640, 480];

            for (int x = 0; x < 640; x++)
            {
                for (int y = 0; y < 480; y++)
                {
                    providedDepthMap[x, y] = Convert.ToInt32(txtReader.ReadLine());
                }
            }
            depthMapLoaded = true;
            return providedDepthMap;
        }

        private byte[] checkIfFlatFloor(int[] realDepthArray)
        {
            
            if (!depthMapLoaded)//Get the testing depth map from our file
                providedDepthMap = provideTestingDepthMapFromFile();
          


            //Should be 43 but we calculated something around 49...
            double verticalAngle = 48.5;
            double horizontalAngle = 57.0;

            //Two dimensional depth array
            int[,] depthArray = new int[640, 480];

            //Place one dimensional depth array into two dimensional depth array
      /*      for (int x = 0; x < 640; x++)
            {
                for (int y = 0; y < 480; y++)
                {
                    depthArray[x, y] = realDepthArray[x + y * 640];
                }
            }*/

            //If we want to calibrate, then we save an image containing a flat surface
            if (!savedFlag)
                saveDepthMapToFile(realDepthArray); 

         
            
            int[,] diffDepthArray = new int[640, 480];
            for (int x = 0; x < 640; x++)
            {
                for (int y = 0; y < 480; y++)
                {
                 //   diffDepthArray[x, y] = depthArray[x, y] - providedDepthMap[x, y];
                   diffDepthArray[x, y] = realDepthArray[x + y*640] - providedDepthMap[x, y];
                }
            }
            
            Boolean goodFlag = true;
            for (int y = 240; y < 480; y++)
            {
                int startDetect = 0;
                for (int x = 0; x < 640; x++)
                {
                    int value = marcSimulatedDepthArray[x, y];
                    if (value == -2 && startDetect == 0)
                    {
                        startDetect = 1;
                    }
                    else if (!(value == -2) && startDetect == 1)
                    {
                        startDetect = 2;
                    }
                    else if (value != -2 && startDetect == 2)
                    {
                        if (Math.Abs(diffDepthArray[x, y]) < MaximumError)
                        {
                            
                        }
                        else
                        {
                            goodFlag = false;
                        }
                    }
                    else if (marcSimulatedDepthArray[x, y] == -2 && startDetect == 2)
                    {
                        break;
                    }
                    

                }
            }


            flatSurface = goodFlag;

            byte[] colorDepthArray = convertDiffToColor(diffDepthArray);

            return colorDepthArray;

        }

        Boolean lastStatus = false;

        private void analyzeRealDepthArray(int[] givenRealDepthArray)
        {

            int initialCounter = 5;
            int counter = initialCounter;
            int valid = -1;
            int maxHeight = 30;

            int[ , ] twodDepth = new int[640,480];


            for (int x = 0; x < 640; x++)
            {
                for (int y = 0; y < 480; y++)
                {
                    twodDepth[x, y] = givenRealDepthArray[x + y * 640];
                }
            }

            int[,] new2DDepth = new int[640, 480];

            double dAngleX = 58.0 / 640;
            double dAngleY = 45.0 / 480;

            for (int x = 0; x < 640; x++)
            {
                for (int y = 0; y < 480; y++)
                {
                    double angleX = 640 * dAngleX - 58 / 2;
                    double angleY = 480 * dAngleY - 45.0 / 2;
                    double angleZ = Math.Atan(Math.Sqrt(Math.Tan(angleX) * Math.Tan(angleX) + Math.Tan(angleY) * Math.Tan(angleY)));
                    int value = (int) (Math.Sin(angleZ) * twodDepth[x, y]);
                    new2DDepth[x, y] = value;
                }
            }


            //Let's do every line
            for (int j = 0; j < 480; j++)
            {
                for (int i = 0; i < 640; i++)
                {
                    if (new2DDepth[i, j] > 100 && new2DDepth[i, j] < MaximumDistance)
                    {
                        if (valid == -1)
                        {
                            valid = new2DDepth[i, j];
                        }
                        else if (Math.Abs(valid - new2DDepth[i, j]) < maxHeight)
                        {
                            valid = new2DDepth[i, j];
                            counter = initialCounter;
                        }
                        else
                        {
                            counter--;
                            if (counter < 1)
                            {
                                Console.WriteLine(valid + " : " + new2DDepth[i, j] + " : " + counter);
                            }
                            
                            //We fail. We can't roll on that print an error message.
                            //Exit
                            if (lastStatus && counter == 0)
                            {
                                Console.WriteLine("FAIL");
                                lastStatus = false;
                            }
                            if (counter == 0)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            //Print success
            //if (!lastStatus)
            {
                Console.WriteLine("SUCCESS");
                lastStatus = true;
            }
        }

        private void grid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                if (flatSurface)
                    Console.Write("FLAT!");
                else
                    Console.Write("Not flat!");
            }
            else if (e.Key == Key.C)
            {
                Console.WriteLine("Saving Calibration Image");
                savedFlag = false;
                
            } 
        }

   }
}