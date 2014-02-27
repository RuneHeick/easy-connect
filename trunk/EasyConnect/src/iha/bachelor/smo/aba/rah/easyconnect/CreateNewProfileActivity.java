package iha.bachelor.smo.aba.rah.easyconnect;

import iha.bachelor.smo.aba.rah.easyconnect.contentprovider.ProfileContentProvider;
import iha.bachelor.smo.aba.rah.easyconnect.sqlite.ProfilesTable;
import android.net.Uri;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import android.app.Activity;
import android.content.ContentValues;
import android.database.Cursor;
import android.text.TextUtils;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

public class CreateNewProfileActivity extends Activity {
	private WifiManager wifiManager;
	private EditText ProfileNameEditText;
	private EditText ProfilePasswordEditText;
	private EditText WifiNameEditText;
	private EditText WifiPasswordEditText;
	
	private Uri ProfileUri;
	
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_create_new_profile);
		
		//Find resources
		ProfileNameEditText = (EditText) findViewById(R.id.ProfileNameEditText);
		ProfilePasswordEditText = (EditText) findViewById(R.id.ProfilePasswordEditText);
		WifiNameEditText = (EditText) findViewById(R.id.WifiNameEditText);
		WifiPasswordEditText = (EditText) findViewById(R.id.WifiPasswordEditText);
		Button createProfileButton = (Button) findViewById(R.id.create_profile_button);
		
		//Fill wifiname with current wifi's name
		wifiManager = (WifiManager) this.getSystemService(WIFI_SERVICE);
		WifiInfo wifiInfo = wifiManager.getConnectionInfo();
		EditText WifiNameEditText = (EditText) findViewById(R.id.WifiNameEditText);
		WifiNameEditText.setText(wifiInfo.getSSID());
		
		Bundle extras = getIntent().getExtras();

	    // check from the saved Instance
		ProfileUri = (savedInstanceState == null) ? null : (Uri) savedInstanceState.getParcelable(ProfileContentProvider.CONTENT_ITEM_TYPE);

	    // Or passed from the other activity
	    if (extras != null) {
	    	ProfileUri = extras.getParcelable(ProfileContentProvider.CONTENT_ITEM_TYPE);

	    	fillData(ProfileUri);
	    }
	    
	    createProfileButton.setOnClickListener(new View.OnClickListener() {
			public void onClick(View view) {
				if (TextUtils.isEmpty(ProfileNameEditText.getText().toString())
						|| TextUtils.isEmpty(ProfilePasswordEditText.getText().toString())
						|| TextUtils.isEmpty(WifiPasswordEditText.getText().toString()) ) {
					MoreDataToast();
				} else {
					setResult(RESULT_OK);
			        finish();
				}
			}
	    });	    
	}

	private void fillData(Uri uri) {
		 String[] projection = { ProfilesTable.COLUMN_ProfileName, 
				 ProfilesTable.COLUMN_ProfilePassword, 
				 ProfilesTable.COLUMN_WifiName,
				 ProfilesTable.COLUMN_WifiPassword};
		 
		 Cursor cursor = getContentResolver().query(uri, projection, null, null, null);
		 if (cursor != null) {
			 cursor.moveToFirst();
			 
			 ProfileNameEditText.setText(cursor.getString(cursor.getColumnIndexOrThrow(ProfilesTable.COLUMN_ProfileName)));
			 ProfilePasswordEditText.setText(cursor.getString(cursor.getColumnIndexOrThrow(ProfilesTable.COLUMN_ProfilePassword)));
			 WifiNameEditText.setText(cursor.getString(cursor.getColumnIndexOrThrow(ProfilesTable.COLUMN_WifiName)));
			 WifiPasswordEditText.setText(cursor.getString(cursor.getColumnIndexOrThrow(ProfilesTable.COLUMN_WifiPassword)));
			 // always close the cursor
			 cursor.close();
		 }
	 }
	 
	protected void onSaveInstanceState(Bundle outState) {
		super.onSaveInstanceState(outState);
		saveState();
		outState.putParcelable(ProfileContentProvider.CONTENT_ITEM_TYPE, ProfileUri);
	}
	
	@Override
	protected void onPause() {
		super.onPause();
		saveState();
	}
	
	private void saveState() {
		String ProfileName = ProfileNameEditText.getText().toString();
	    String ProfilePassword = ProfilePasswordEditText.getText().toString();
	    String WifiName = WifiNameEditText.getText().toString();
	    String WifiPassword = WifiPasswordEditText.getText().toString();

	    // only save if other fields then wifiname is filled out

	    if (ProfileName.length() == 0 && ProfilePassword.length() == 0 && WifiPassword.length() == 0) {
	    	return;
	    }

	    ContentValues values = new ContentValues();
	    values.put(ProfilesTable.COLUMN_ProfileName, ProfileName);
	    values.put(ProfilesTable.COLUMN_ProfilePassword, ProfilePassword);
	    values.put(ProfilesTable.COLUMN_WifiName, WifiName);
	    values.put(ProfilesTable.COLUMN_WifiPassword, WifiPassword);

	    if (ProfileUri == null) {
	    	// New Profile
	    	ProfileUri = getContentResolver().insert(ProfileContentProvider.CONTENT_URI, values);
	    } else {
	    	// Update Profile
	    	getContentResolver().update(ProfileUri, values, null, null);
	    }
	  }
	
	private void MoreDataToast(){
		Toast.makeText(this, "Please fill more data" , Toast.LENGTH_LONG).show();	
	}
	
	private void makeImplementToast(){
		Toast.makeText(this, "needs to be implemented" , Toast.LENGTH_LONG).show();	
	}
	
	public void ClearProfile(View view){
		//EditText ProfileNameEditText = (EditText) findViewById(R.id.ProfileNameEditText);
		//db.deleteProfile(ProfileNameEditText.getText().toString());
		makeImplementToast();
		//db.deleteProfiles();
	}
}
