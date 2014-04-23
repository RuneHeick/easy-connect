package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

import java.util.ArrayList;
import java.util.List;

import android.util.Log;

public class FunctionList {
	private static final String LOG_TAG = "FunctionList class";
	private String _hashValue;
	private int _version;
	private List<ECRU> ECRUList = new ArrayList<ECRU>();
	
	public FunctionList(){}
	
	public FunctionList(String hash, int version){
		_hashValue = hash;
		_version = version;
	}
	
	public String getHashValue(){
		return _hashValue;
	}
	
	public int getVersion(){
		return _version;
	}
	
	public ArrayList<String> getECRUNames(){
		ArrayList<String> temp = new ArrayList<String>();
		for (ECRU e : ECRUList){
			temp.add(e._ECRUName);
		}
		return temp;
	}
	
	public ArrayList<String> getModuleNames(String s){
		ArrayList<String> temp = new ArrayList<String>();
		
		for (ECRU e : ECRUList){
			if (s.equals(e._ECRUName)){
				for (String mns : e._moduleNameList){
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
		String temp = "FunctionList version: " + _version + "\n";
		for (ECRU e : ECRUList){
			temp += e.toString();
		}
		return temp;
	}
}