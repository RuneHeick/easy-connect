package iha.bachelor.smo.aba.rah.easyconnect.sqlite;

import android.content.Context;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;

public class SQLiteHelper  extends SQLiteOpenHelper{
	
	// Database Version and Name
    private static final int DATABASE_VERSION = 4;
    private static final String DATABASE_NAME = "EasyConnect_DB";

	public SQLiteHelper(Context context) {
        super(context, DATABASE_NAME, null, DATABASE_VERSION);  
	}

	@Override
	public void onCreate(SQLiteDatabase db) {
		ProfilesTable.onCreate(db);
	}

	@Override
	public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
        ProfilesTable.onCreate(db);
	}
	
}
