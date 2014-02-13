package iha.bachelor.smo.aba.rah.easyconnect;

import android.os.Bundle;
import android.app.Activity;
import android.view.Menu;

public class ConfigureModuleActivity extends Activity {

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_configure_module);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.configure_module, menu);
		return true;
	}

}
