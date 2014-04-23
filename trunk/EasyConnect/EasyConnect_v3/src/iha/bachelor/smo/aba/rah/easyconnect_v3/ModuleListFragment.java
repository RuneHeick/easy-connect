package iha.bachelor.smo.aba.rah.easyconnect_v3;

import iha.bachelor.smo.aba.rah.easyconnect_v3.adapter.ExpandableListAdapter;
import iha.bachelor.smo.aba.rah.easyconnect_v3.contentprovider.FileHandler;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.Characteristic;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.ECRU;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.FunctionList;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.Module;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.Service;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

import com.google.gson.Gson;

import android.app.Fragment;
import android.os.Bundle;
import android.util.DisplayMetrics;
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
		
//		CreateDummyFunctionList();
		createREALCollection();
		
		expListView = (ExpandableListView) rootView.findViewById(R.id.function_list);
		final ExpandableListAdapter expListAdapter = new ExpandableListAdapter(getActivity(), groupList, roomUnitCollection);
		expListView.setAdapter(expListAdapter);
		 
		expListView.setOnChildClickListener(new OnChildClickListener() {
			public boolean onChildClick(ExpandableListView parent, View v, int groupPosition, int childPosition, long id) {
				final String selected = (String) expListAdapter.getChild(groupPosition, childPosition);
				Toast.makeText(getActivity().getBaseContext(), selected, Toast.LENGTH_LONG).show();
				return true;
			}
		});
		
		Log.i(LOG_TAG, "FileWrite complete");
		
        return rootView;
    }

	public FunctionList CreateDummyFunctionList(){
		FunctionList fl = new FunctionList("hashash", 1);
		for (int i = 0; i < 4;i++){
			fl.addECRU(createDummyECRU("ECRU" + i));
		}
		writeFileToSDCard(FileHandler.FUNCTIONS_LIST_DIR,"EC_CONNECT","FunctionList.txt", EncodeGSoN(fl));
		return fl;
	}
	
	public ECRU createDummyECRU(String s){
		ECRU e = new ECRU(s);
		for (int i=0;i<5;i++){
			Module temp = createDummyModule(s+i);
			e.InsertModuleName(temp.ModuleName);
			writeFileToSDCard(FileHandler.MODULE_DIR, "EC_CONNECT", temp.ModuleName, EncodeGSoN(temp));
		}
		return e;
	}
	
	public String EncodeGSoN(Object m){
		Gson gson = new Gson();
		return gson.toJson(m);
	}
	
	public FunctionList DecodeGSoNFunctionList(String s){
		Gson gson = new Gson();
		return gson.fromJson(s, FunctionList.class);
	}
	
	public Module DecodeGSoNModule(String s){
		Gson gson = new Gson();
		return gson.fromJson(s, Module.class);
	}
	
	public Module createDummyModule(String s) {
		Characteristic cha1 = new Characteristic("UserDescription1"+s, "GeneralPurposeFile1"+s, "CharacteristicPurposeFile1"+s, "ValueRange1"+s, "SubSCriptionOptions1"+s);
		Characteristic cha2 = new Characteristic("UserDescription2"+s, "GeneralPurposeFile2"+s, "CharacteristicPurposeFile2"+s, "ValueRange2"+s, "SubSCriptionOptions2"+s);
		Characteristic cha3 = new Characteristic("UserDescription3"+s, "GeneralPurposeFile3"+s, "CharacteristicPurposeFile3"+s, "ValueRange3"+s, "SubSCriptionOptions3"+s);
		Characteristic cha4 = new Characteristic("UserDescription4"+s, "GeneralPurposeFile4"+s, "CharacteristicPurposeFile4"+s, "ValueRange4"+s, "SubSCriptionOptions4"+s);
		Service serv1 = new Service("OddService"+s);
		serv1.addCharacteristic(cha1);
		serv1.addCharacteristic(cha3);
		Service serv2 = new Service("EvenService2"+s);
		serv2.addCharacteristic(cha2);
		serv2.addCharacteristic(cha4);
		Module mod = new Module("Module"+s);
		mod.addService(serv1);
		mod.addService(serv2);
		return mod;
	}
	
	public void writeFileToSDCard(String fileType,String ProfileName, String fileName, String GSoNString){
		FileHandler fh = new FileHandler();
		fh.writeToFile(getActivity(), ProfileName, fileType, fileName, GSoNString);
	}
	
	public String ReadFileFromSDCard(String ProfileName, String fileType, String FileName){
		FileHandler fh = new FileHandler();
		String s = fh.ReadFromFile(getActivity(), ProfileName, fileType, FileName);
		return s;
	}

	private void createREALCollection(){
        FunctionList testFL = DecodeGSoNFunctionList(
				ReadFileFromSDCard(
						"EC_CONNECT",
						FileHandler.FUNCTIONS_LIST_DIR,
						"FunctionList.txt"));
        groupList = new ArrayList<String>();
        roomUnitCollection = new LinkedHashMap<String, List<String>>();

        for (String s : testFL.getECRUNames()){
        	groupList.add(s);
            roomUnitCollection.put(s, testFL.getModuleNames(s));
        }
	}
}