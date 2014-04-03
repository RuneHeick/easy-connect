package iha.bachelor.smo.aba.rah.easyconnect_v3;

import android.app.Fragment;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

public class AddModuleFragment extends Fragment{

	public AddModuleFragment(){}
	
	@Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState) {
  
        View rootView = inflater.inflate(R.layout.fragment_add_module, container, false);
          
        return rootView;
    }
	
}