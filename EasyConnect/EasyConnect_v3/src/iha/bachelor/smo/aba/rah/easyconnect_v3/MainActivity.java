package iha.bachelor.smo.aba.rah.easyconnect_v3;

import iha.bachelor.smo.aba.rah.easyconnect_v3.adapter.NavDrawerListAdapter;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.NavDrawerItem;

import java.util.ArrayList;

import android.os.Bundle;
import android.app.Activity;
import android.app.Fragment;
import android.app.FragmentManager;
import android.content.Intent;
import android.content.res.Configuration;
import android.content.res.TypedArray;
import android.support.v4.app.ActionBarDrawerToggle;
import android.support.v4.widget.DrawerLayout;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ListView;

public class MainActivity extends Activity {
	private static final String TAG = "MainActivity"; 

	private Fragment fragment;
	
	private DrawerLayout mDrawerLayout;
	private ListView mDrawerList;
	private ActionBarDrawerToggle mDrawerToggle;
	
	// nav drawer title
	private CharSequence mDrawerTitle;
	
	// used to store app title
	private CharSequence mTitle;
	
	// slide menu items
	private String[] navMenuTitles;
	private TypedArray navMenuIcons;
	
	private ArrayList<NavDrawerItem> navDrawerItems;
	private NavDrawerListAdapter adapter;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		Log.i(TAG, "made it to OnCreate()");
		
		// code to select current fragment
		Bundle extras = getIntent().getExtras();
		if (extras != null) {
			int tgtFragment = extras.getInt("TargetFragment");
			switch (tgtFragment){
				case 0:
					fragment = new ModuleListFragment();
					break;
				case LoaderActivity.CREATE_PROFILE:
					fragment = new CreateProfileFragment();
					break;
				case LoaderActivity.MODULE_LIST:
					fragment = new ModuleListFragment();
					break;
				case LoaderActivity.PROFILE_LIST:
					fragment = new ProfileListFragment();
					break;
				default:
					fragment = new ModuleListFragment();
					break;
			}
		}
		displayView();
		
		mTitle = mDrawerTitle = getTitle();
		
		// load slide menu items
		navMenuTitles = getResources().getStringArray(R.array.nav_drawer_items);
		
		// nav drawer icons from resources
		navMenuIcons = getResources().obtainTypedArray(R.array.nav_drawer_icons);
		
		mDrawerLayout = (DrawerLayout) findViewById(R.id.drawer_layout);
		mDrawerList = (ListView) findViewById(R.id.list_slidermenu);
		//populating the navigation drawerlist 	    
		navDrawerItems = new ArrayList<NavDrawerItem>();
		
		int i = 0;
		for (String s : navMenuTitles)
		{
			navDrawerItems.add(new NavDrawerItem(s, navMenuIcons.getResourceId(i, -1)));
			i++;
		}
		
		// Recycle the typed array
		navMenuIcons.recycle();
		
		// setting the nav drawer list adapter
		adapter = new NavDrawerListAdapter(getApplicationContext(),navDrawerItems);
		mDrawerList.setAdapter(adapter);
		
		// enabling action bar app icon and behaving it as toggle button
		getActionBar().setDisplayHomeAsUpEnabled(true);
		getActionBar().setHomeButtonEnabled(true);
		
		mDrawerToggle = new ActionBarDrawerToggle(this, mDrawerLayout,
				R.drawable.ic_launcher, //nav menu toggle icon
				R.string.app_name, // nav drawer open - description for accessibility
				R.string.app_name // nav drawer close - description for accessibility
				){
					public void onDrawerClosed(View view) {
						getActionBar().setTitle(mTitle);
						// calling onPrepareOptionsMenu() to show action bar icons
						invalidateOptionsMenu();
					}
					
					public void onDrawerOpened(View drawerView) {
						getActionBar().setTitle(mDrawerTitle);
						// calling onPrepareOptionsMenu() to hide action bar icons
						invalidateOptionsMenu();
					}
				};
		mDrawerLayout.setDrawerListener(mDrawerToggle);
		
