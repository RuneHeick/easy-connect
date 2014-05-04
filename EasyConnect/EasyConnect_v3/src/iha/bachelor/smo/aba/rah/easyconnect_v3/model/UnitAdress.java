package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

import java.net.InetAddress;

import android.text.format.Time;

public class UnitAdress {
	public String _macAdress;
	public InetAddress _currentIp;
	public Time _lastSeen;
	
	public UnitAdress(String macAdress, InetAddress currentIp) {
		_lastSeen = new Time();
		_macAdress = macAdress;
		_currentIp = currentIp;
		this.wasSeen();
	}
	
	public void wasSeen(){
		_lastSeen.setToNow();
	}
	
	public boolean SeenWithinThreeMinutes(){
		Time temp = new Time();
		temp.setToNow();
		temp.minute -= 3;
		temp.normalize(true);
		if (Time.compare(_lastSeen,temp) >= 0){
			return true;
		} else {
			return false;
		}
	}
	
	public boolean SeenWithinThreeSeconds(){
		Time temp = new Time();
		temp.setToNow();
		temp.second -= 3;
		temp.normalize(true);
		if (Time.compare(_lastSeen,temp) >= 0){
			return true;
		} else {
			return false;
		}
	}

	@Override
	public boolean equals(Object object){
		boolean isEqual= false;
		if (object != null && object instanceof UnitAdress) {
			isEqual = (this._macAdress.equals(((UnitAdress) object)._macAdress));
		}
		return isEqual;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result
				+ ((_macAdress == null) ? 0 : _macAdress.hashCode());
		return result;
	}
}