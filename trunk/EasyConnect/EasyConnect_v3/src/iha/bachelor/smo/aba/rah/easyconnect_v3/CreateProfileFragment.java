package iha.bachelor.smo.aba.rah.easyconnect_v3;

import iha.bachelor.smo.aba.rah.easyconnect_v3.contentprovider.ProfileContentProvider;
import iha.bachelor.smo.aba.rah.easyconnect_v3.sqlite.ProfilesTable;
import android.app.Fragment;
import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

public class CreateProfileFragment extends Fragment{
	private WifiManager wifiManager;
	private EditText ProfileNameEditText;
	private EditText ProfilePasswordEditText;
	private EditText WifiNameEditText;
	private EditText WifiPasswordEditText;
	
	private Uri ProfileUri;
	
	public CreateProfileFragment(){}
	
	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		View rootView = inflater.inflate(R.layout.fragment_create_profile, container, false);

		//Find resources
		ProfileNameEditText = (EditText) rootView.findViewById(R.id.ProfileNameEditText);
		ProfilePasswordEditText = (EditText) rootView.findViewById(R.id.ProfilePasswordEditText);
		WifiNameEditText = (EditText) rootView.findViewById(R.id.WifiNameEditText);
		WifiPasswordEditText = (EditText) rootView.findViewById(R.id.WifiPasswordEditText);
		Button createProfileButton = (Button) rootView.findViewById(R.id.create_profile_button);
		
		//Fill wifiname with current wifi's name
		wifiManager = (WifiManager) getActivity().getSystemService(Context.WIFI_SERVICE);
		WifiInfo wifiInfo = wifiManager.getConnectionInfo();
		WifiNameEditText.setText(wifiInfo.getSSID());
		
		Bundle extras = getArguments();

	    // check from the saved Instance
		ProfileUri = (savedInstanceState == null) ? null : (Uri) savedInstanceState.getParcelable(ProfileContentProvider.CONTENT_ITEM_TYPE);

	    // Or passed from the other activity
	    if (extras != null) {
	    	String temp = extras.getString(ProfileContentProvider.CONTENT_ITEM_TYPE);
	    	ProfileUri = Uri.parse(temp);
	    	fillData(ProfileUri);
	    }
	    
	    createProfileButton.setOnClickListener(new View.OnClickListener() {
			public void onClick(View view) {
				if (TextUtils.isEmpty(ProfileNameEditText.getText().toString())
						|| TextUtils.isEmpty(ProfilePasswordEditText.getText().toString())
						|| TextUtils.isEmpty(WifiPasswordEditText.getText().toString()) ) {
					MoreDataToast();
				} else {
					saveProfile();
				}
			}
	    });	 
		
		return rootView;
	}
	
	private void fillData(Uri uri) {
		 String[] projection = { ProfilesTable.COLUMN_ProfileName, 
				 ProfilesTable.COLUMN_ProfilePassword, 
				 ProfilesTable.COLUMN_WifiName,
				 ProfilesTable.COLUMN_WifiPassword};
		 
		 Cursor cursor = getActivity().getContentResolver().query(uri, projection, null, null, null);
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

	private void saveProfile() {
		String ProfileName = ProfileNameEditText.getText().toString();
	    String ProfilePassword = ProfilePasswordEditText.getText().toString();
	    String WifiName = WifiNameEditText.getText().toString();
	    String WifiPassword = WifiPasswordEditText.getText().toString();

	    // only save if other fields then wifiname is filled out

	    if (ProfileName.length() == 0 &&
	    		ProfilePassword.length() == 0 &&
	    		WifiPassword.length() == 0) {
	    	return;
	    }

	    ContentValues values = new ContentValues();
	    values.put(ProfilesTable.COLUMN_ProfileName, ProfileName);
	    values.put(ProfilesTable.COLUMN_ProfilePassword, ProfilePassword);
	    values.put(ProfilesTable.COLUMN_WifiName, WifiName);
	    values.put(ProfilesTable.COLUMN_WifiPassword, WifiPassword);

	    if (ProfileUri == null) {
	    	// New Profile
	    	ProfileUri = getActivity().getContentResolver().insert(ProfileContentProvider.CONTENT_URI, values);
	    } else {
	    	// Update Profile
	    	getActivity().getContentResolver().update(ProfileUri, values, null, null);
	    }

	    Intent done = new Intent(this.getActivity(), LoaderActivity.class);
	    done.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
	    done.putExtra("ProfileSaved", "True");
	    startActivity(done);
	}

	private void MoreDataToast(){
		Toast.makeText(this.getActivity(), "Please fill more data" , Toast.LENGTH_LONG).show();	
	}
	
	@Override
	public String toString(){
		return "CreateProfileFragment";
	}
}