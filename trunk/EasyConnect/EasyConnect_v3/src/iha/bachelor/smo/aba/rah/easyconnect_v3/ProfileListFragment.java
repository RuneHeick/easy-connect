package iha.bachelor.smo.aba.rah.easyconnect_v3;

import android.app.Fragment;
import android.app.FragmentManager;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

public class ProfileListFragment extends Fragment{

	public ProfileListFragment(){}
	
	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		View rootView = inflater.inflate(R.layout.fragment_profile_list, container, false);

		return rootView;
	}
}