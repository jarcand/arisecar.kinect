using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace WindowsFormsApplication1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            TcpClient client = new System.Net.Sockets.TcpClient("192.168.3.101", 1234);
            System.IO.StreamReader reader = new System.IO.StreamReader(client.GetStream());
            Console.WriteLine(reader.ReadLine());

            }

        }
    
}
