package iha.bachelor.smo.aba.rah.easyconnect_v3.service;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.math.BigInteger;
import java.net.InetAddress;
import java.net.Socket;
import java.util.Arrays;

import android.app.IntentService;
import android.content.Intent;
import android.util.Log;

public class GetDataIntentService extends IntentService
{
	private final String TARGET_IP_ADDRESS = "TargetIpAddress";
	private final String TARGET_MAC_ADDRESS = "TargetMacAddress";
	private final String TARGET_HANDLE = "TargetHandle";
	private static final int SERVERPORT = 4543;
	private final String LOG_TAG = "GetDataIntentService";
	public GetDataIntentService(){
		super("GetDataIntentService");
	}

	@Override
	protected void onHandleIntent(Intent intent) {
		String targetAddress = intent.getStringExtra(TARGET_IP_ADDRESS);
		String targetMac = intent.getStringExtra(TARGET_MAC_ADDRESS);
		String targetHandle = intent.getStringExtra(TARGET_HANDLE);
		
		Socket tcpSocket = null;
		InputStream in = null;
		OutputStream out = null;
		try {
			Log.d(LOG_TAG, "reqDevInf: Connecting...");
			
			if (targetAddress != null)
				tcpSocket = new Socket(InetAddress.getByName(targetAddress), SERVERPORT);
			else
				tcpSocket = new Socket(InetAddress.getByName("192.168.1.3"), SERVERPORT);
			
			Log.d(LOG_TAG, "reqDevInf: Connected...");

			in = tcpSocket.getInputStream();
			out = tcpSocket.getOutputStream();

			Log.d(LOG_TAG, "reqDevInf: Sending command.");
			String outMsg;
			outMsg = "RequestECMData";
			out.write(outMsg.getBytes("UTF-8"));	// Sending Request ECM Data

			byte[] buf = new byte[1024];
			int availableBytes = 0;
			Thread.sleep(500);
			availableBytes = in.read(buf);		// reading if accepted

			if (availableBytes > 0){
				availableBytes = 0;
				Log.d(LOG_TAG, "reqDevInf: Sending macAddres & Handle.");
				byte[] mac;
				
				if (targetMac != null && targetHandle != null)
					mac = new BigInteger(targetMac+targetHandle,16).toByteArray();
				else
					mac = new BigInteger("E68170E5C5780023",16).toByteArray();
				
				out.write(mac, 1, 8);				// Sending MacAddress of requested device

				availableBytes = in.read(buf);	// Reading the ModuleInformation
				if (availableBytes > 0){
					byte[] deviceInformation = Arrays.copyOfRange(buf, 0, availableBytes);
					Log.i(LOG_TAG, "received: ");
				}
			}
		} catch (Exception e) {
			Log.e(LOG_TAG, "Error", e);
		} finally {
			try {
				if ( tcpSocket != null && tcpSocket.isConnected()){
					tcpSocket.close();
					Log.i(LOG_TAG, "TCP-connection Closed");
				}
			} catch (IOException e) {
				Log.e(LOG_TAG, "Error", e);
			}
		}
	
//		Intent responceBroadcastIntent = new Intent();
//		responceBroadcastIntent.setAction(GetDataReceiver.RESPONSE);
//		responceBroadcastIntent.addCategory(Intent.CATEGORY_DEFAULT);
//		responceBroadcastIntent.putExtra(RESPONSESTRING, value)
		
		
	}

}
