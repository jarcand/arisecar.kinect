using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WPFKinectTest
{
    class FloorDetection
    {

        int mid = 360;
        public Boolean flatSurfaceUp = true;
        public Boolean flatSurfaceDown = true;
        public Boolean flatSurfaceLeft = true;
        public Boolean flatSurfaceRight = true;

        //Maximum Error allowed between depth array and calibration image
        int MaximumError = 100;

        //Identify each color layer on the R G B
        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        //Marc Andre's static depth map
        int[,] marcSimulatedDepthArray = new int[640, 480];
        int[,] zoneArray = new int[4, 240];

        public Boolean savedFlag = true;
        Boolean depthMapLoaded = false;

        //Depth map loaded by test file
        int[,] providedDepthMap;

        public FloorDetection()
        {
            //Build Marc Andre's static depth map
            buildMarcSimulatedDepthMap();
            createZones();
        }

        public byte[] execute(int[] realDepthArray)
        {
            return checkIfFlatFloor(realDepthArray);
        }

        //================= PRIVATE METHODS =================================

        private int[,] provideTestingDepthMapFromFile()
        {
            //Testing Purposes
            TextReader txtReader = new StreamReader("TESTINGFILE.txt");
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

        //Here we save an image from the kinect camera so that we may run
        //automated tests later
        private void saveDepthMapToFile(int[] depthArray)
        {
            //Here we write the depth map to a file for testing purposes
            TextWriter tw = new StreamWriter("TESTINGFILE.txt");

            for (int i = 0; i < 640; i++)
                for (int j = 0; j < 480; j++)
                    tw.WriteLine(depthArray[i + j * 640]);

            // close the stream
            tw.Close();
            savedFlag = true;
            providedDepthMap = provideTestingDepthMapFromFile();
        }

        private byte[] convertDiffToColor(int[,] diffDepthArray)
        {

            byte[] colorDepthArray = new byte[4 * 640 * 480];

            for (int x = 0; x < 640; x++)
            {
                for (int y = 0; y < 480; y++)
                {
                    int diff = diffDepthArray[x, y];
                    if ((y == 240 && (zoneArray[0, 240 - 240] <= x && zoneArray[3, 240 - 240] >= x))
                        || (y == mid && (zoneArray[1, mid - 240] <= x && zoneArray[2, mid - 240] >= x)))
                    {
                        colorDepthArray[4 * (x + y * 640) + RedIndex] = 0;
                        colorDepthArray[4 * (x + y * 640) + GreenIndex] = 255;
                        colorDepthArray[4 * (x + y * 640) + BlueIndex] = 0;
                    }
                    else if (y >= 240 &&
                        (zoneArray[0, y - 240] == x || zoneArray[1, y - 240] == x
                        || zoneArray[2, y - 240] == x || zoneArray[3, y - 240] == x))
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

        private byte[] checkIfFlatFloor(int[] realDepthArray)
        {

            if (!depthMapLoaded)//Get the testing depth map from our file
            {
                providedDepthMap = provideTestingDepthMapFromFile();
            }

            //If we want to calibrate, then we save an image containing a flat surface
            if (!savedFlag)
            {
                saveDepthMapToFile(realDepthArray);
            }



            int[,] diffDepthArray = new int[640, 480];
            for (int x = 0; x < 640; x++)
            {
                for (int y = 0; y < 480; y++)
                {
                    diffDepthArray[639 - x, y] = realDepthArray[x + y * 640] - providedDepthMap[x, y];
                }
            }


            Boolean goodFlagUp = true;
            Boolean goodFlagDown = true;
            Boolean goodFlagLeft = true;
            Boolean goodFlagRight = true;
            for (int y = 240; y < 480; y++)
            {
                for (int x = 0; x < 640; x++)
                {
                    if (x < zoneArray[0, y - 240])
                    {
                        //Do nothing we are at the begin
                    }
                    else if (x >= zoneArray[0, y - 240] && x <= zoneArray[1, y - 240])
                    {
                        if (Math.Abs(diffDepthArray[x, y]) >= MaximumError)
                        {
                            goodFlagLeft = false;
                        }
                    }
                    else if (x > zoneArray[1, y - 240] && x < zoneArray[2, y - 240])
                    {
                        if (y < mid)
                        {
                            if (Math.Abs(diffDepthArray[x, y]) >= MaximumError)
                            {
                                goodFlagUp = false;
                            }
                        }
                        else
                        {
                            if (Math.Abs(diffDepthArray[x, y]) >= MaximumError)
                            {
                                goodFlagDown = false;
                            }
                        }
                    }
                    else if (x >= zoneArray[2, y - 240] && x <= zoneArray[3, y - 240])
                    {
                        if (Math.Abs(diffDepthArray[x, y]) >= MaximumError)
                        {
                            goodFlagRight = false;
                        }
                    }
                    else if (x > zoneArray[3, y - 240])
                    {
                        //We reach the end of the testing zone.
                        break;
                    }
                }
            }


            flatSurfaceUp = goodFlagUp;
            flatSurfaceDown = goodFlagDown;
            flatSurfaceLeft = goodFlagLeft;
            flatSurfaceRight = goodFlagRight;

            byte[] colorDepthArray = convertDiffToColor(diffDepthArray);

            return colorDepthArray;

        }

        private void buildMarcSimulatedDepthMap()
        {
            int Width = 640;
            int Height = 480;

            int heightKinect = 660;
            double angleKinect = 71;

            double verticalWide = 42;
            double horizontalWide = 57;

            //int MaxDistance = 4096;
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

        private void createZones()
        {

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
                        zoneArray[0, y - 240] = x;
                        zoneArray[1, y - 240] = x + getZoneSize(y);
                    }
                    else if (value != -2 && startDetect == 2)
                    {

                    }
                    else if (marcSimulatedDepthArray[x, y] == -2 && startDetect == 2)
                    {
                        zoneArray[3, y - 240] = x - 1;
                        zoneArray[2, y - 240] = x - 1 - getZoneSize(y);
                        break;
                    }
                }
            }

        }

        private int getZoneSize(int y)
        {
            int maxSize = 50;
            int minSize = 0;
            int dif = maxSize - minSize;
            int result = (int)(minSize + dif * (480 - y) / 240.0);
            return result;
        }

    }
   
}