		if (savedInstanceState == null) {
			// on first time display view for first nav item
			// displayView(0);
		}
		mDrawerList.setOnItemClickListener(new SlideMenuClickListener());
	}

	/**
	 * Slide menu item click listener
	 * */
	private class SlideMenuClickListener implements ListView.OnItemClickListener {
		@Override
		public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
			// display view for selected nav drawer item
			selectView(position);
		}
	}
	
	/**
	 * Diplaying fragment view for selected nav drawer list item
	 * */
	private void selectView(int position) {
		Log.d(TAG, "made it to displayView");
		// update the main content by replacing fragments
		switch (position) {
			case 0:
				fragment = new ModuleListFragment();
				break;
			case 1:
				ShowProfileListFragment();
				break;
			case 2:
				fragment = new SetupRoomUnitFragment();
				break;
			case 3:
				fragment = new AddModuleFragment();
				break;
			default:
				break;
		}
		
		// update selected item and title, then close the drawer
		mDrawerList.setItemChecked(position, true);
		mDrawerList.setSelection(position);
		setTitle(navMenuTitles[position]);
		mDrawerLayout.closeDrawer(mDrawerList);
		
		
		displayView();
	}
	
	private void displayView() {
		
		if (fragment != null) {
			FragmentManager fragmentManager = getFragmentManager();
			fragmentManager.beginTransaction().replace(R.id.frame_container, fragment).commit();
			Log.e(TAG, "Current fragment: "+fragment.toString());
		} else {
			// error in creating fragment
			Log.e(TAG, "Error in creating fragment");
		}
		invalidateOptionsMenu();
	}
	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}
	
	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		// toggle nav drawer on selecting action bar app icon/title
		if (mDrawerToggle.onOptionsItemSelected(item)) {
			return true;
		}
		
		// Handle action bar actions click
		switch (item.getItemId()) {
			case R.id.create_profile_menu_item:
				fragment = new CreateProfileFragment();
				displayView();
				Log.i(TAG,"You pressed Create Profile");
				return true;
			default:
				return super.onOptionsItemSelected(item);
		}
	}
	
	/***
	 * Called when invalidateOptionsMenu() is triggered
	 */
	@Override
	public boolean onPrepareOptionsMenu(Menu menu) {
		Log.i(TAG,"onPrepareOptionsMenu");
		//only show "create profile menuitem, when in ProfileListFragment
		if (fragment.getClass() == ProfileListFragment.class){
			Log.i(TAG, "Showing menu item");
			menu.findItem(R.id.create_profile_menu_item).setVisible(true);
		} else {
			Log.i(TAG, "Hiding menu item");
			menu.findItem(R.id.create_profile_menu_item).setVisible(false);
		}

		return super.onPrepareOptionsMenu(menu);	
	}
	
	@Override
	public void setTitle(CharSequence title) {
		mTitle = title;
		getActionBar().setTitle(mTitle);
	}
	
	/**
	 * When using the ActionBarDrawerToggle, you must call it during
	 * onPostCreate() and onConfigurationChanged()...
	 */
	
	@Override
	protected void onPostCreate(Bundle savedInstanceState) {
		super.onPostCreate(savedInstanceState);
		// Sync the toggle state after onRestoreInstanceState has occurred.
		mDrawerToggle.syncState();
	}
	
	@Override
	public void onConfigurationChanged(Configuration newConfig) {
		super.onConfigurationChanged(newConfig);
		// Pass any configuration change to the drawer toggls
		mDrawerToggle.onConfigurationChanged(newConfig);
	}
	
	@Override
	public void onBackPressed(){
		FragmentManager fragmentManager = getFragmentManager();
		Fragment temp = (Fragment) fragmentManager.findFragmentById(R.id.frame_container);
		if (temp.toString() == "CreateProfileFragment"){
			ShowProfileListFragment();
		} else if (temp.toString() == "ModuleListFragment"){
			Log.i("onBackPressed", "ModuleListFragment");
		} else {
			fragment = new ModuleListFragment();
		}
		displayView();
	}
	
	public void ShowProfileListFragment(){
		Intent done = new Intent(this, MainActivity.class);
		done.putExtra("TargetFragment", LoaderActivity.PROFILE_LIST);
	    startActivity(done);
	}
}