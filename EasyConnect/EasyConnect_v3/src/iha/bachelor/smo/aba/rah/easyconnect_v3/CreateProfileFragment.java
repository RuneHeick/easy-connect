package iha.bachelor.smo.aba.rah.easyconnect_v3;

import android.app.Fragment;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

public class CreateProfileFragment extends Fragment{

	public CreateProfileFragment(){}
	
	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		View rootView = inflater.inflate(R.layout.fragment_create_profile, container, false);

		return rootView;
	}
}