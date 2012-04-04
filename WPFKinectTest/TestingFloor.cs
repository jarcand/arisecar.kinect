using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFKinectTest
{
    class TestingFloor
    {
        int[] testMap;
        FloorDetection floorDetection = new FloorDetection();
        
        int[] leftZoneMap; //Map with something in the left zone and all others clear
        int[] rightZoneMap; //Map with something in the right zone and all others clear
        int[] topZoneMap; //Map with something in the top zone and all others clear
        int[] bottomZoneMap; //Map with something in the bottom zone and all others clear
        int[] allZoneMap; // Map with something in all the zones and all others clear
        int[] clearZoneMap; //Map with nothing in all the zones

        public TestingFloor()
        {
            loadTestMaps();
            testing_LeftZone();
            testing_RightZone();
            testing_TopZone();
            testing_BottomZone();
            testing_AllZone();
            testing_ClearZone();
        }

        //Loading simulation files. The prefixes to ZoneMap indicate whether or not something is in the zone
        //Example: leftZoneMap means something is in the left zone
        private void loadTestMaps()
        {
            leftZoneMap = floorDetection.testing_LoadMap("leftZone.txt");
            rightZoneMap = floorDetection.testing_LoadMap("rightZone.txt");
            topZoneMap = floorDetection.testing_LoadMap("topZone.txt");
            bottomZoneMap = floorDetection.testing_LoadMap("bottomZone.txt");
            allZoneMap = floorDetection.testing_LoadMap("allZones.txt");
            clearZoneMap = floorDetection.testing_LoadMap("clearZone.txt"); 
        }


        //We want all tests to pass, meaning we want them to all equal true

        bool result;
        public bool testing_LeftZone()
        {
           floorDetection.testing_CheckIfFlatFloor(leftZoneMap);
           result = !floorDetection.flatSurfaceLeft;
           Console.WriteLine(result); //Print true if something exists in left zone
           return result;
        }

        public bool testing_RightZone()
        {
            floorDetection.testing_CheckIfFlatFloor(rightZoneMap);
            result = !floorDetection.flatSurfaceRight;
            Console.WriteLine(result); //Print true if something exists in right zone
            return result;
        }

        public bool testing_TopZone()
        {
            floorDetection.testing_CheckIfFlatFloor(topZoneMap);
            result = !floorDetection.flatSurfaceUp;
            Console.WriteLine(result); //Print true if something exists in top zone
            return result;
        }

        public bool testing_BottomZone()
        {
            floorDetection.testing_CheckIfFlatFloor(bottomZoneMap);
            result = !floorDetection.flatSurfaceLeft;
            Console.WriteLine(result); //Print true if something exists in bottom zone
            return result;
        }

        public bool testing_AllZone()
        {
            floorDetection.testing_CheckIfFlatFloor(allZoneMap); //Load map containing all zones being nonflat
            if (floorDetection.flatSurfaceDown || floorDetection.flatSurfaceLeft || floorDetection.flatSurfaceRight || floorDetection.flatSurfaceUp) //If any zones are reporting flat we fail
            {
                Console.WriteLine(false);
                return false;
            }
            else
            {
                Console.WriteLine(true);
                return true;
            }
        }

        public bool testing_ClearZone()
        {
            floorDetection.testing_CheckIfFlatFloor(clearZoneMap);
            if (floorDetection.flatSurfaceDown && floorDetection.flatSurfaceLeft && floorDetection.flatSurfaceRight && floorDetection.flatSurfaceUp)
            {
                Console.WriteLine(true);  //Print true if all zones are clear
                return true;
            }
            else
            {
                Console.WriteLine(false);
                return false;
            }
        }

        public FloorDetection chooseRandomMap()
        {
            System.Random random = new System.Random();
            int[] randomMap;
            switch (random.Next(6))
            {
                case 0:
                    randomMap = leftZoneMap;
                    break;
                case 1:
                    randomMap = rightZoneMap;
                    break;
                case 2:
                    randomMap = topZoneMap;
                    break;
                case 3:
                    randomMap = bottomZoneMap;
                    break;
                case 4:
                    randomMap = allZoneMap;
                    break;
                case 5:
                    randomMap = clearZoneMap;
                    break;
                default:
                    randomMap = allZoneMap;
                    break;
            }

            floorDetection.testing_CheckIfFlatFloor(randomMap);
            return floorDetection;
        }
        




    }
}
