package iha.bachelor.smo.aba.rah.easyconnect;

import iha.bachelor.smo.aba.rah.easyconnect.contentprovider.ProfileContentProvider;
import iha.bachelor.smo.aba.rah.easyconnect.sqlite.ProfilesTable;
import android.net.Uri;
import android.os.Bundle;
import android.app.ListActivity;
import android.app.LoaderManager;
import android.content.CursorLoader;
import android.content.Intent;
import android.content.Loader;
import android.database.Cursor;
import android.widget.AdapterView.AdapterContextMenuInfo;
import android.widget.ListView;
import android.widget.SimpleCursorAdapter;
import android.util.Log;
import android.view.ContextMenu;
import android.view.ContextMenu.ContextMenuInfo;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;

public class EditProfileActivity extends ListActivity implements LoaderManager.LoaderCallbacks<Cursor> {
	private static final int DELETE_ID = Menu.FIRST + 1;
	private SimpleCursorAdapter adapter;
	private boolean reloadNeeded;
	private String TAG = "EditProfileActivity";
	  
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_edit_profile);
		
		this.getListView().setDividerHeight(2);
	    fillData();
	    registerForContextMenu(getListView());
	    Log.d(TAG,"onCreate");
        Log.d(TAG+ "ReloadNeeded",String.valueOf(reloadNeeded));
        
	    reloadNeeded = false;
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.edit_profile, menu);
		return true;
	}

	// Reaction to the menu selection
	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case R.id.CreateProfileItem:
			CreateProfile();
			return true;
		}
		return super.onOptionsItemSelected(item);
	}
	
	@Override
	public boolean onContextItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case DELETE_ID:
			AdapterContextMenuInfo info = (AdapterContextMenuInfo) item.getMenuInfo();
			Uri uri = Uri.parse(ProfileContentProvider.CONTENT_URI + "/" + info.id);
			getContentResolver().delete(uri, null, null);
			fillData();
			reloadNeeded = true;
			return true;
		}
		return super.onContextItemSelected(item);
	}
	
	private void CreateProfile() {
		Intent i = new Intent(this, CreateNewProfileActivity.class);
	    startActivity(i);
	    finish();
	}
	
	// Opens the second activity if an entry is clicked
	@Override
	protected void onListItemClick(ListView l, View v, int position, long id) {
		super.onListItemClick(l, v, position, id);
		Intent i = new Intent(this, CreateNewProfileActivity.class);
		Uri ProfileUri = Uri.parse(ProfileContentProvider.CONTENT_URI + "/" + id);
		i.putExtra(ProfileContentProvider.CONTENT_ITEM_TYPE, ProfileUri);
		startActivity(i);
		finish();
	}
	
	private void fillData() {
		// Fields from the database (projection)
		// Must include the _id column for the adapter to work
		String[] From = new String[] { ProfilesTable.COLUMN_ProfileName, ProfilesTable.COLUMN_WifiName };
		// Fields on the UI to which we map
		int[] to = new int[] { R.id.label, R.id.wifi_label };

		getLoaderManager().initLoader(0, null, this);
		adapter = new SimpleCursorAdapter(this, R.layout.profile_row, null, From, to, 0);

		setListAdapter(adapter);
	}
	
	@Override
	public void onCreateContextMenu(ContextMenu menu, View v, ContextMenuInfo menuInfo) {
		super.onCreateContextMenu(menu, v, menuInfo);
		menu.add(0, DELETE_ID, 0, R.string.delete_profile);
	  }
	
	@Override
	public Loader<Cursor> onCreateLoader(int id, Bundle args) {
		String[] projection = { ProfilesTable.COLUMN_Id, 
				ProfilesTable.COLUMN_ProfileName, 
				ProfilesTable.COLUMN_ProfilePassword,
				ProfilesTable.COLUMN_WifiName,
				ProfilesTable.COLUMN_WifiPassword};
	    CursorLoader cursorLoader = new CursorLoader(this, ProfileContentProvider.CONTENT_URI, projection, null, null, null);
	    return cursorLoader;
	}

	@Override
	public void onLoadFinished(Loader<Cursor> Loader, Cursor data) {
		adapter.swapCursor(data);
	}

	@Override
	public void onLoaderReset(Loader<Cursor> Loader) {
		adapter.swapCursor(null);
	}
	
	@Override
	public void onBackPressed(){
		if (reloadNeeded){
			Intent i = new Intent(this, InitialActivity.class);
			i.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
			startActivity(i);
		} else{
			Intent i = new Intent(this, SettingsActivity.class);
			startActivity(i);
		}
	}
	   // TEST ON SAVE
    protected void onPause() {
        Log.d(TAG,"onPause");
        Log.d(TAG+ "ReloadNeeded",String.valueOf(reloadNeeded));
        super.onPause();
    }
    protected void onStart() {
        Log.d(TAG,"onStart");
        Log.d(TAG+ "ReloadNeeded",String.valueOf(reloadNeeded));
        super.onStart();
    }
    
    protected void onResume() {
    	 // Check if a new Profile has been made
	    if (!reloadNeeded){
	    	Bundle extras = getIntent().getExtras(); 
	    	if (extras != null) {
	    		String value = extras.getString("ProfileSaved", "False");
	    		if ( value.equals("False")){
	    			reloadNeeded = false;
	    		}else {
	    			reloadNeeded = true;
	    		}
	    	}
	    }
	    
        Log.d(TAG,"onResume");
        Log.d(TAG+ "ReloadNeeded",String.valueOf(reloadNeeded));
        super.onResume();
    }
    protected void onStop() {
        Log.d(TAG,"onStop");
        Log.d(TAG+ "ReloadNeeded",String.valueOf(reloadNeeded));
        super.onStop();
    }
}
