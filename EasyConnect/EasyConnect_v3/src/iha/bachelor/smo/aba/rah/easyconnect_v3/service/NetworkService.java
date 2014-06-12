package iha.bachelor.smo.aba.rah.easyconnect_v3.service;

import iha.bachelor.smo.aba.rah.easyconnect_v3.contentprovider.FileHandler;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.ECRU;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.RoutingTable;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.UnitAdress;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.math.BigInteger;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.Socket;
import java.util.Arrays;

import android.app.Service;
import android.content.Intent;
import android.os.Binder;
import android.os.IBinder;
import android.util.Log;

public class NetworkService extends Service {
	private final String LOG_TAG = "NetworkService";
	final private String ReqDevInf = "RequestDeviceInformation";
	final private String ReqDevs = "RequestDevices"; 
	private static final int SERVERPORT = 4543; 
	private DatagramSocket socket;
	private DatagramPacket packet;
	private byte[] receiveBuffer;
	private boolean UDPrunning;
	private boolean socketClosed;
	private RoutingTable routingTable;
	private InetAddress currentServerIP;
	private String MacAddress;
	private IBinder mBinder = new ServiceBinder();
	public String CurrentProfileName;

	@Override
	public void onCreate() {

		Log.i(LOG_TAG,"onCreate Called");
		UDPrunning = true;
		routingTable = new RoutingTable();

		new Thread(new UDPServer()).start(); 
		try { 
			Thread.sleep(500); 
		} catch (InterruptedException e) { } 
	}

	@Override
	public IBinder onBind(Intent arg0) {
		Log.i(LOG_TAG, "onBind Called");
		return mBinder;
	}

	public class ServiceBinder extends Binder {
		public NetworkService getService() {
			return NetworkService.this;
		}
	}

	@Override
	public int onStartCommand(Intent intent, int flags, int startId) {
		Log.i(LOG_TAG, "onStartCommand Called");
		CurrentProfileName = (String) intent.getExtras().get("CurrentProfile");
		return Service.START_REDELIVER_INTENT;
	}

	@Override
	public void onDestroy() {
		Log.i(LOG_TAG, "onDestroy Called");
		UDPrunning = false;
		while (!socketClosed){}
		super.onDestroy();
	}

	public class UDPServer implements Runnable {
		private static final String LOG_TAG = "NetworkService:UDPServer";

		@Override 
		public void run() {
			Log.i(LOG_TAG, "run() startet");
			while(UDPrunning==true){
				Log.i(LOG_TAG, "Start of WhileLoop");
				socketClosed = false;
				socket = null;
				receiveBuffer = new byte[10];
				try {
					socket = new DatagramSocket(SERVERPORT);
					packet = new DatagramPacket(receiveBuffer, receiveBuffer.length);
					socket.receive(packet);

					Log.i(LOG_TAG, "ClientAdress received: "+ packet.getAddress().toString());
					Log.i(LOG_TAG, "ClientPort received: " + packet.getPort());

					MacAddress = getMacAdress(packet);
					if (MacAddress != null){

						currentServerIP = packet.getAddress();
						UnitAdress received = new UnitAdress(MacAddress, currentServerIP);
						Log.i(LOG_TAG, "UnitAdress created!");

						if (!routingTable.contains(received)){
							routingTable.add(received);
							getDevices(received);						
							Log.i(LOG_TAG, "UnitAddress added");
							FileHandler.writeToFile(getBaseContext(), CurrentProfileName, FileHandler.ROUTING_TABLE_DIR, "routingTable.txt", FileHandler.EncodeGSoN(routingTable));
						} else {
							Log.i(LOG_TAG, "UnitAddress already there!");
						}
//						getDevices(received);
						Log.i(LOG_TAG, "end of try");
					} else {
						Log.i(LOG_TAG, "Wrong package type");
					}
				} catch (Exception e) {
					Log.i(LOG_TAG, "woups Exception Caught: "+ e.toString());
				}
				finally{
					socket.close();
					socketClosed = true;
				}
				Log.i(LOG_TAG, "completed while loop");
			}
		}
	}

