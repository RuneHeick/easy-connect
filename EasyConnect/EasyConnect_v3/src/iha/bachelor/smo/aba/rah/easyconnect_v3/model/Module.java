package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

import java.util.ArrayList;
import java.util.List;
import android.util.Log;

public class Module {
	private static final String LOG_TAG = "Module class";
	public String ModuleName;
	public List<Service> ServiceList = new ArrayList<Service>();
	
	public Module(){};
	
	public Module(String name){
		ModuleName = name;
	};
	
	public void addService(Service s){
		ServiceList.add(s);
//		Log.i(LOG_TAG, "number of elements in ServiceList: "+ ServiceList.size());
	}
	
	@Override
	public String toString(){
		String temp = "";
		Log.i(LOG_TAG, "Number of elements in ServiceList: "+ServiceList.size());
		for (int i = 0; i < ServiceList.size(); i++){
			temp += ServiceList.get(i).toString();
			Log.i(LOG_TAG, "Module iteration number: " + i);
		}
		
		return "Module: " + ModuleName + "{\n" + temp +"\n}";
	}
}