package iha.bachelor.smo.aba.rah.easyconnect_v3;

import iha.bachelor.smo.aba.rah.easyconnect_v3.contentprovider.ProfileContentProvider;
import iha.bachelor.smo.aba.rah.easyconnect_v3.sqlite.ProfilesTable;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import android.app.Activity;
import android.content.Intent;
import android.database.Cursor;
import android.util.Log;
import android.view.Menu;

public class LoaderActivity extends Activity {
	private WifiManager wifiManager;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_initial);
		

		wifiManager = (WifiManager) this.getSystemService(WIFI_SERVICE);
		//CheckConnection();
		
		Intent NetworkNotAvailable = new Intent(this, MainActivity.class);
		startActivity(NetworkNotAvailable);
	}
	
	@Override
	protected void onResume(){
		super.onResume();
		
		CheckConnection();
	}
	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.initial, menu);
		return true;
	}
	
	public void CheckConnection(){
		WifiInfo wifiInfo = wifiManager.getConnectionInfo();
		
		if (SeachProfilesForCurrentWifi(wifiInfo.getSSID())){
			Intent NetworkNotAvailable = new Intent(this, MainActivity.class);
			startActivity(NetworkNotAvailable);
		}
		else {
			//Intent NetworkAvailable = new Intent(this, CreateNewProfileActivity.class); //this line is the correct line
			Intent NetworkAvailable = new Intent(this, MainActivity.class);
			startActivity(NetworkAvailable);
		}
		
	}
	
	public boolean SeachProfilesForCurrentWifi(CharSequence CurrentWifi){
		//FIX ME - Insert code for searching the database
		Log.i("InitCheckForProfile", "Starting function");
		Log.i("InitCheckForProfile", "CurrentWifi" + CurrentWifi);
		String[] PROJECTION = { ProfilesTable.COLUMN_Id, 
				ProfilesTable.COLUMN_ProfileName, 
				ProfilesTable.COLUMN_ProfilePassword,
				ProfilesTable.COLUMN_WifiName,
				ProfilesTable.COLUMN_WifiPassword};

		String SELECTION = ProfilesTable.COLUMN_WifiName + " = '" + (String) CurrentWifi + "'";
		
		Cursor cursor = getContentResolver().query(ProfileContentProvider.CONTENT_URI,	PROJECTION, SELECTION, null, null);
		if (cursor.getCount() != 0){
			cursor.moveToFirst();
			
			Log.i("InitCheckForProfile", "profile: " + 
			cursor.getString(cursor.getColumnIndexOrThrow(ProfilesTable.COLUMN_ProfileName)) + 
			". For wifi: " + 
			cursor.getString(cursor.getColumnIndexOrThrow(ProfilesTable.COLUMN_WifiName)) + ".");
		
			
			Log.i("InitCheckForProfile", "Cursor not equal to null");
			return true;
		} else {
			Log.i("InitCheckForProfile", "Cursor equal to null");
			return false;
		}
	}
}