	private void getDevices(UnitAdress roomUnit){
		Socket tcpSocket = null;
		BufferedReader in = null;
		BufferedWriter out = null;
		ECRU tempEcru = null;
		try {
			Log.d(LOG_TAG, "Connecting...");
			tcpSocket = new Socket(roomUnit._currentIp, SERVERPORT);
			Log.d(LOG_TAG, "Connected...");

			in = new BufferedReader(new InputStreamReader(tcpSocket.getInputStream()));

			Log.d(LOG_TAG, "Sending command.");
			out = new BufferedWriter(new OutputStreamWriter(tcpSocket.getOutputStream()));

			String outMsg;
			outMsg = ReqDevs;
			out.write(outMsg);

			out.flush();
			Log.i(LOG_TAG, "sent: " + outMsg);

			String inMsg = in.readLine() + System.getProperty("line.separator");
			inMsg = inMsg.replace("Accepted", "");

			Log.i(LOG_TAG, "received inMsg: " + inMsg);
			tempEcru = FileHandler.DecodeGsonEcru(inMsg);

			Log.i(LOG_TAG, "received ECRU: " + tempEcru.toString());
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

		FileHandler.writeToFile(this, CurrentProfileName, FileHandler.FUNCTIONS_LIST_DIR, roomUnit._macAdress + ".txt", tempEcru.toString());
		if (!tempEcru.Devices.isEmpty()){
			for (String s: tempEcru.Devices){
				getDeviceInformation(roomUnit, s);
			}
		}
	}

	public void getDeviceInformation(UnitAdress roomUnit, String deviceMacAddress){
		Socket tcpSocket = null;
		InputStream in = null;
		OutputStream out = null;
		try {
			Log.d(LOG_TAG, "reqDevInf: Connecting...");
			tcpSocket = new Socket(roomUnit._currentIp, SERVERPORT);
			Log.d(LOG_TAG, "reqDevInf: Connected...");

			in = tcpSocket.getInputStream();
			out = tcpSocket.getOutputStream();

			Log.d(LOG_TAG, "reqDevInf: Sending command.");
			String outMsg;
			outMsg = ReqDevInf;
			out.write(outMsg.getBytes("UTF-8"));	// Sending Request Device Information

			byte[] buf = new byte[1024];
			int availableBytes = 0;
			Thread.sleep(500);
			availableBytes = in.read(buf);		// reading if accepted

			if (availableBytes > 0){
				availableBytes = 0;
				Log.d(LOG_TAG, "reqDevInf: Sending macAddres.");
				byte[] mac = new BigInteger(deviceMacAddress,16).toByteArray();
				out.write(mac, 1, 6);				// Sending MacAddress of requested device

				availableBytes = in.read(buf);	// Reading the ModuleInformation
				if (availableBytes > 0){
					byte[] deviceInformation = Arrays.copyOfRange(buf, 0, availableBytes);
					Log.i(LOG_TAG, "Writing ECM to sd-card");
					FileHandler.writeToFile(this, CurrentProfileName, FileHandler.MODULE_DIR, deviceMacAddress + ".BLE", deviceInformation);
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
	}

	public String getMacAdress(DatagramPacket dataPacket){
		Log.i(LOG_TAG, "getting Mac Address");
		final char[] hexArray = "0123456789ABCDEF".toCharArray();
		byte[] buf = dataPacket.getData();

		if ( buf[0] == 1){
			char[] hexChars = new char[12];
			for ( int j = 0; j < 6; j++ ) {
				int v = buf[j+1] & 0xFF;
				hexChars[j * 2] = hexArray[v >>> 4];
				hexChars[j * 2 + 1] = hexArray[v & 0x0F];
			}
			String MacAddress = new String(hexChars);
			Log.i(LOG_TAG, "Mac Address: " + MacAddress);

			return MacAddress;
		} else {
			return null;
		}
	}
}