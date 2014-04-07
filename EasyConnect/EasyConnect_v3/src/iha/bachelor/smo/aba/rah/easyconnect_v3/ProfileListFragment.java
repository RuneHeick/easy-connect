package iha.bachelor.smo.aba.rah.easyconnect_v3;

import iha.bachelor.smo.aba.rah.easyconnect_v3.contentprovider.ProfileContentProvider;
import iha.bachelor.smo.aba.rah.easyconnect_v3.sqlite.ProfilesTable;
import android.app.ListFragment;
import android.app.LoaderManager;
import android.content.Context;
import android.content.CursorLoader;
import android.content.Intent;
import android.content.Loader;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.view.ContextMenu;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.view.ContextMenu.ContextMenuInfo;
import android.widget.ListView;
import android.widget.SimpleCursorAdapter;
import android.widget.Toast;
import android.widget.AdapterView.AdapterContextMenuInfo;

public class ProfileListFragment extends ListFragment implements LoaderManager.LoaderCallbacks<Cursor>{
	private static final int DELETE_ID = Menu.FIRST + 1;
	private SimpleCursorAdapter adapter;
	private boolean reloadNeeded;
	private String TAG = "ProfileListFragment";
	Context context = getActivity();
	
	public ProfileListFragment(){}
	
	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		View rootView = inflater.inflate(R.layout.fragment_profile_list, container, false);

		return rootView;
	}
	
	@Override
	public void onActivityCreated(Bundle savedInstanceState) {
		super.onActivityCreated(savedInstanceState);
		
		this.getListView().setDividerHeight(2);
	    fillData();
	    registerForContextMenu(getListView());
	}
	
	@Override
	public void onCreateContextMenu(ContextMenu menu, View v, ContextMenuInfo menuInfo) {
		super.onCreateContextMenu(menu, v, menuInfo);
		menu.add(0, DELETE_ID, 0, R.string.delete_profile);
	  }
	
	@Override
	public boolean onContextItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case DELETE_ID:
			Log.i("ProfileListFragment","delete Profile requested");
			//AdapterContextMenuInfo info = (AdapterContextMenuInfo) item.getMenuInfo();
			//Uri uri = Uri.parse(ProfileContentProvider.CONTENT_URI + "/" + info.id);
			//context.getContentResolver().delete(uri, null, null);
			//fillData();
			//reloadNeeded = true;
			return true;
		}
		return super.onContextItemSelected(item);
	}
		
	@Override
	public void onListItemClick(ListView l, View v, int position, long id) {
		super.onListItemClick(l, v, position, id);
		//Intent i = new Intent(this, CreateNewProfileActivity.class);
		Uri ProfileUri = Uri.parse(ProfileContentProvider.CONTENT_URI + "/" + id);
		//i.putExtra(ProfileContentProvider.CONTENT_ITEM_TYPE, ProfileUri);
		//startActivity(i);
		//finish();
		Toast.makeText(getActivity(),"onListItemClick() called.", Toast.LENGTH_LONG).show();
		
	}
	
	private void fillData() {
		// Fields from the database (projection)
		// Must include the _id column for the adapter to work
		String[] From = new String[] { ProfilesTable.COLUMN_ProfileName, ProfilesTable.COLUMN_WifiName };
		// Fields on the UI to which we map
		int[] to = new int[] { R.id.label, R.id.wifi_label };

		getActivity().getLoaderManager().initLoader(0, null, this);
		adapter = new SimpleCursorAdapter(getActivity(), R.layout.profile_row, null, From, to, 0);

		setListAdapter(adapter);
	}

	@Override
	public Loader<Cursor> onCreateLoader(int id, Bundle args) {
		String[] projection = { ProfilesTable.COLUMN_Id, 
				ProfilesTable.COLUMN_ProfileName, 
				ProfilesTable.COLUMN_ProfilePassword,
				ProfilesTable.COLUMN_WifiName,
				ProfilesTable.COLUMN_WifiPassword};
	    CursorLoader cursorLoader = new CursorLoader(getActivity(), ProfileContentProvider.CONTENT_URI, projection, null, null, null);
	    return cursorLoader;
	}

	@Override
	public void onLoadFinished(Loader<Cursor> Loader, Cursor data) {
		adapter.swapCursor(data);
	}

	@Override
	public void onLoaderReset(Loader<Cursor> arg0) {
		adapter.swapCursor(null);
		
	}
	
}