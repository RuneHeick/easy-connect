package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

import java.net.InetAddress;
import java.util.ArrayList;
import java.util.List;

import android.util.Log;

public class RoutingTable {
	private static final String LOG_TAG = "RoutingTable";
	public List<UnitAdress> UnitAdresses;
	
	public RoutingTable(){
		UnitAdresses = new ArrayList<UnitAdress>();
	}
	
	public InetAddress GetIPFromMAC(String mac){
		InetAddress result = null;
		for (UnitAdress ua : UnitAdresses){
			if (ua._macAdress.equals(mac)){
				result = ua._currentIp;
			}
		}
		if (result != null){
			return result;
		} else {
			Log.i(LOG_TAG, "No match for that mac was found!");
			return null;
		}
	}

	public boolean isEmpty() {
		return UnitAdresses.isEmpty();
	}

	public boolean contains(UnitAdress ua) {
		boolean isPresent = UnitAdresses.contains(ua);
		if (isPresent){
			UnitAdresses.set(UnitAdresses.indexOf(ua), ua);
		}
		return isPresent; 
	}

	public void add(UnitAdress ua) {
		UnitAdresses.add(ua);
	}
}
