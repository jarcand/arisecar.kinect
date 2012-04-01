using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;

namespace WPFKinectTest
{
    //Simulation Server running without the kinect, with a simulated depth image

    class TestingServer
    {
        public TestingServer()
        {
            //Create a floorDetection object detecting the floor of a randomly generated simulation depth map
            TestingFloor testingFloor = new TestingFloor();
            FloorDetection floorDetection = testingFloor.chooseRandomMap();
            FloorServer floorServer = new FloorServer(floorDetection);

        }

    }
}
