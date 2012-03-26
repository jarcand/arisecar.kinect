import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;

class RecieveThread extends Thread {
    public RecieveThread() {
	super();
    }
    
    
    public void run() {
    	
    	
    	boolean flag = true;
    	try {
			Client.inFromServer = new BufferedReader(new
					 InputStreamReader(
					 Client.clientSocket.getInputStream()));
			
			
			while(flag){
				System.out.println("try to recieve message!!");
				System.out.println(Client.inFromServer.readLine());
				System.out.println("recieved!!");
			}
		} catch (IOException e) {
			e.printStackTrace();
		}
		
    	
    }
}