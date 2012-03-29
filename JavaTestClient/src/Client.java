import java.net.Socket;
import java.net.SocketException;
import java.io.OutputStream;
import java.io.IOException;
import java.io.InputStream;


public class Client  { 
	protected static OutputStream outToServer;
	protected static InputStream inFromServer; 
	protected static Socket clientSocket;
	
	public static void main(String argv[]) throws Exception {
		
		System.out.println("start to connect!!");
		clientSocket = new Socket("192.168.3.110", 1234);
		System.out.println("connected!!");
		outToServer = clientSocket.getOutputStream();
		inFromServer = clientSocket.getInputStream();
		(new Thread() {
			public void run() {
				int outCount = 0;
				System.out.println("started timed thread");
				long lastTime = System.currentTimeMillis();
				while (true) {
					try {
						outToServer.write('a');
						String response = inFromServer.read() == '1' ? "True" : "False";
						long currTime = System.currentTimeMillis();
						if (outCount++ % 2 == 0) {
							System.out.println("response: " + response + "; delta: " + (currTime - lastTime));
						}
						lastTime = currTime;
					} catch (IOException e) {
						if (e instanceof SocketException) {
							break;
						}
						e.printStackTrace();
//					} catch (InterruptedException e) {
					}
				}
			}
		}).start();
	}


}
