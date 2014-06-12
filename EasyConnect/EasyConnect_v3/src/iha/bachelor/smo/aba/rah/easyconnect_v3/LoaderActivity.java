package iha.bachelor.smo.aba.rah.easyconnect_v3;

import iha.bachelor.smo.aba.rah.easyconnect_v3.contentprovider.ProfileContentProvider;
import iha.bachelor.smo.aba.rah.easyconnect_v3.service.NetworkService;
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
	private final String LOG_TAG = "Loader Activity";
	private WifiManager wifiManager;
	
	public static final int CREATE_PROFILE = 10;
	public static final int MODULE_LIST = 20;
	public static final int PROFILE_LIST = 30;
	private String foundProfile;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_initial);
	}
	
	@Override
	protected void onResume(){
		super.onResume();
		wifiManager = (WifiManager) this.getSystemService(WIFI_SERVICE);
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
			Intent NetworkAvailable = new Intent(this, MainActivity.class);
			NetworkAvailable.putExtra("CurrentProfile", foundProfile);
			
			Bundle extras = getIntent().getExtras();
			if (extras == null) {
				NetworkAvailable.putExtra("TargetFragment", MODULE_LIST);
			}else {
				NetworkAvailable.putExtra("TargetFragment", PROFILE_LIST);
			}
			
			
			startActivity(NetworkAvailable);
		}
		else {
			Intent NetworkNotAvailable = new Intent(this, MainActivity.class);
			NetworkNotAvailable.putExtra("TargetFragment", CREATE_PROFILE);
			startActivity(NetworkNotAvailable);
		}
		
	}
	
	public boolean SeachProfilesForCurrentWifi(CharSequence CurrentWifi){
		Log.i(LOG_TAG +"InitCheckForProfile", "Starting function");
		Log.i(LOG_TAG +"InitCheckForProfile", "CurrentWifi: " + CurrentWifi);
		String[] PROJECTION = { ProfilesTable.COLUMN_Id,
				ProfilesTable.COLUMN_ProfileName,
				ProfilesTable.COLUMN_ProfilePassword,
				ProfilesTable.COLUMN_WifiName,
				ProfilesTable.COLUMN_WifiPassword};

		String SELECTION = ProfilesTable.COLUMN_WifiName + " = '" + (String) CurrentWifi + "'";
		
		Cursor cursor = getContentResolver().query(ProfileContentProvider.CONTENT_URI,	PROJECTION, SELECTION, null, null);
		if (cursor.getCount() != 0){
			cursor.moveToFirst();
			foundProfile = cursor.getString(cursor.getColumnIndexOrThrow(ProfilesTable.COLUMN_ProfileName));
			
			Log.i(LOG_TAG, "Starting Service!");
			Intent networkServiceIntent = new Intent(this, NetworkService.class);
			networkServiceIntent.putExtra("CurrentProfile", foundProfile);
			startService(networkServiceIntent);
			
			Log.i(LOG_TAG +"InitCheckForProfile", "profile: " + 
			cursor.getString(cursor.getColumnIndexOrThrow(ProfilesTable.COLUMN_ProfileName)) + 
			". For wifi: " + 
			cursor.getString(cursor.getColumnIndexOrThrow(ProfilesTable.COLUMN_WifiName)) + ".");
		
			
			Log.i(LOG_TAG + "InitCheckForProfile", "Cursor not equal to null");
			return true;
		} else {
			Log.i(LOG_TAG +"InitCheckForProfile", "Cursor equal to null");
			return false;
		}
	}
}
