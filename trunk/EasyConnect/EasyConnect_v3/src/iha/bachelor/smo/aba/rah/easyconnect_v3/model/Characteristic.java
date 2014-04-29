package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

import iha.bachelor.smo.aba.rah.easyconnect_v3.adapter.ModuleInfoParser;

import java.io.UnsupportedEncodingException;

import android.util.Log;

public class Characteristic {
	private static final String LOG_TAG = "Characteristic class";
	
	public IHandleValue.CharacteristicValueHandle _value;
	public IHandleValue.HandleValuePair _description;
	public IHandleValue.HandleValuePair _format;
	public IHandleValue.HandleValuePair _gUIFormat;
	public IHandleValue.HandleValuePair _range;
	public IHandleValue.HandleValuePair _subscription;
	
	public Characteristic(){
		_value = new IHandleValue.CharacteristicValueHandle();
		_description = new IHandleValue.HandleValuePair();
		_format = new IHandleValue.HandleValuePair();
		_gUIFormat = new IHandleValue.HandleValuePair();
		_range = new IHandleValue.HandleValuePair();
		_subscription = new IHandleValue.HandleValuePair();
	}
	
	@Override
	public String toString(){
		Log.i(LOG_TAG, "Characteristic.toString() called");
		String temp = "";
		try {
			 temp = "\t\tCharacteristic: " + new String(_description.getValue(),"UTF-8") + "\n" +
					"\t\tformat: " + ModuleInfoParser.bytesToHex(_format.getValue()) + "\n" +
					"\t\tguiFormat: " + ModuleInfoParser.bytesToHex(_gUIFormat.getValue()) + "\n" +
					"\t\trange: " + ModuleInfoParser.bytesToHex(_range.getValue()) + "\n" +
					"\t\tsubs: " + ModuleInfoParser.bytesToHex(_subscription.getValue()) + "\n\n";
		} catch (UnsupportedEncodingException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		return temp;
	} 
}