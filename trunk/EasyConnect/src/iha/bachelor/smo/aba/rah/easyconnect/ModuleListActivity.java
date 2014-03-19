package iha.bachelor.smo.aba.rah.easyconnect;

import android.os.Bundle;
import android.app.Activity;
import android.content.Intent;
import android.view.Menu;
import android.view.MenuItem;

public class ModuleListActivity extends Activity {

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_module_list);
						
	}

	private void goToSettingsActivity(){
    	Intent goToSettings = new Intent(this, SettingsActivity.class);
		startActivity(goToSettings);
	}
	
	private void goToConfigModuleActivity(){
    	Intent goToConfigModule = new Intent(this, ConfigureModuleActivity.class);
		startActivity(goToConfigModule);
	}
	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.module_list, menu);
		return true;
	}
	
	// Reaction to the menu selection
	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case R.id.SettingsItem:
			goToSettingsActivity();
			return true;
		case R.id.ConfigureModuleItem:
			goToConfigModuleActivity();
			return true;
			
		}
		return super.onOptionsItemSelected(item);
	}

}
