package iha.bachelor.smo.aba.rah.easyconnect_v3;

import iha.bachelor.smo.aba.rah.easyconnect_v3.adapter.ExpandableListAdapter;
import iha.bachelor.smo.aba.rah.easyconnect_v3.contentprovider.FileHandler;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.ECRU;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.FunctionList;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

import com.google.gson.Gson;

import android.app.Fragment;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ExpandableListView;
import android.widget.Toast;
import android.widget.ExpandableListView.OnChildClickListener;

public class ModuleListFragment extends Fragment{
	private final String LOG_TAG = "ModuleListFragment";
	List<String> groupList;
	Map<String, List<String>> roomUnitCollection;
	ExpandableListView expListView;
	
	public ModuleListFragment(){}
	
	@Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		View rootView = inflater.inflate(R.layout.fragment_module_list, container, false);
	
		if (savedInstanceState != null){
			return rootView;
		}
//		createTestFunctionList();
		createREALCollection();
		
		expListView = (ExpandableListView) rootView.findViewById(R.id.function_list);
		final ExpandableListAdapter expListAdapter = new ExpandableListAdapter(getActivity(), groupList, roomUnitCollection);
		expListView.setAdapter(expListAdapter);
		 
		expListView.setOnChildClickListener(new OnChildClickListener() {
			public boolean onChildClick(ExpandableListView parent, View v, int groupPosition, int childPosition, long id) {
				final String selected = (String) expListAdapter.getChild(groupPosition, childPosition);
				Toast.makeText(getActivity().getBaseContext(), selected, Toast.LENGTH_LONG).show();
				
				
				
				Bundle bundle = new Bundle();
				bundle.putString("module", selected);
				Fragment ModuleFragment = new ModuleFragment();
				ModuleFragment.setArguments(bundle);
				getFragmentManager().beginTransaction().replace(R.id.frame_container, ModuleFragment).commit();;
				return true;
			}
		});
		
		Log.i(LOG_TAG, "FileWrite complete");
		
        return rootView;
    }

	public FunctionList createTestFunctionList(){
		FunctionList fl = new FunctionList("FunctionList");
		ECRU ru = new ECRU("A4B627FE2F7A");
		ru.InsertModuleName("E68170E5C578");
		ru.InsertModuleName("F83A228CBA1C");
		fl.addECRU(ru);
		writeFileToSDCard("EC_CONNECT", FileHandler.NET_DIR, "FunctionList.txt", EncodeGSoN(fl));
		return null;
	}
		
	public String EncodeGSoN(Object m){
		Gson gson = new Gson();
		return gson.toJson(m);
	}
	
	public FunctionList DecodeGSoNFunctionList(String s){
		Gson gson = new Gson();
		return gson.fromJson(s, FunctionList.class);
	}
	
	public void writeFileToSDCard(String ProfileName, String fileType, String fileName, String GSoNString){
		FileHandler fh = new FileHandler();
		fh.writeToFile(getActivity(), ProfileName, fileType, fileName, GSoNString);
	}
	
	public String ReadFileFromSDCard(String ProfileName, String fileType, String FileName){
		FileHandler fh = new FileHandler();
		String s = fh.ReadStringFromFile(getActivity(), ProfileName, fileType, FileName);
		return s;
	}

	private void createREALCollection(){
        FunctionList testFL = DecodeGSoNFunctionList(
        		ReadFileFromSDCard("EC_CONNECT",FileHandler.NET_DIR,"FunctionList.txt"));
        groupList = new ArrayList<String>();
        roomUnitCollection = new LinkedHashMap<String, List<String>>();

        for (String s : testFL.getECRUNames()){
        	groupList.add(s);
            roomUnitCollection.put(s, testFL.getModuleNames(s));
        }
	}
}