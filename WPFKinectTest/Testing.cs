using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFKinectTest
{
    class Testing
    {
        int[] testMap;
        FloorDetection floorDetection = new FloorDetection();
        
        int[] leftZoneMap; //Map with something in the left zone and all others clear
        int[] rightZoneMap; //Map with something in the right zone and all others clear
        int[] topZoneMap; //Map with something in the top zone and all others clear
        int[] bottomZoneMap; //Map with something in the bottom zone and all others clear
        int[] allZoneMap; // Map with something in all the zones and all others clear
        int[] clearZoneMap; //Map with nothing in all the zones

        public Testing()
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
            allZoneMap = floorDetection.testing_LoadMap("allZone.txt");
            clearZoneMap = floorDetection.testing_LoadMap("clearZone.txt"); 
        }


        //We want all tests to pass, meaning we want them to all equal true

        private void testing_LeftZone()
        {
           floorDetection.testing_CheckIfFlatFloor(leftZoneMap);
           Console.WriteLine(!floorDetection.flatSurfaceLeft); //Print true if something exists in left zone
        }

        private void testing_RightZone()
        {
            floorDetection.testing_CheckIfFlatFloor(rightZoneMap);
            Console.WriteLine(!floorDetection.flatSurfaceRight); //Print true if something exists in right zone
        }

        private void testing_TopZone()
        {
            floorDetection.testing_CheckIfFlatFloor(topZoneMap);
            Console.WriteLine(!floorDetection.flatSurfaceUp); //Print true if something exists in top zone
        }

        private void testing_BottomZone()
        {
            floorDetection.testing_CheckIfFlatFloor(bottomZoneMap);
            Console.WriteLine(!floorDetection.flatSurfaceLeft); //Print true if something exists in bottom zone
        }

        private void testing_AllZone()
        {
            floorDetection.testing_CheckIfFlatFloor(allZoneMap); //Load map containing all zones being nonflat
            if (floorDetection.flatSurfaceDown || floorDetection.flatSurfaceLeft || floorDetection.flatSurfaceRight || floorDetection.flatSurfaceUp) //If any zones are reporting flat we fail
                Console.WriteLine(false); 
            else
                Console.WriteLine(true); 
        }

        private void testing_ClearZone()
        {
            floorDetection.testing_CheckIfFlatFloor(clearZoneMap);
            if (floorDetection.flatSurfaceDown && floorDetection.flatSurfaceLeft && floorDetection.flatSurfaceRight && floorDetection.flatSurfaceUp)
                Console.WriteLine(true);  //Print true if all zones are clear
            else
                Console.WriteLine(false);
        }






    }
}
