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
		clientSocket = new Socket("192.168.3.101", 1234);
		System.out.println("connected!!");
		outToServer = new DataOutputStream(clientSocket.getOutputStream());
		inFromServer = new BufferedReader(new
		 InputStreamReader(
		 clientSocket.getInputStream()));
//		 for(int i = 0; i<1000; i++ ){
//		 System.out.println(inFromServer.readLine());
//		 }
		RecieveThread recv = new RecieveThread();
		recv.start();
		while(flag){
			
		}
	//	sentence = inFromUser.readLine();
		//outToServer.writeBytes(sentence + '\n');
		// modifiedSentence = inFromServer.readLine();
		 //System.out.println("FROM SERVER: " + modifiedSentence);
		 clientSocket.close();

	}

}

class KeyboardControl extends Client implements KeyListener {

	@Override
	public void keyPressed(KeyEvent e) {
		// System.out.println("pressed: " + e.getKeyCode());
		if (e.getKeyCode() == KeyEvent.VK_X) {
			try {
				
				System.out.println("Pressed key down");
				outToServer.writeChar('a');
				outToServer.writeByte('\n');
				outToServer.flush();
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
