package iha.bachelor.smo.aba.rah.easyconnect_v3.service;

import iha.bachelor.smo.aba.rah.easyconnect_v3.MainActivity;
import iha.bachelor.smo.aba.rah.easyconnect_v3.adapter.ExpandableListModuleAdapter.GetDataReceiver;
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
import java.util.Arrays;

import android.app.IntentService;
import android.content.Intent;
import android.util.Log;

public class GetDataIntentService extends IntentService
{
	public static final String TARGET_ECRU = "TargetEcru";
	public static final String TARGET_MODULE = "TargetModule";
	public static final String TARGET_HANDLE = "TargetHandle";
	public static final String RESPONSE_DATA = "Response";
	public static final String PARENTPOSITION = "Parent";
	public static final String CHILDPOSITION = "Child";
	
	private static final int SERVERPORT = 4543;
	private final String LOG_TAG = "GetDataIntentService";
	public GetDataIntentService(){
		super("GetDataIntentService");
	}

	@Override
	protected void onHandleIntent(Intent intent) {
		String serialRoutingTable = FileHandler.ReadStringFromFile(this, MainActivity.CurrentProfileName, FileHandler.ROUTING_TABLE_DIR, "routingTable.txt");
		RoutingTable rt = FileHandler.DecodeGsonRoutingTable(serialRoutingTable);

		String serialECRU = intent.getStringExtra(TARGET_ECRU);
		ECRU targetEcru = ECRU.fromString(serialECRU);

		String targetModule = intent.getStringExtra(TARGET_MODULE);

		short targetHandle = intent.getShortExtra(TARGET_HANDLE, (short) 0000);
		String handleAndMac = new String(targetModule + "00" + ModuleInfoParser.byteToHex((byte)targetHandle));


		Socket tcpSocket = null;
		InputStream in = null;
		OutputStream out = null;
		byte[] ResponceData = null;
		try {
			Log.d(LOG_TAG, "Creating Socket");

			tcpSocket = new Socket(rt.GetIPFromMAC(targetEcru.mac), SERVERPORT);

			Log.d(LOG_TAG, "SocketCreated");

			in = tcpSocket.getInputStream();
			out = tcpSocket.getOutputStream();

			Log.d(LOG_TAG, "Sending command.");
			String outMsg;
			outMsg = "RequestECMData";
			out.write(outMsg.getBytes("UTF-8"));	// Sending Request ECM Data

			byte[] buf = new byte[1024];
			int availableBytes = 0;
			Thread.sleep(500);
			availableBytes = in.read(buf);		// reading if accepted

			if (availableBytes > 0){
				availableBytes = 0;
				Log.d(LOG_TAG, "Sending macAddres & Handle.");
				byte[] mac = new BigInteger(handleAndMac,16).toByteArray();
				out.write(mac, 0, 8);				// Sending MacAddress of requested device

				availableBytes = in.read(buf);	// Reading the ModuleInformation
				Log.d(LOG_TAG, "received: " + availableBytes + "bytes");
				if (availableBytes > 0){
					ResponceData = Arrays.copyOfRange(buf, 0, availableBytes);
					Log.d(LOG_TAG, "received: " + ModuleInfoParser.bytesToHex(ResponceData));
				}
			}
		} catch (Exception e) {
			Log.e(LOG_TAG, "Error", e);
		} finally {
			try {
				if ( tcpSocket != null && tcpSocket.isConnected()){
					tcpSocket.close();
					Log.d(LOG_TAG, "TCP-connection Closed");
				}
			} catch (IOException e) {
				Log.e(LOG_TAG, "Error", e);
			}
		}

		if (ResponceData != null){
			Intent responceBroadcastIntent = new Intent();
			responceBroadcastIntent.setAction(GetDataReceiver.RESPONSE);
			responceBroadcastIntent.addCategory(Intent.CATEGORY_DEFAULT);
			responceBroadcastIntent.putExtra(RESPONSE_DATA, ResponceData);
			responceBroadcastIntent.putExtra(CHILDPOSITION, intent.getIntExtra(CHILDPOSITION, -1));
			responceBroadcastIntent.putExtra(PARENTPOSITION, intent.getIntExtra(PARENTPOSITION, -1));
			sendBroadcast(responceBroadcastIntent);
		}
	}

}
