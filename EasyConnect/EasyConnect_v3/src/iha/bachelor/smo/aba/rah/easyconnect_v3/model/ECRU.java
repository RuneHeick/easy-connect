package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

import java.util.ArrayList;
import java.util.List;

import android.util.Log;

public class ECRU {
	private static final String LOG_TAG = "ECRU class";
	public String _ECRUName;
	public List<String> _moduleNameList = new ArrayList<String>();
	
	public ECRU(){
		Log.i(LOG_TAG, "BaseConstructerCalled");
	};
	
	public ECRU(String name){
		_ECRUName = name;
		Log.i(LOG_TAG,"ConstructerCalled");
	}
	
	public void InsertModuleName(String name){
		_moduleNameList.add(name);
		Log.i(LOG_TAG, "module "+ name + "added to functionlist");
	}
	
	@Override
	public String toString(){
		String temp = "\t" + _ECRUName + "\n";
		
		for (String s : _moduleNameList){
			temp += "\t\t" + s + "\n";
		}
		return temp;
	}
}