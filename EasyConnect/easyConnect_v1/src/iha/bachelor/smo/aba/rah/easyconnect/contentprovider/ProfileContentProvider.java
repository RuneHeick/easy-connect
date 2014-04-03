package iha.bachelor.smo.aba.rah.easyconnect.contentprovider;

import java.util.Arrays;
import java.util.HashSet;
import android.content.ContentProvider;
import android.content.ContentResolver;
import android.content.ContentValues;
import android.content.UriMatcher;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteQueryBuilder;
import android.net.Uri;
import android.text.TextUtils;
import android.util.Log;
import iha.bachelor.smo.aba.rah.easyconnect.sqlite.ProfilesTable;
import iha.bachelor.smo.aba.rah.easyconnect.sqlite.SQLiteHelper;

public class ProfileContentProvider extends ContentProvider {
	private SQLiteHelper db;
	
	// used for the UriMacher
	private static final int PROFILES = 10;
	private static final int PROFILE_NAME = 20;

	private static final String AUTHORITY = "iha.bachelor.smo.aba.rah.easyconnect.contentprovider";

	private static final String BASE_PATH = "Profiles";
	public static final Uri CONTENT_URI = Uri.parse("content://" + AUTHORITY + "/" + BASE_PATH);

	public static final String CONTENT_TYPE = ContentResolver.CURSOR_DIR_BASE_TYPE + "/profiles";
	public static final String CONTENT_ITEM_TYPE = ContentResolver.CURSOR_ITEM_BASE_TYPE + "/profile";

	private static final UriMatcher sURIMatcher = new UriMatcher(UriMatcher.NO_MATCH);
	static {
		sURIMatcher.addURI(AUTHORITY, BASE_PATH, PROFILES);
		sURIMatcher.addURI(AUTHORITY, BASE_PATH + "/#", PROFILE_NAME);
	}
	
	@Override
	public boolean onCreate() {
		db = new SQLiteHelper(getContext());
		return false;
	}
	
	@Override
	public Cursor query(Uri uri, String[] projection, String selection, String[] selectionArgs, String sortOrder) {
		Log.i("ProfileContentProvider.Query", "Starting function");
	    // Using SQLiteQueryBuilder instead of query() method
	    SQLiteQueryBuilder queryBuilder = new SQLiteQueryBuilder();

	    // check if the caller has requested a column which does not exists
	    checkColumns(projection);
	    Log.i("ProfileContentProvider.Query", "Checked Columns");
	    // Set the table
	    queryBuilder.setTables(ProfilesTable.TABLE_PROFILES);

	    int uriType = sURIMatcher.match(uri);
	    switch (uriType) {
	    case PROFILES:
	    	Log.i("ProfileContentProvider.Query", "URI match Profiles");
	      break;
	    case PROFILE_NAME:
	    	Log.i("ProfileContentProvider.Query", "URI match Profile_name");
	      // adding the ID to the original query
	      queryBuilder.appendWhere(ProfilesTable.COLUMN_Id + "=" + uri.getLastPathSegment());
	      break;
	    default:
	      throw new IllegalArgumentException("Unknown URI: " + uri);
	    }

	    SQLiteDatabase database = db.getWritableDatabase();
	    Cursor cursor = queryBuilder.query(database, projection, selection, selectionArgs, null, null, sortOrder);
	    Log.i("ProfileContentProvider.Query", "Called to DB");
	    // make sure that potential listeners are getting notified
	    cursor.setNotificationUri(getContext().getContentResolver(), uri);

	    return cursor;
	}
	
	@Override
	public String getType(Uri arg0) {
		return null;
	}
	
	@Override
	public Uri insert(Uri uri, ContentValues values) {
		int uriType = sURIMatcher.match(uri);
	    SQLiteDatabase sqlDB = db.getWritableDatabase();
	    long id = 0;
	    switch (uriType) {
	    case PROFILES:
	      id = sqlDB.insert(ProfilesTable.TABLE_PROFILES, null, values);
	      break;
	    default:
	      throw new IllegalArgumentException("Unknown URI: " + uri);
	    }
	    getContext().getContentResolver().notifyChange(uri, null);
	    return Uri.parse(BASE_PATH + "/" + id);
	}
	
	@Override
	public int delete(Uri uri, String selection, String[] selectionArgs) {
	    int uriType = sURIMatcher.match(uri);
	    SQLiteDatabase sqlDB = db.getWritableDatabase();
	    int rowsDeleted = 0;
	    switch (uriType) {
	    case PROFILES:
	      rowsDeleted = sqlDB.delete(ProfilesTable.TABLE_PROFILES, selection, selectionArgs);
	      break;
	    case PROFILE_NAME:
	      String id = uri.getLastPathSegment();
	      if (TextUtils.isEmpty(selection)) {
	        rowsDeleted = sqlDB.delete(ProfilesTable.TABLE_PROFILES, ProfilesTable.COLUMN_Id + "=" + id, null);
	      } else {
	        rowsDeleted = sqlDB.delete(ProfilesTable.TABLE_PROFILES, ProfilesTable.COLUMN_Id + "=" + id + " and " + selection, selectionArgs);
	      }
	      break;
	    default:
	      throw new IllegalArgumentException("Unknown URI: " + uri);
	    }
	    getContext().getContentResolver().notifyChange(uri, null);
	    return rowsDeleted;
	}

	@Override
	public int update(Uri uri, ContentValues values, String selection, String[] selectionArgs) {

	    int uriType = sURIMatcher.match(uri);
	    SQLiteDatabase sqlDB = db.getWritableDatabase();
	    int rowsUpdated = 0;
	    switch (uriType) {
	    case PROFILES:
	    	rowsUpdated = sqlDB.update(ProfilesTable.TABLE_PROFILES,
	    			values, 
	    			selection,
	    			selectionArgs);
	      break;
	    case PROFILE_NAME:
	    	String id = uri.getLastPathSegment();
	      if (TextUtils.isEmpty(selection)) {
	    	  rowsUpdated = sqlDB.update(ProfilesTable.TABLE_PROFILES, 
	        		values,
	        		ProfilesTable.COLUMN_Id + "=" + id, 
	        		null);
	      } else {
	    	  rowsUpdated = sqlDB.update(ProfilesTable.TABLE_PROFILES, 
	        		values,
	        		ProfilesTable.COLUMN_Id + "=" + id + " and " + selection,
	        		selectionArgs);
	      }
	      break;
	    default:
	    	throw new IllegalArgumentException("Unknown URI: " + uri);
	    }
	    getContext().getContentResolver().notifyChange(uri, null);
	    return rowsUpdated;
	}

	private void checkColumns(String[] projection) {
		String[] available = { ProfilesTable.COLUMN_Id,
				ProfilesTable.COLUMN_ProfileName,
	    		ProfilesTable.COLUMN_ProfilePassword, 
	    		ProfilesTable.COLUMN_WifiName,
	    		ProfilesTable.COLUMN_WifiPassword };
	    if (projection != null) {
	    	HashSet<String> requestedColumns = new HashSet<String>(Arrays.asList(projection));
	      HashSet<String> availableColumns = new HashSet<String>(Arrays.asList(available));
	      // check if all columns which are requested are available
	      if (!availableColumns.containsAll(requestedColumns)) {
	    	  throw new IllegalArgumentException("Unknown columns in projection");
	      }
	    }
	}
}
