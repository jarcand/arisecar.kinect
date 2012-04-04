using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;

namespace WPFKinectTest {
    class FloorServer {
        //Server handling client requests asking for flat surface
        System.Net.Sockets.TcpListener Server;

        FloorDetection floorDetection;

        public FloorServer(FloorDetection floorDetection) {
            this.floorDetection = floorDetection;
            Thread thread = new Thread(new ThreadStart(AcceptClients));
            thread.Start();
        }

        private void AcceptClients() {
            Server = new System.Net.Sockets.TcpListener(IPAddress.Any, 1234);
            Console.WriteLine("\nWaiting for Clients");
            Server.Start();
            while (true) {
                System.Net.Sockets.TcpClient chatConnection = Server.AcceptTcpClient();
                Thread thread = new Thread(communicateWithClient);
                thread.Start(chatConnection);
            }

        }

        int connections = 0;

        private void communicateWithClient(object givenChatConnection) {
            System.Net.Sockets.TcpClient chatConnection = (System.Net.Sockets.TcpClient) givenChatConnection;
            connections++;
            Console.WriteLine("Someone connected!  Connection count is " + connections);
            StreamReader inputStream = new System.IO.StreamReader(chatConnection.GetStream());
            StreamWriter outputStream = new System.IO.StreamWriter(chatConnection.GetStream());
            Console.WriteLine("Reader stream");
            try {
                int input = 0;
                while (input != -1) {
                    input = inputStream.Read();
                    switch (input) {
                        case 'a':
                            //Console.WriteLine("Received request: " + flatSurface);
                            int resp = (floorDetection.flatSurfaceDown ? 1 : 0) << 0
                                | (floorDetection.flatSurfaceUp ? 1 : 0) << 1
                                | (floorDetection.flatSurfaceLeft ? 1 : 0) << 2
                                | (floorDetection.flatSurfaceRight ? 1 : 0) << 3;
                            char respB = (char) resp;
                            outputStream.Write(respB);
                            outputStream.Flush();
                            break;
                        case 'c':
                            floorDetection.savedFlag = false;
                            break;
                        case 'e':
                            int newMaxE1 = inputStream.Read();
                            int newMaxE2 = inputStream.Read();
                            int newMaxE = newMaxE2 << 7 | newMaxE1;
                            if (0 <= newMaxE && newMaxE <= 16383) {
                                floorDetection.MaximumError = newMaxE;
                            }
                            break;
                        case 'i':
                            int newMinI1 = inputStream.Read();
                            int newMinI2 = inputStream.Read();
                            int newMinI = newMinI2 << 7 | newMinI1;
                            if (0 <= newMinI && newMinI <= 16383) {
                                floorDetection.MinimumInteferance = newMinI;
                            }
                            break;
                        case 0:
                            break;
                        default:
                            Console.WriteLine("Junk:" + input);
                            break;
                    }
                }
            } catch (IOException e) {
                Console.WriteLine("IOException: " + e);
            }
            connections--;
            Console.WriteLine("Client disconnected.  Connection count is " + connections);
        }
    }
}
