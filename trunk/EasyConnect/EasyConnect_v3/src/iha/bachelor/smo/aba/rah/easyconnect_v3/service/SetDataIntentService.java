package iha.bachelor.smo.aba.rah.easyconnect_v3.service;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.math.BigInteger;
import java.net.InetAddress;
import java.net.Socket;
import android.app.IntentService;
import android.content.Intent;
import android.util.Log;

public class SetDataIntentService extends IntentService {
		private static final int SERVERPORT = 4543;
		private final String LOG_TAG = "SetDataIntentService";
		public SetDataIntentService(){
			super("SetDataIntentService");
		}

		@Override
		protected void onHandleIntent(Intent intent) {
			Socket tcpSocket = null;
			InputStream in = null;
			OutputStream out = null;
			try {
				Log.d(LOG_TAG, "reqDevInf: Connecting...");
				tcpSocket = new Socket(InetAddress.getByName("192.168.1.3"), SERVERPORT);
				Log.d(LOG_TAG, "reqDevInf: Connected...");

				in = tcpSocket.getInputStream();
				out = tcpSocket.getOutputStream();

				Log.d(LOG_TAG, "reqDevInf: Sending command.");
				String outMsg;
				outMsg = "SetECMData";
				out.write(outMsg.getBytes("UTF-8"));	// Sending Request ECM Data

				byte[] buf = new byte[1024];
				int availableBytes = 0;
				Thread.sleep(500);
				availableBytes = in.read(buf);		// reading if accepted

				if (availableBytes > 0){
					availableBytes = 0;
					Log.d(LOG_TAG, "SetECMDATA: Sending macAddres");
					byte[] mac = new BigInteger("E68170E5C5780023414e4452454153",16).toByteArray();
					out.write(mac, 1, 15);				// Sending MacAddress of requested device
					Log.d(LOG_TAG, "SetECMDATA: DONE");
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
		
		}


}
