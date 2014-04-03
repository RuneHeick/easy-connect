package iha.bachelor.smo.aba.rah.easyconnect_v3;

import android.app.Fragment;
import android.app.FragmentManager;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

public class ManageProfilesFragment extends Fragment{

	public ManageProfilesFragment(){}
	
	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		View rootView = inflater.inflate(R.layout.fragment_manage_profiles, container, false);
		
		Fragment fragment = null;
		fragment = new ProfileListFragment();
		FragmentManager fragmentManager = getFragmentManager();
		fragmentManager.beginTransaction().replace(R.id.manageProfile_frame_container, fragment).commit();
		
		return rootView;
	}
}