package iha.bachelor.smo.aba.rah.easyconnect;

import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import android.app.Activity;
import android.content.Intent;
import android.view.Menu;

public class InitialActivity extends Activity {
private WifiManager wifiManager;


	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_initial);

		wifiManager = (WifiManager) this.getSystemService(WIFI_SERVICE);
		CheckConnection();
	}
	
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
			Intent NetworkNotAvailable = new Intent(this, ModuleListActivity.class);
			startActivity(NetworkNotAvailable);
		}
		else {
			Intent NetworkAvailable = new Intent(this, CreateNewProfileActivity.class);
			startActivity(NetworkAvailable);
		}
		
	}
	
	public boolean SeachProfilesForCurrentWifi(CharSequence CurrentWifi){
		//FIX ME - Insert code for searching the database
		CharSequence temp = "\"WiredSSID\"!";
		return CurrentWifi.equals(temp) ? true : false;

	}
}
