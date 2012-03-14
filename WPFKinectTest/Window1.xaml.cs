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

        //XBox Kinects (default) are limited between 800mm and 4096mm.
        int MinimumDistance = 800;
        int MaximumDistance = 4096;

        Boolean savedFlag = false;

        //height from camera lense to floor
        double heightOfCamera = 760; //in mm 

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

            
            //checkIfFlatFloorTest(); //For testing purposes
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
                
                
                
            //    Console.Write(realDepth+" ");



                //We are left with 13 bits of depth information that we need to convert into an 8 bit number for each pixel.
                //There are hundreds of ways to do this. This is just the simplest one.
                //Lets create a byte variable called Distance. 
                //We will assign this variable a number that will come from the conversion of those 13 bits.
                byte Distance = 0;
                


                //XBox Kinects (default) are not reliable closer to 800mm, so let's take those useless measurements out.
                //If the distance on this pixel is bigger than 800mm, we will paint it in its equivalent gray
                if (realDepth > MinimumDistance)
                {
                    //Convert the realDepth into the 0 to 255 range for our actual distance.
                    //Use only one of the following Distance assignments 
                    //White = Far
                    //Black = Close
                    //Distance = (byte)(((realDepth - MinimumDistance) * 255 / (MaximumDistance-MinimumDistance)));

                    //White = Close
                    //Black = Far
                    Distance = (byte)(255-((realDepth - MinimumDistance) * 255 / (MaximumDistance - MinimumDistance)));
                    
                    //Use the distance to paint each layer (R G & B) of the current pixel.
                    //Painting R, G and B with the same color will make it go from black to gray
                    this.depthFrame32[i32 + RedIndex] = (byte)(Distance);
                    this.depthFrame32[i32 + GreenIndex] = (byte)(Distance);
                    this.depthFrame32[i32 + BlueIndex] = (byte)(Distance);
                    //this.depthFrame32[i32 + RedIndex] = 150;
                    //this.depthFrame32[i32 + GreenIndex] = 150;
                    //this.depthFrame32[i32 + BlueIndex] = 150;
                }

                //If we are closer than 800mm, the just paint it red so we know this pixel is not giving a good value
                else
                {
                    this.depthFrame32[i32 + RedIndex] = 150;
                    this.depthFrame32[i32 + GreenIndex] = 0;
                    this.depthFrame32[i32 + BlueIndex] = 0;
                }
            }

            //Analyze realDepth array:
            //analyzeRealDepthArray(realDepthArray);
           checkIfFlatFloor(realDepthArray);

            //Now that we are done painting the pixels, we can return the byte array to be painted
            return this.depthFrame32;
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

        private void modifyDepthArray(int[] depthArray)
        {


        }


        //Here we save an image from the kinect camera so that we may run
        //automated tests later
        private void saveDepthMapToFile(int[,] twodDepth)
        {
            //Here we write the depth map to a file for testing purposes
            TextWriter tw = new StreamWriter("TESTINGFILE.txt");
            
            //Write the height, then the best angle
            tw.WriteLine(heightOfCamera);
            tw.WriteLine(bestAngle);

            for (int i = 0; i < 640; i++)
                for (int j = 0; j < 480; j++)
                    tw.WriteLine(twodDepth[i, j]);

            // close the stream
            tw.Close();
            savedFlag = true;
            
        }


        //Method used for testing simulated depth maps
        private void checkIfFlatFloorTest()
        {
            //Open the file that contains the depth map of what you want to test
            TextReader txtReader = new StreamReader("nonFlatFloorHeight760mmElevationminus20.txt");
            int[,] depthArray = new int[640, 480];

            //Set the testingAngle or  use a floor loop on a flat surface to find out the best angle
            int testingHeight = Convert.ToInt32(txtReader.ReadLine());
            int testingAngle = Convert.ToInt32(txtReader.ReadLine());
            

            //Convert txt file to two dimensional depth map
            for (int x = 0; x < 640; x++)
            {
                for (int y = 0; y < 480; y++)
                {
                    depthArray[x, y] = Convert.ToInt32(txtReader.ReadLine());
                }
            }

            //Test the depth map
                    double radAngle = Math.PI / 180 * testingAngle;
                    double calculatedDistance = heightOfCamera / Math.Cos(radAngle);
                    if (Math.Abs(depthArray[320, 240] - calculatedDistance) < 50)
                    {
                        Console.WriteLine("\nTesting: Flat");
                    }
                    else
                    {
                        Console.WriteLine("\nTesting: NOT Flat");
                    }
        }


        private void checkIfFlatFloor(int[] realDepthArray)
        {
            //Two dimensional depth array
            int[,] twodDepth = new int[640, 480];

            //Place one dimensional depth array into two dimensional depth array
            for (int x = 0; x < 640; x++)
            {
                for (int y = 0; y < 480; y++)
                {
                    twodDepth[x, y] = realDepthArray[x + y * 640];
                }
            }


            //Find the best angle that allows us to calculate distance from middle point [320,240]
            //to camera, and then that value be closest to actual value given to us by kinect

            if (bestAngle == -361)
            {
                for (int angle = 50; angle <= 90; angle++)
                {

                    double radAngle = Math.PI / 180 * angle;
                    double calculatedDistance = heightOfCamera / Math.Cos(radAngle);
                    if (Math.Abs(twodDepth[320, 240] - calculatedDistance) < 50)
                    {
                        Console.WriteLine("\n"+angle + " Flat");
                        bestAngle = angle;
                        //saveDepthMapToFile(twodDepth); //Saving depth map for testing purposes
                        break;
                    }
                }
            }
            else
            {
                //We've already figured out the best angle now we must detect if a non-flat surface
                //is ahead of us
                double radAngle = Math.PI / 180 * bestAngle;
                double calculatedDistance = heightOfCamera / Math.Cos(radAngle);
                if (Math.Abs(twodDepth[320, 240] - calculatedDistance) < 50)
                {
                    int pointsPerAngle = (480 / 43);
                    int middlePoint = 320;
                    for (int j = 240; j < 480 - pointsPerAngle;)
                    {
                        radAngle -= 1;
                        calculatedDistance = heightOfCamera / Math.Cos(radAngle);
                        if (Math.Abs(twodDepth[middlePoint, j += pointsPerAngle] - calculatedDistance) < 50)
                            Console.WriteLine("Flat" + j);
                        
                    }

                    /*
                    for (int j = 240; j < 480; j++)
                    {
                        int n = j - 240;
                        double vertAngle = bestAngle + (n / 480.0 * (43));
                        double dStar = heightOfCamera / Math.Cos(vertAngle);
                        double radiusRobot = 800 / 2;
                        double horAngle = Math.Atan2(radiusRobot, dStar);
                        int point = (int)((horAngle / (57)) * 640);

                        for (int i = point; i < 640 - point; i++)
                        {
                     
                        }
                    }*/
                }
                else
                {
                    Console.WriteLine(bestAngle + "\n Not Flat!");
                  //  if(savedFlag)
                      //  saveDepthMapToFile(twodDepth);
                }
            }


            



            
          

            /*
            if (horizontalPlane == -1)
            {
                for (int i = 639; i > 320; i--)
                {
                    for (int j = 0; j < 480; j++)
                    {
                        //We'll only look at valid values for now
                        if (twodDepth[i,j] != -1 && Math.Abs(twodDepth[i, j] - calculatedDistance) < 10)
                        {
                            horizontalPlane = i;
                        }

                    }
                }
            }

            for (int x = 0; x < 480; x++)
            {
                //We'll only look at valid values for now
                if (twodDepth[horizontalPlane,x] != -1)
                {
                    averageActualDistance += twodDepth[horizontalPlane, x];
                    counter++;
                }

            }


                averageActualDistance /= counter;
                if (Math.Abs(averageActualDistance - calculatedDistance) > 100)
                {
                    Console.WriteLine("Not flat!");
                }
                else
                {
                    Console.WriteLine("Flat!");
                }*/

            }

            /*
            for (int i = 639; i > 320; i++)
            {
                for (int j = 0; j < 480; j++)
                {
                    //We'll only look at valid values for now
                    if (twodDepth[320, j] > MinimumDistance && twodDepth[320, j] < MaximumDistance)
                    {

                    }
                }
            }*/
        


        

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
   }
}