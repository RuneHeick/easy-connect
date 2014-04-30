package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

import iha.bachelor.smo.aba.rah.easyconnect_v3.adapter.ModuleInfoParser;

import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.List;

import android.util.Log;

public class ModuleInfo {
	private static final String LOG_TAG = "Module class";
	
	public byte[] Address;
	public IHandleValue.HandleValuePair Name;
	public IHandleValue.HandleValuePair Model;
	public IHandleValue.HandleValuePair Serial;
	public IHandleValue.HandleValuePair Manufacturer;
	public List<Service> ServiceList = new ArrayList<Service>();
	
	public ModuleInfo(){
		Address = new byte[6];
		Name = new IHandleValue.HandleValuePair();
		Model = new IHandleValue.HandleValuePair();
		Serial = new IHandleValue.HandleValuePair();
		Manufacturer = new IHandleValue.HandleValuePair();
	};
	
	public ArrayList<String> getServiceNames(){
		ArrayList<String> temp = new ArrayList<String>();
		for (Service e : ServiceList){
			try {
				temp.add(new String(e._description.getValue(),"UTF-8"));
			} catch (UnsupportedEncodingException e1) {
				// TODO Auto-generated catch block
				e1.printStackTrace();
			}
		}
		return temp;
	}
	
	public ArrayList<String> getCharacteristicNames(String s){
		ArrayList<String> temp = new ArrayList<String>();
		
		for (Service e : ServiceList){
			try {
				if (s.equals(new String(e._description.getValue(),"UTF-8"))){
					for (Characteristic c : e._characteristicList){
						temp.add(new String(c._description.getValue(),"UTF-8"));
					}
					return temp;
				}
			} catch (UnsupportedEncodingException e1) {
				// TODO Auto-generated catch block
				e1.printStackTrace();
			}
		}
		return temp;
	}

	public void addService(Service s){
		ServiceList.add(s);
//		Log.i(LOG_TAG, "number of elements in ServiceList: "+ ServiceList.size());
	}
	
	public Characteristic getCharacteristic(String serviceName, String moduleName){
		Characteristic temp = new Characteristic();
		for (Service s : ServiceList){
			try {
				if (serviceName.equals(new String(s._description.getValue(),"UTF-8"))){
					for (Characteristic c : s._characteristicList){
						temp = c;
					}
				}
			} catch (UnsupportedEncodingException e1) {
				// TODO Auto-generated catch block
				e1.printStackTrace();
			}
		}
		return temp;
	}
	
	@Override
	public String toString(){
		Log.i(LOG_TAG, "Number of elements in ServiceList: "+ServiceList.size());
		String temp= "";
		try {
			temp += "Module: " + new String(Name.getValue(),"UTF-8") + ". Address: "+ ModuleInfoParser.bytesToHex(Address) +"{\n";
		} catch (UnsupportedEncodingException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		for (int i = 0; i < ServiceList.size(); i++){
			temp += ServiceList.get(i).toString();
			Log.i(LOG_TAG, "Module iteration number: " + i);
		}
		temp += "\n}";
		return temp;
	}
}