package iha.bachelor.smo.aba.rah.easyconnect_v3.contentprovider;

import iha.bachelor.smo.aba.rah.easyconnect_v3.model.ECRU;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.FunctionList;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.IOException;
import java.io.PrintWriter;

import com.google.gson.Gson;

import android.content.Context;
import android.os.Environment;
import android.util.Log;

public class FileHandler {
	private final static String LOG_TAG = "FileHandler";
	public final static String MODULE_DIR = "modules";
	public final static String FUNCTIONS_LIST_DIR = "functionsList";
	public final static String NET_DIR = "net";
	
	public FileHandler(){
		
	}
	
	public void deleteFile(Context context, String CurrentProfile, String FileType, String FileName){
		File temp = getStorageDir(context, CurrentProfile, FileType);
		File myFile = new File(temp,FileName);
		myFile.delete();		
	}
	
	public void LogFileNames(Context context, String CurrentProfile, String DirType){
		String path = getStorageDir(context, CurrentProfile, DirType).toString();
		Log.i("Files", "Path: " + path);
		File f = new File(path);        
		File file[] = f.listFiles();
		Log.i("Files", "Size: "+ file.length);
		for (int i=0; i < file.length; i++)
		{
		    Log.e("Files", "FileName:" + file[i].getName());
		}
	}
	
	public static void writeToFile(Context context,String CurrentProfile, String DirType, String FileName, byte[] bytes){
		File StorageDir = getStorageDir(context, CurrentProfile, DirType);
		File myFile = new File(StorageDir, FileName);
		
		try {
	        FileOutputStream f = new FileOutputStream(myFile);
	        f.write(bytes);
//	        PrintWriter pw = new PrintWriter(f);
//	        pw.print(bytes);
//	        pw.flush();
//	        pw.close();
	        f.close();
	    } catch (Exception e) {
	        e.printStackTrace();
	        Log.i(LOG_TAG, "******* File not found. Did you add a WRITE_EXTERNAL_STORAGE permission to the manifest?");
	    }
	}
	
	public static void writeToFile(Context context,String CurrentProfile, String DirType, String FileName, String Text){
		File StorageDir = getStorageDir(context, CurrentProfile, DirType);
		File myFile = new File(StorageDir, FileName);
		
		try {
	        FileOutputStream f = new FileOutputStream(myFile);
	        PrintWriter pw = new PrintWriter(f);
	        pw.println(Text);
	        pw.flush();
	        pw.close();
	        f.close();
	    } catch (Exception e) {
	        e.printStackTrace();
	        Log.i(LOG_TAG, "******* File not found. Did you add a WRITE_EXTERNAL_STORAGE permission to the manifest?");
	    }
	}
	
	public static String ReadStringFromFile(Context context,String CurrentProfile, String DirType, String FileName){
		File StorageDir = getStorageDir(context, CurrentProfile, DirType);
		File myFile = new File(StorageDir, FileName);
		
		StringBuilder text = new StringBuilder();

		try {
		    BufferedReader br = new BufferedReader(new FileReader(myFile));
		    String line;

		    while ((line = br.readLine()) != null) {
		        text.append(line);
		        text.append('\n');
		    }
		    br.close();
		    Log.i(LOG_TAG, "Read Completed!");
		}
		catch (Exception e) {
			Log.e(LOG_TAG, "Read FAILED!!!");
			return null;
		}
		return text.toString();
	}
	
	public static byte[] ReadBytesFromFile(Context context,String CurrentProfile, String DirType, String FileName){
		File StorageDir = getStorageDir(context, CurrentProfile, DirType);
		File file = new File(StorageDir, FileName);
	    int size = (int) file.length();
	    Log.i(LOG_TAG,"Size of file: " + size);
	    
	    byte[] bytes = new byte[size];
	    try {
	        BufferedInputStream buf = new BufferedInputStream(new FileInputStream(file));
	        buf.read(bytes, 0, bytes.length);
	        buf.close();
	    } catch (FileNotFoundException e) {
	        e.printStackTrace();
	    } catch (IOException e) {
	        e.printStackTrace();
	    }
	    Log.i(LOG_TAG,"Size of bytes: " + bytes.length);
	    
	    
		return bytes;
	}
	
	public static File getStorageDir(Context context,String CurrentProfile, String dir) {
	    // Get the directory for the app's private pictures directory. 
		if (isExternalStorageWritable()  && isExternalStorageReadable()){
			File file = new File(context.getExternalFilesDir(CurrentProfile), dir);
			Log.i(LOG_TAG, "\nExternal file system: "+ file);
			if (!file.mkdirs()) {
				Log.e(LOG_TAG + ": getModuleStorageDir", "Directory \"\\"+ dir +"\" not created");
			} else{
				Log.i(LOG_TAG + ": getModuleStorageDir", "Directory \"\\"+ dir +"\" was created");
			}
			return file;
		}
		return null;
	}
	
	/* Checks if external storage is available for read and write */
	public static boolean isExternalStorageWritable() {
	    String state = Environment.getExternalStorageState();
	    if (Environment.MEDIA_MOUNTED.equals(state)) {
	    	Log.i(LOG_TAG + ": isExternalStorageWritable", "External storage is writeable");
	        return true;
	    }
	    Log.e(LOG_TAG + ": isExternalStorageWritable", "External storage is NOT writeable");
	    return false;
	}

	/* Checks if external storage is available to at least read */
	public static boolean isExternalStorageReadable() {
	    String state = Environment.getExternalStorageState();
	    if (Environment.MEDIA_MOUNTED.equals(state) ||
	        Environment.MEDIA_MOUNTED_READ_ONLY.equals(state)) {
	    	Log.i(LOG_TAG + ": isExternalStorageReadable", "External storage is readable");
	        return true;
	    }
	    Log.e(LOG_TAG + ": isExternalStorageReadable", "External storage is NOT readable");
	    return false;
	}
	
	public static String EncodeGSoN(Object m){
		Gson gson = new Gson();
		return gson.toJson(m);
	}
	
	public static FunctionList DecodeGSoNFunctionList(String s){
		Gson gson = new Gson();
		return gson.fromJson(s, FunctionList.class);
	}
	
	public static ECRU DecodeGsonEcru(String s){
		Gson gson = new Gson();
		return gson.fromJson(s, ECRU.class);
	}
}