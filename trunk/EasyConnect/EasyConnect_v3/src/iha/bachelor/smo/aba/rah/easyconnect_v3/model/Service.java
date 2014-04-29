package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.List;

import android.util.Log;

public class Service {
	private static final String LOG_TAG = "Service class";
	
	public short _starthandle;
	public short _endhandle;
	public short _updatehandle;
	public IHandleValue.HandleValuePair _description;
	public List<Characteristic> _characteristicList = new ArrayList<Characteristic>();
	
	public Service(){
		_description = new IHandleValue.HandleValuePair();
	}
	
	public void addCharacteristic(Characteristic c){
		_characteristicList.add(c);
//		Log.i(LOG_TAG, "Number of elements in CharacteristicList: "+ CharacteristicList.size());
	}
	
	@Override
	public String toString(){
		Log.i(LOG_TAG, "Number of elements in CharacteristicList: "+_characteristicList.size());
		
		String temp= "";
		try {
			temp = "\tService: " + new String(_description.getValue(),"UTF-8") + "{\n";
		} catch (UnsupportedEncodingException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		for (int i = 0; i < _characteristicList.size(); i++){
			temp += _characteristicList.get(i).toString();
			Log.i(LOG_TAG, "Service iteration number: " + i);
		}
		temp += "\n\t}\n";
		
		return temp;
	}
}
