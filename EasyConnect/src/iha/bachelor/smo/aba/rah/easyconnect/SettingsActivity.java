package iha.bachelor.smo.aba.rah.easyconnect;

import android.os.Bundle;
import android.app.Activity;
import android.content.Intent;
import android.view.Menu;
import android.view.View;
import android.widget.Button;

public class SettingsActivity extends Activity {

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_settings);
		
		
		Button EditProfilesButton = (Button) findViewById(R.id.manageProfilesButton);
		EditProfilesButton.setOnClickListener(new View.OnClickListener() {
			public void onClick(View view) {
				goToEditProfilesActivity();
			}
	    });	 
		
		Button RoomUnitSetupButton = (Button) findViewById(R.id.roomUnitSetupButton);
		RoomUnitSetupButton.setOnClickListener(new View.OnClickListener() {
			public void onClick(View view) {
				goToSetupRoomUnitActivity();
			}
	    });	 
		
		
	}

	public void goToEditProfilesActivity(){
		Intent manageProfiles = new Intent(this, EditProfileActivity.class);
	    startActivity(manageProfiles);
	}
	
	public void goToSetupRoomUnitActivity(){
		Intent SetupECRU = new Intent(this, RoomUnitSetupActivity.class);
	    startActivity(SetupECRU);
	}
	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.settings, menu);
		return true;
	}

}
