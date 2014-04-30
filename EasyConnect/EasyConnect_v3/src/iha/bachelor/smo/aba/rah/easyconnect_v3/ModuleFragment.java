package iha.bachelor.smo.aba.rah.easyconnect_v3;

import iha.bachelor.smo.aba.rah.easyconnect_v3.adapter.ExpandableListModuleAdapter;
import iha.bachelor.smo.aba.rah.easyconnect_v3.adapter.ModuleInfoParser;
import iha.bachelor.smo.aba.rah.easyconnect_v3.contentprovider.FileHandler;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.ModuleInfo;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

import android.app.Fragment;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ExpandableListView;
import android.widget.Toast;
import android.widget.ExpandableListView.OnChildClickListener;

public class ModuleFragment extends Fragment {
	private final String LOG_TAG = "ModuleFragment";
	List<String> ServiceNameList;
	Map<String, List<String>> CharacteristicsCollection;
	ExpandableListView expListView;

	
	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		View rootView = inflater.inflate(R.layout.fragment_module, container, false);
		
		Log.i(LOG_TAG, "OnCreateView called");
		/**************************/
		Bundle extras = getArguments();
		if (extras != null) {
	    	String temp = extras.getString("module");
	    	createREALCollection(temp);
	    }
		
		/**************************/
		
		expListView = (ExpandableListView) rootView.findViewById(R.id.service_list);
		final ExpandableListModuleAdapter expListAdapter = new ExpandableListModuleAdapter(getActivity(), ServiceNameList, CharacteristicsCollection);
		expListView.setAdapter(expListAdapter);
				
		return rootView;
	}
	
	private void createREALCollection(String fileName){
		ModuleInfo testMI = ModuleInfoParser.ByteToInfo(ReadFileFromSDCard("EC_CONNECT",FileHandler.NET_DIR, fileName+".BLE"));
		Log.i(LOG_TAG, testMI.toString());
		ServiceNameList = new ArrayList<String>();
		CharacteristicsCollection = new LinkedHashMap<String, List<String>>();
		
		for (String s : testMI.getServiceNames()){
			ServiceNameList.add(s);
			CharacteristicsCollection.put(s, testMI.getCharacteristicNames(s));
		
			
        }
	}
	
	public byte[] ReadFileFromSDCard(String ProfileName, String fileType, String FileName){
		FileHandler fh = new FileHandler();
		byte[] s = fh.ReadBytesFromFile(getActivity(), ProfileName, fileType, FileName);
		return s;
	}
	
}
