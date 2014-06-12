package iha.bachelor.smo.aba.rah.easyconnect_v3.adapter;

import iha.bachelor.smo.aba.rah.easyconnect_v3.model.Characteristic;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.ModuleInfo;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.Service;

import java.util.Arrays;

import android.util.Log;

public class ModuleInfoParser {
	private static final String LOG_TAG = "ModuleInfoParser";
	private static Service tempService = null;
	private static Characteristic tempCharacteristic = null;

	public static ModuleInfo ByteToInfo(byte[] data)
    {
		ModuleInfo file = new ModuleInfo();
        try
        {
            int i = 0;
            while (i < data.length)
            {
                byte Command = data[i];
                int len = data[i + 1] + 2;
                switch (Command)
                {
                    case (byte) 0xA0:
                        file.Address = Arrays.copyOfRange(data, i+2, i+len);
                        break;
//                    case (byte) 0xA1:
//                        file.TimeHandel = (short)((data[i + 2] << 8) + data[i + 3]);
//                        file.PassCodeHandel = (short)((data[i + 4] << 8) + data[i + 5]);
//                        break;
                    case (byte) 0x10:
                        file.Manufacturer.handle = (short)(((short)(data[i + 2]) << 8) + ((short)data[i + 3]));
                    	file.Manufacturer.Value = Arrays.copyOfRange(data, i+4, i+len);
                        break;
                    case (byte) 0x11:
                        file.Model.handle = (short)(((short)(data[i + 2]) << 8) + ((short)data[i + 3]));
                        file.Model.Value = Arrays.copyOfRange(data, i+4, i+len);
                        break;
                    case (byte) 0x12:
                        file.Name.handle = (short)((data[i + 2] << 8) + data[i + 3]);
                        file.Name.Value = Arrays.copyOfRange(data, i+4, i+len);
                        break;
                    case (byte) 0x13:
                        file.Serial.handle = (short)((data[i + 2] << 8) + data[i + 3]);
                        file.Serial.Value = Arrays.copyOfRange(data, i+4, i+len);
                        break;
                    case (byte) 0x20:
                    	if (tempService == null){
                    		tempService = new Service();
                    	} else {
                    		file.addService(tempService);
                    		tempService = new Service();
                    	}
                        tempService._updatehandle = (short)((data[i + 6] << 8) + data[i + 7]);
                        break;
                    case (byte) 0x14:
                    	tempService._description.handle = (short)((data[i + 2] << 8) + data[i + 3]);
                    	tempService._description.Value = Arrays.copyOfRange(data, i+4, i+len); 
                        break;
                    case (byte) 0xC0:
                    	if (tempCharacteristic == null){
                    		tempCharacteristic = new Characteristic();
                    	} else {
                    		tempService.addCharacteristic(tempCharacteristic);
                    		tempCharacteristic = new Characteristic();
                    	}
                        tempCharacteristic._value.handle =(short)((data[i + 2] << 8) + data[i + 3]);
                        tempCharacteristic._value.ReadWriteProps = data[i + 4];
                        break;
                    case (byte) 0xC1:
                        tempCharacteristic._description.handle = (short)((data[i + 2] << 8) + data[i + 3]);
                        tempCharacteristic._description.Value = Arrays.copyOfRange(data, i+4, i+len);
                        break;
                    case (byte) 0xC2:
                        tempCharacteristic._format.handle = (short)((data[i + 2] << 8) + data[i + 3]);
                        tempCharacteristic._format.Value = Arrays.copyOfRange(data, i+4, i+len);
                        break;
                    case (byte) 0xC3:
                        tempCharacteristic._gUIFormat.handle = (short)((data[i + 2] << 8) + data[i + 3]);
                        tempCharacteristic._gUIFormat.Value = Arrays.copyOfRange(data, i+4, i+len);
                        break;
                    case (byte) 0xC4:
                        tempCharacteristic._range.handle = (short)((data[i + 2] << 8) + data[i + 3]);
                        tempCharacteristic._range.Value = Arrays.copyOfRange(data, i+4, i+len);
                        break;
                    case (byte) 0xC5:
                        tempCharacteristic._subscription.handle = (short)((data[i + 2] << 8) + data[i + 3]);
                        tempCharacteristic._subscription.Value = Arrays.copyOfRange(data, i+4, i+len);                        
                        break;
                    default:
                    	break;
                }

                i += len;
            }
        } catch(Exception e){
            Log.e(LOG_TAG,"Exception caught: "+ e.toString());
            return null;
        }
        tempService.addCharacteristic(tempCharacteristic);
        tempCharacteristic = null;
        
        file.addService(tempService);
        tempService = null;
        
        return file;
    }
	
	public static String bytesToHex(byte[] bytes) {
		final char[] hexArray = "0123456789ABCDEF".toCharArray();
	    char[] hexChars = new char[bytes.length * 2];
	    for ( int j = 0; j < bytes.length; j++ ) {
	        int v = bytes[j] & 0xFF;
	        hexChars[j * 2] = hexArray[v >>> 4];
	        hexChars[j * 2 + 1] = hexArray[v & 0x0F];
	    }
	    return new String(hexChars);
	}
	
	public static String byteToHex(byte bytes) {
		final char[] hexArray = "0123456789ABCDEF".toCharArray();
	    char[] hexChars = new char[2];
        int v = bytes & 0xFF;
        hexChars[0] = hexArray[v >>> 4];
        hexChars[1] = hexArray[v & 0x0F];
	    return new String(hexChars);
	}
}
