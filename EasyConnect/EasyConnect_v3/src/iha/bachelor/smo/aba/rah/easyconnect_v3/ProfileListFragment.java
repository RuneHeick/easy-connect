package iha.bachelor.smo.aba.rah.easyconnect_v3;

import iha.bachelor.smo.aba.rah.easyconnect_v3.contentprovider.ProfileContentProvider;
import iha.bachelor.smo.aba.rah.easyconnect_v3.sqlite.ProfilesTable;
import android.app.Fragment;
import android.app.ListFragment;
import android.app.LoaderManager;
import android.content.CursorLoader;
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
import android.widget.AdapterView.AdapterContextMenuInfo;

public class ProfileListFragment extends ListFragment implements LoaderManager.LoaderCallbacks<Cursor>{
	private static final int DELETE_ID = Menu.FIRST + 1;
	private SimpleCursorAdapter adapter;
	private String TAG = "ProfileListFragment";
	
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
			Log.i(TAG,"delete Profile requested");
			AdapterContextMenuInfo info = (AdapterContextMenuInfo) item.getMenuInfo();
			Uri uri = Uri.parse(ProfileContentProvider.CONTENT_URI + "/" + info.id);
			Log.i(TAG,"uri to delete" + uri);
			getActivity().getContentResolver().delete(uri, null, null);
			//fillData();
			//reloadNeeded = true;
			return true;
		}
		return super.onContextItemSelected(item);
	}
		
	@Override
	public void onListItemClick(ListView l, View v, int position, long id) {
		super.onListItemClick(l, v, position, id);
		//build selector URI as a string
		String ProfileUri = ProfileContentProvider.CONTENT_URI + "/" + id;
		
		//Set the URI as an argument for the next fragment
		Bundle bundle = new Bundle();
		bundle.putString(ProfileContentProvider.CONTENT_ITEM_TYPE, ProfileUri);
		Fragment CreateProfile = new CreateProfileFragment();
		CreateProfile.setArguments(bundle);
		
		//replace the fragment with new fragment
		getFragmentManager().beginTransaction().replace(R.id.frame_container, CreateProfile).commit();; 
	}
	
	private void fillData() {
		// Fields from the database (projection)
		// Must include the _id column for the adapter to work
		String[] From = new String[] { ProfilesTable.COLUMN_ProfileName, ProfilesTable.COLUMN_WifiName };
		// Fields on the UI to which we map
		int[] to = new int[] { R.id.label, R.id.wifi_label };

		getActivity().getLoaderManager().initLoader(0, null, this);
		if (adapter == null){
			adapter = new SimpleCursorAdapter(getActivity(), R.layout.profile_row, null, From, to, 0);
		}
		setListAdapter(adapter);
		Log.i(TAG, "filldata this method is done now!");
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
		Log.i(TAG+":onLoadFinished", "crasy things are going down here");
		adapter.swapCursor(data);
	}

	@Override
	public void onLoaderReset(Loader<Cursor> arg0) {
		adapter.swapCursor(null);
		
	}
	
	@Override
	public String toString(){
		return "ProfileListFragment";
	}
}