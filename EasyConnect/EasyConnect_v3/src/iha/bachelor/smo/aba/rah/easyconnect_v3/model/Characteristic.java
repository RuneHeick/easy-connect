package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

import android.util.Log;

public class Characteristic {
	private static final String LOG_TAG = "Characteristic class";
	public String UserDescription;
	public String GuiPresentationFormat;
	public String CharacteristicPresentationFormat;
	public String ValidRange;
	public String SubscriptionOption;
	
	public Characteristic(){}
	
	public Characteristic(String uD, String gPF, String cPF, String vR, String sO) {
		UserDescription = uD;
		GuiPresentationFormat = gPF;
		CharacteristicPresentationFormat = cPF;
		ValidRange = vR;
		SubscriptionOption = sO;
	}
	
	@Override
	public String toString(){
		Log.i(LOG_TAG, "Characteristic.toString() called");
		return "\t\tCharacteristic: " + UserDescription + "\n" +
				"\t\t" + GuiPresentationFormat + "\n" +
				"\t\t" + CharacteristicPresentationFormat + "\n" +
				"\t\t" + ValidRange + "\n" +
				"\t\t" + SubscriptionOption + "\n\n";
	} 
}