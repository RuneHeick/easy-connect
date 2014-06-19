package iha.bachelor.smo.aba.rah.easyconnect_v3;

import iha.bachelor.smo.aba.rah.easyconnect_v3.adapter.ExpandableListAdapter;
import iha.bachelor.smo.aba.rah.easyconnect_v3.contentprovider.FileHandler;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.ECRU;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.RoutingTable;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.UnitAdress;

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

public class ModuleListFragment extends Fragment{
	private final String LOG_TAG = "ModuleListFragment";
	
	ExpandableListView expListView;
	List<ECRU> EcruList;
	Map<ECRU, List<String>> EcruDeviceCollection;

	public ModuleListFragment(){}

	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		View rootView = inflater.inflate(R.layout.fragment_module_list, container, false);

		if (savedInstanceState != null){
			return rootView;
		}
		createREALCollection();

		expListView = (ExpandableListView) rootView.findViewById(R.id.function_list);
		final ExpandableListAdapter expListAdapter = new ExpandableListAdapter(getActivity(), EcruList, EcruDeviceCollection);
		expListView.setAdapter(expListAdapter);

		expListView.setOnChildClickListener(new OnChildClickListener() {
			public boolean onChildClick(ExpandableListView parent, View v, int groupPosition, int childPosition, long id) {
				final String selected = (String) expListAdapter.getChild(groupPosition, childPosition);
				Toast.makeText(getActivity().getBaseContext(), selected, Toast.LENGTH_LONG).show();



				Bundle bundle = new Bundle();
				bundle.putString("module", selected);
				bundle.putString("ECRU", EcruList.get(groupPosition).toString());
				Fragment ModuleFragment = new ModuleFragment();
				ModuleFragment.setArguments(bundle);
				getFragmentManager().beginTransaction().replace(R.id.frame_container, ModuleFragment).commit();;
				return true;
			}
		});

		Log.i(LOG_TAG, "FileWrite complete");

		return rootView;
	}

	public void writeFileToSDCard(String ProfileName, String fileType, String fileName, String GSoNString){
		FileHandler.writeToFile(getActivity(), ProfileName, fileType, fileName, GSoNString);
	}

	private void createREALCollection(){
		try{
			RoutingTable rt =  FileHandler.DecodeGsonRoutingTable(
					FileHandler.ReadStringFromFile(
							getActivity(),
							MainActivity.CurrentProfileName,
							"routingTable",
							"routingTable.txt"));

			EcruList = new ArrayList<ECRU>();
			EcruDeviceCollection = new LinkedHashMap<ECRU, List<String>>();

			for (UnitAdress ua : rt.UnitAdresses ){
				String serialiseRoomUnit = FileHandler.ReadStringFromFile(getActivity(), MainActivity.CurrentProfileName, FileHandler.FUNCTIONS_LIST_DIR, ua._macAdress+ ".txt");
				ECRU tempEcru = FileHandler.DecodeGsonEcru(serialiseRoomUnit);
				EcruList.add(tempEcru);
				EcruDeviceCollection.put(tempEcru, tempEcru.Devices);
			}
		}
		catch (Exception e){
			Toast.makeText(getActivity().getBaseContext(), "No resources available", Toast.LENGTH_LONG).show();
		}
	}
}
