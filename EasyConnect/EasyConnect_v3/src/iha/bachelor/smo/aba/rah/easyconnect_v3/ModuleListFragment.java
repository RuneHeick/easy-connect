package iha.bachelor.smo.aba.rah.easyconnect_v3;

import android.app.Fragment;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

public class ModuleListFragment extends Fragment{

	public ModuleListFragment(){}
	
	@Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState) {
  
        View rootView = inflater.inflate(R.layout.fragment_module_list, container, false);
          
        return rootView;
    }
	
	@Override
	public String toString(){
		return "ModuleListFragment";
	}
}
