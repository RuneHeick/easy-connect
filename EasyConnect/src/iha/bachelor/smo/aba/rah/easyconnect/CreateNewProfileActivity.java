package iha.bachelor.smo.aba.rah.easyconnect;

import android.os.Bundle;
import android.app.Activity;
import android.view.Menu;
import android.view.View;
import android.widget.Toast;

public class CreateNewProfileActivity extends Activity {

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_create_new_profile);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.create_new_profile, menu);
		return true;
	}
	
	public void SaveProfile(View view) {
		
		
		
		Toast.makeText(this, "To be implemented", Toast.LENGTH_SHORT).show();
	}

}
