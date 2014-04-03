package iha.bachelor.smo.aba.rah.easyconnect.sqlite;

import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

public class ProfilesTable {
	// profiles table name
    public static final String TABLE_PROFILES = "profiles";
	//profiles Table Columns names
    public static final String COLUMN_Id = "_id";
	public static final String COLUMN_ProfileName = "ProfileName";
	public static final String COLUMN_ProfilePassword = "ProfilePassword";
	public static final String COLUMN_WifiName = "WifiName";
	public static final String COLUMN_WifiPassword = "WifiPassword";
	
	// Database creation SQL statement
	private static final String DATABASE_CREATE = "CREATE TABLE " +
			TABLE_PROFILES + "( " +
			COLUMN_Id + " integer primary key autoincrement, " +
			COLUMN_ProfileName + " TEXT not null, " + 
			COLUMN_ProfilePassword + " TEXT not null, " + 
			COLUMN_WifiName + " TEXT not null, " + 
			COLUMN_WifiPassword + " TEXT not null )";

	public static void onCreate(SQLiteDatabase database) {
		database.execSQL(DATABASE_CREATE);
	}

	public static void onUpgrade(SQLiteDatabase database, int oldVersion,
			int newVersion) {
		Log.w(ProfilesTable.class.getName(), "Upgrading database from version " +
				oldVersion + " to " + newVersion + 
				", which will destroy all old data");
		database.execSQL("DROP TABLE IF EXISTS " + TABLE_PROFILES);
		onCreate(database);
	}
}
