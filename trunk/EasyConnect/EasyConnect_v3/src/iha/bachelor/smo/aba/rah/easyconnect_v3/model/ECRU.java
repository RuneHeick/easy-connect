package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

import java.util.ArrayList;
import java.util.List;

import com.google.gson.Gson;

import android.util.Log;

public class ECRU {
	private static final String LOG_TAG = "ECRU class";
	public String mac;
	public String Name;
	public List<String> Devices = new ArrayList<String>();
	
	public ECRU(String Mac){
		mac = Mac;
		Log.i(LOG_TAG,"ConstructerCalled");
	}
	
	public void insertModuleMac(String mac){
		Devices.add(mac);
		Log.i(LOG_TAG, "module :"+ mac + ". added to functionlist");
	}
	
	@Override
	public String toString(){
		Gson gson = new Gson();
		return gson.toJson(this);
	}
}