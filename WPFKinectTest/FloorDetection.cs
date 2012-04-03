using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WPFKinectTest {
    class FloorDetection {

        public Boolean flatSurfaceUp = true;
        public Boolean flatSurfaceDown = true;
        public Boolean flatSurfaceLeft = true;
        public Boolean flatSurfaceRight = true;

        //Maximum Error allowed between depth array and calibration image
        public int MaximumError = 100;
        public int MinimumInteferance = 10;

        const int TOP = 300;
        int MID = 380;

        //Identify each color layer on the R G B
        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        //Marc Andre's static depth map
        int[,] marcSimulatedDepthArray = new int[640, 480];
        int[,] zoneArray = new int[4, 480 - TOP];

        public Boolean savedFlag = true;
        Boolean depthMapLoaded = false;

        //Depth map loaded by test file
        int[,] providedDepthMap;

        public FloorDetection() {
            //Build Marc Andre's static depth map
            buildMarcSimulatedDepthMap();
            createZones();
        }

        public byte[] execute(int[] realDepthArray) {
            return checkIfFlatFloor(realDepthArray);
        }

        //================= PRIVATE METHODS =================================

        private int[,] provideTestingDepthMapFromFile() {
            //Testing Purposes
            TextReader txtReader = new StreamReader("TESTINGFILE.txt");
            int[,] providedDepthMap = new int[640, 480];

            for (int x = 0; x < 640; x++) {
                for (int y = 0; y < 480; y++) {
                    providedDepthMap[x, y] = Convert.ToInt32(txtReader.ReadLine());
                }
            }
            depthMapLoaded = true;
            return providedDepthMap;
        }

        //Here we save an image from the kinect camera so that we may run
        //automated tests later
        private void saveDepthMapToFile(int[] depthArray) {
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

        private byte[] convertDiffToColor(int[,] diffDepthArray) {

            byte[] colorDepthArray = new byte[4 * 640 * 480];

            for (int x = 0; x < 640; x++) {
                for (int y = 0; y < 480; y++) {
                    int diff = diffDepthArray[x, y];
                    int r = 0, g = 0, b = 0;
                    if (y == TOP && (zoneArray[0, 0] <= x && zoneArray[3, 0] >= x)) {
                        r = flatSurfaceUp ? 0 : 255;
                        g = flatSurfaceUp ? 255 : 0;
                        b = flatSurfaceUp ? 0 : 255;
                    } else if (y == MID && (zoneArray[1, MID - TOP] <= x && zoneArray[2, MID - TOP] >= x)) {
                        r = flatSurfaceDown ? 0 : 255;
                        g = flatSurfaceDown ? 255 : 0;
                        b = flatSurfaceDown ? 0 : 255;
                    } else if (y >= TOP && (zoneArray[0, y - TOP] == x || zoneArray[1, y - TOP] == x)) {
                        r = flatSurfaceLeft ? 0 : 255;
                        g = flatSurfaceLeft ? 255 : 0;
                        b = flatSurfaceLeft ? 0 : 255;
                    } else if (y >= TOP && (zoneArray[2, y - TOP] == x || zoneArray[3, y - TOP] == x)) {
                        r = flatSurfaceRight ? 0 : 255;
                        g = flatSurfaceRight ? 255 : 0;
                        b = flatSurfaceRight ? 0 : 255;
                    } else if (Math.Abs(diff) < MaximumError) {
                        r = 255;
                        g = 255;
                        b = 255;
                    } else if (diff < -MaximumError) {
                        int col = 200 - Math.Min(-diff, 200);
                        r = 255;
                        g = col;
                        b = col;
                    } else if (diff > MaximumError) {
                        int col = 200 - Math.Min(diff, 200);
                        r = col;
                        g = col;
                        b = 255;
                    }
                    colorDepthArray[4 * (x + y * 640) + RedIndex] = (byte) r;
                    colorDepthArray[4 * (x + y * 640) + GreenIndex] = (byte) g;
                    colorDepthArray[4 * (x + y * 640) + BlueIndex] = (byte) b;
                }
            }

            return colorDepthArray;
        }

        private byte[] checkIfFlatFloor(int[] realDepthArray) {

            if (!depthMapLoaded)//Get the testing depth map from our file
            {
                providedDepthMap = provideTestingDepthMapFromFile();
            }

            //If we want to calibrate, then we save an image containing a flat surface
            if (!savedFlag) {
                saveDepthMapToFile(realDepthArray);
            }



            int[,] diffDepthArray = new int[640, 480];
            for (int x = 0; x < 640; x++) {
                for (int y = 0; y < 480; y++) {
                    diffDepthArray[639 - x, y] = realDepthArray[x + y * 640] - providedDepthMap[x, y];
                }
            }


            int goodFlagUp = 0;
            int goodFlagDown = 0;
            int goodFlagLeft = 0;
            int goodFlagRight = 0;
            for (int y = TOP; y < 480; y++) {
                for (int x = 0; x < 640; x++) {
                    if (x < zoneArray[0, y - TOP]) {
                        //Do nothing we are at the begin
                    } else if (x >= zoneArray[0, y - TOP] && x <= zoneArray[1, y - TOP]) {
                        if (Math.Abs(diffDepthArray[x, y]) >= MaximumError) {
                            goodFlagLeft++;
                        }
                    } else if (x > zoneArray[1, y - TOP] && x < zoneArray[2, y - TOP]) {
                        if (y < MID) {
                            if (Math.Abs(diffDepthArray[x, y]) >= MaximumError) {
                                goodFlagUp++;
                            }
                        } else {
                            if (Math.Abs(diffDepthArray[x, y]) >= MaximumError) {
                                goodFlagDown++;
                            }
                        }
                    } else if (x >= zoneArray[2, y - TOP] && x <= zoneArray[3, y - TOP]) {
                        if (Math.Abs(diffDepthArray[x, y]) >= MaximumError) {
                            goodFlagRight++;
                        }
                    } else if (x > zoneArray[3, y - TOP]) {
                        //We reach the end of the testing zone.
                        break;
                    }
                }
            }


            flatSurfaceUp = goodFlagUp < MinimumInteferance;
            flatSurfaceDown = goodFlagDown < MinimumInteferance;
            flatSurfaceLeft = goodFlagLeft < MinimumInteferance;
            flatSurfaceRight = goodFlagRight < MinimumInteferance;

            byte[] colorDepthArray = convertDiffToColor(diffDepthArray);

            return colorDepthArray;

        }

        private void buildMarcSimulatedDepthMap() {
            int Width = 640;
            int Height = 480;

            int heightKinect = 660;
            double angleKinect = 71;

            double verticalWide = 42;
            double horizontalWide = 57;

            //int MaxDistance = 4096;
            int radius = 400;

            int[,] depthArray = new int[640, 480];

            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {
                    double currentVertAngle = angleKinect - (verticalWide / 2.0) * ((j - TOP) / (480.0 - TOP));
                    double radVertAngle = currentVertAngle * Math.PI / 180.0;
                    double calculatedDistance = heightKinect / Math.Cos(radVertAngle);

                    double currentHorAngle = -(horizontalWide / 2.0) + i / 640.0 * horizontalWide;
                    double radHorAngle = currentHorAngle / 180.0 * Math.PI;
                    double finalDistance = calculatedDistance / Math.Cos(radHorAngle);

                    depthArray[i, j] = (int) finalDistance;
                }
            }
            for (int x = 0; x < Width; x++) {
                for (int j = TOP; j < Height; j++) {
                    double currentVertAngle = angleKinect - (verticalWide / 2.0) * ((j - TOP) / (480.0 - TOP));
                    double radVertAngle = currentVertAngle * Math.PI / 180.0;
                    double calculatedDistance = heightKinect / Math.Cos(radVertAngle);

                    double currentHorAngle = -(horizontalWide / 2.0) + x / 640.0 * horizontalWide;
                    double radHorAngle = currentHorAngle / 180.0 * Math.PI;
                    double finalDistance = calculatedDistance / Math.Cos(radHorAngle);
                    double horDistance = finalDistance * Math.Sin(radHorAngle);
                    int finalValue = (int) horDistance;

                    if (Math.Abs(Math.Abs(finalValue) - radius) < 3) {
                        depthArray[x, j] = -2;
                    }
                }
            }


            marcSimulatedDepthArray = depthArray;
        }

        private void createZones() {

            for (int y = TOP; y < 480; y++) {
                int startDetect = 0;
                for (int x = 0; x < 640; x++) {
                    int value = marcSimulatedDepthArray[x, y];
                    if (value == -2 && startDetect == 0) {
                        startDetect = 1;
                    } else if (!(value == -2) && startDetect == 1) {
                        startDetect = 2;
                        zoneArray[0, y - TOP] = x;
                        zoneArray[1, y - TOP] = x + getZoneSize(y);
                    } else if (value != -2 && startDetect == 2) {

                    } else if (marcSimulatedDepthArray[x, y] == -2 && startDetect == 2) {
                        zoneArray[3, y - TOP] = x - 1;
                        zoneArray[2, y - TOP] = x - 1 - getZoneSize(y);
                        break;
                    }
                }
            }

        }

        private int getZoneSize(int y) {
            int maxSize = 50;
            int minSize = 0;
            int dif = maxSize - minSize;
            int result = (int) (minSize + dif * (480.0 - y) / TOP);
            return result;
        }

    }

}
