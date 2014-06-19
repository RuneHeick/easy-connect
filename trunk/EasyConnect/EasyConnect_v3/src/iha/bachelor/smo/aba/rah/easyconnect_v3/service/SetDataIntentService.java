package iha.bachelor.smo.aba.rah.easyconnect_v3.service;

import iha.bachelor.smo.aba.rah.easyconnect_v3.MainActivity;
import iha.bachelor.smo.aba.rah.easyconnect_v3.adapter.ExpandableListModuleAdapter.TypeEnum;
import iha.bachelor.smo.aba.rah.easyconnect_v3.adapter.ModuleInfoParser;
import iha.bachelor.smo.aba.rah.easyconnect_v3.contentprovider.FileHandler;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.ECRU;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.RoutingTable;

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
		private static final String LOG_TAG = "SetDataIntentService";
		public static final String TARGET_MODULE = "TrgtMdl";
		public static final String TARGET_ECRU = "TrgtEcru";
		public static final String TARGET_HANDLE = "TrgtHndl";
		public static final String VALUE = "Value";
		public static final String DATA_TYPE = "Data type";
		
		public SetDataIntentService(){
			super("SetDataIntentService");
		}

		@Override
		protected void onHandleIntent(Intent intent) {
			String serialRoutingTable = FileHandler.ReadStringFromFile(this, MainActivity.CurrentProfileName, FileHandler.ROUTING_TABLE_DIR, "routingTable.txt");
			RoutingTable rt = FileHandler.DecodeGsonRoutingTable(serialRoutingTable);

			String serialECRU = intent.getStringExtra(TARGET_ECRU);
			ECRU targetEcru = ECRU.fromString(serialECRU);

			String targetModule = intent.getStringExtra(TARGET_MODULE);

			short targetHandle = intent.getShortExtra(TARGET_HANDLE, (short) 0);
			
			int data_type = intent.getIntExtra(DATA_TYPE, 0);
			
			byte[] data = intent.getByteArrayExtra(VALUE);
			
			String handleAndMac = null;
			switch (data_type){
			case TypeEnum.Bool:
				break;
			case TypeEnum.UINT8:
				handleAndMac = new String(targetModule + "00" + ModuleInfoParser.byteToHex((byte)targetHandle)+ ModuleInfoParser.bytesToHex(data));	
				break;
			case TypeEnum.UTFString:
				break;
			default:
				break;
			}
			int dataLenght = handleAndMac.length()/2;
			// TODO: Parse Data Correctly.
			
			
			

			
			
			Socket tcpSocket = null;
			InputStream in = null;
			OutputStream out = null;
			try {
				Log.d(LOG_TAG, "Creating Socket");
				tcpSocket = new Socket(rt.GetIPFromMAC(targetEcru.mac), SERVERPORT);
				Log.d(LOG_TAG, "SocketCreated");

				in = tcpSocket.getInputStream();
				out = tcpSocket.getOutputStream();
				
				String outMsg;
				outMsg = "SetECMData";
				Log.d(LOG_TAG, "Sending command: " + outMsg );
				out.write(outMsg.getBytes("UTF-8"));	// Sending Request ECM Data

				byte[] buf = new byte[1024];
				int availableBytes = 0;
				Thread.sleep(500);
				availableBytes = in.read(buf);		// reading if accepted

				if (availableBytes > 0){
					availableBytes = 0;
					Log.d(LOG_TAG, "SetECMDATA: Sending macAddres");
					//byte[] mac = new BigInteger("E68170E5C5780023414e4452454153",16).toByteArray();
					byte[] mac = new BigInteger(handleAndMac,16).toByteArray();
					out.write(mac, 0, dataLenght);				// Sending MacAddress of requested device
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
