package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

import java.util.ArrayList;
import java.util.List;

import android.util.Log;

public class Service {
	private static final String LOG_TAG = "Service class";
	public String ServiceName;
	private List<Characteristic> CharacteristicList = new ArrayList<Characteristic>();
	
	public Service(){}
	
	public Service(String name){
		ServiceName = name;
	}
		
	public void addCharacteristic(Characteristic c){
		CharacteristicList.add(c);
//		Log.i(LOG_TAG, "Number of elements in CharacteristicList: "+ CharacteristicList.size());
	}
	
	@Override
	public String toString(){
		String temp = "";
		Log.i(LOG_TAG, "Number of elements in CharacteristicList: "+CharacteristicList.size());
		for (int i = 0; i < CharacteristicList.size(); i++){
			temp += CharacteristicList.get(i).toString();
			Log.i(LOG_TAG, "Service iteration number: " + i);
		}
		
		return "\tService: " + ServiceName + "{\n" + temp +"\n\t}\n";
	}
}