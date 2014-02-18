package iha.bachelor.smo.aba.rah.easyconnect.model;

public class ProfileModel {
	public ProfileModel() {
		_profileName = "";
		_profilePassword = "";
		_wifiName = "";
		_wifiPassword = "";
	}
	
	public ProfileModel(String ProfileName, String ProfilePassword, String WifiName, String WifiPassword ) {
		_profileName = ProfileName;
		_profilePassword = ProfilePassword;
		_wifiName = WifiName;
		_wifiPassword = WifiPassword;
	}
	
	public String _profileName;
	public String _profilePassword;
	public String _wifiName;
	public String _wifiPassword;
}
