package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

import java.util.ArrayList;
import java.util.List;

import android.util.Log;

public class FunctionList {
	private static final String LOG_TAG = "FunctionList class";
	private String roomUnitMacAddress;
	private List<ECRU> ECRUList = new ArrayList<ECRU>();
	
	public FunctionList(){}
	
	public FunctionList(String eCRUmacAdress){
		roomUnitMacAddress = eCRUmacAdress;
	}
	
	public String getHashValue(){
		return roomUnitMacAddress;
	}
	
	public ArrayList<String> getECRUMacs(){
		ArrayList<String> temp = new ArrayList<String>();
		for (ECRU e : ECRUList){
			temp.add(e.mac);
		}
		return temp;
	}
	
	public ArrayList<String> getModuleNames(String s){
		ArrayList<String> temp = new ArrayList<String>();
		
		for (ECRU e : ECRUList){
			if (s.equals(e.mac)){
				for (String mns : e.Devices){
					temp.add(mns);
				}
			}
		}
		return temp;
	}
	
	public void addECRU(ECRU roomUnit){
		ECRUList.add(roomUnit);
		Log.i(LOG_TAG, "ECRU "+ roomUnit + "added to ECRUList");
	}
	
	@Override
	public String toString(){
		String temp = "FunctionList: " + roomUnitMacAddress + "\n";
		for (ECRU e : ECRUList){
			temp += e.toString();
		}
		return temp;
	}
}