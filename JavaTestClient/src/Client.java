import java.net.*;
import java.awt.event.KeyEvent;
import java.awt.event.KeyListener;
import java.io.*;

import javax.swing.JFrame;

public class Client  { 
	protected static DataOutputStream outToServer;
	protected static BufferedReader inFromServer; 
	protected static Socket clientSocket;
	
	public static void main(String argv[]) throws Exception {
		
		
		boolean flag = true;
		final JFrame frame;
		frame = new JFrame("TCP-reciever");
		frame.addKeyListener(new KeyboardControl());
		frame.setSize(400, 400);
		frame.setVisible(true);


//		BufferedReader inFromUser = new BufferedReader(new InputStreamReader(
//				System.in));
		System.out.println("start to connect!!");
		clientSocket = new Socket("192.168.3.110", 1234);
		System.out.println("connected!!");
		outToServer = new DataOutputStream(clientSocket.getOutputStream());
		inFromServer = new BufferedReader(new
		 InputStreamReader(
		 clientSocket.getInputStream()));
//		 for(int i = 0; i<1000; i++ ){
//		 System.out.println(inFromServer.readLine());
//		 }
		inFromServer = new BufferedReader(new
				 InputStreamReader(
				 Client.clientSocket.getInputStream()));
	//	sentence = inFromUser.readLine();
		//outToServer.writeBytes(sentence + '\n');
		// modifiedSentence = inFromServer.readLine();
		 //System.out.println("FROM SERVER: " + modifiedSentence);
		 //clientSocket.close();

		(new Thread() {
			public void run() {
				System.out.println("started timed thread");
				long lastTime = System.currentTimeMillis();
				while (true) {
					try {
						outToServer.writeChar('a');
						outToServer.writeByte('\n');
						outToServer.flush();
						long currTime = System.currentTimeMillis();
						System.out.println("response: " + inFromServer.readLine() + "; delta: " + (currTime - lastTime));
						lastTime = currTime;
//						Thread.sleep(10);
					} catch (IOException e) {
						if (e instanceof SocketException) {
							break;
						}
						e.printStackTrace();
//					} catch (InterruptedException e) {
//						e.printStackTrace();
					}
				}
			}
		}).start();
	}

}

class KeyboardControl extends Client implements KeyListener {

	@Override
	public void keyPressed(KeyEvent e) {
		// System.out.println("pressed: " + e.getKeyCode());
		if (e.getKeyCode() == KeyEvent.VK_A) {
			try {
				
				System.out.println("Pressed key down");
				outToServer.writeChar('a');
				outToServer.writeByte('\n');
				outToServer.flush();
				System.out.println("response: " + inFromServer.readLine());

				//System.out.println(inFromServer.readLine());
				
			} catch (IOException e1) {
				// TODO Auto-generated catch block
				e1.printStackTrace();
			}
		}
	}

	@Override
	public void keyReleased(KeyEvent arg0) {
		// TODO Auto-generated method stub

	}

	@Override
	public void keyTyped(KeyEvent arg0) {
		// TODO Auto-generated method stub

	}

}
