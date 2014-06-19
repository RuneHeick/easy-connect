package iha.bachelor.smo.aba.rah.easyconnect_v3.adapter;

import iha.bachelor.smo.aba.rah.easyconnect_v3.R;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.Characteristic;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.ECRU;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.Service;
import iha.bachelor.smo.aba.rah.easyconnect_v3.service.GetDataIntentService;

import java.io.UnsupportedEncodingException;
import java.util.List;
import java.util.Map;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Typeface;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.BaseExpandableListAdapter;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.SeekBar;
import android.widget.TextView;
import android.widget.Toast;

public class ExpandableListModuleAdapter extends BaseExpandableListAdapter {
	private Activity context;
	private Map<Service, List<Characteristic>> serviceCollections;
	private List<Service> services;
	private final Byte ReadMask = 0x02;
	private final Byte WriteMask = 0x04;
	private String ModuleMacAdress;
	private ECRU ParentEcru;

	public ExpandableListModuleAdapter(Activity context, List<Service> services, Map<Service, List<Characteristic>> laptopCollections, String ModuleMac, ECRU parentEcru) {
		this.context = context;
		this.serviceCollections = laptopCollections;
		this.services = services;
		this.ModuleMacAdress = ModuleMac;
		this.ParentEcru = parentEcru;
	}

	public Object getChild(int groupPosition, int childPosition) {
		return serviceCollections.get(services.get(groupPosition)).get(childPosition);
	}

	public long getChildId(int groupPosition, int childPosition) {
		return childPosition;
	}


	public int getChildrenCount(int groupPosition) {
		return serviceCollections.get(services.get(groupPosition)).size();
	}

	public Object getGroup(int groupPosition) {
		return services.get(groupPosition);
	}

	public int getGroupCount() {
		return services.size();
	}

	public long getGroupId(int groupPosition) {
		return groupPosition;
	}

	public View getGroupView(int groupPosition, boolean isExpanded, View convertView, ViewGroup parent) {
		Service service = (Service) getGroup(groupPosition);
		if (convertView == null) {
			LayoutInflater infalInflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			convertView = infalInflater.inflate(R.layout.service_item, null);
		}
		TextView item = (TextView) convertView.findViewById(R.id.service);
		item.setTypeface(null, Typeface.BOLD);

		try {
			item.setText(new String(service._description.Value,"UTF-8"));
		} catch (UnsupportedEncodingException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

		ImageView update = (ImageView) convertView.findViewById(R.id.update_service);
		update.setOnClickListener(new OnClickListener() {

			public void onClick(View v) {
				AlertDialog.Builder builder = new AlertDialog.Builder(context);
				builder.setMessage("Start Service?");
				builder.setCancelable(false);
				builder.setPositiveButton("Yes",
						new DialogInterface.OnClickListener() {
					public void onClick(DialogInterface dialog, int id) {
						Toast.makeText(context, "Updated", Toast.LENGTH_LONG).show(); 
					}
				});
				builder.setNegativeButton("No",
						new DialogInterface.OnClickListener() {
					public void onClick(DialogInterface dialog, int id) {
						dialog.cancel();
					}
				});
				AlertDialog alertDialog = builder.create();
				alertDialog.show();
			}
		});

		return convertView;
	}

	public boolean hasStableIds() {
		return true;
	}

	public boolean isChildSelectable(int groupPosition, int childPosition) {
		return true;
	}

	@Override
	public View getChildView(final int groupPosition, final int childPosition, boolean isLastChild, View convertView, ViewGroup parent) {
		Characteristic tempChar = (Characteristic) getChild(groupPosition, childPosition);
		LayoutInflater inflater = context.getLayoutInflater();

		tempChar._value.Value[0] = 2;
		
		boolean Read = (tempChar._value.ReadWriteProps & ReadMask) > 0;
		boolean Write = (tempChar._value.ReadWriteProps & WriteMask) > 0;

		if (Read){
			Intent testintent = new Intent(context, GetDataIntentService.class);
			testintent.putExtra(GetDataIntentService.TARGET_MODULE, ModuleMacAdress);
			testintent.putExtra(GetDataIntentService.TARGET_ECRU, ParentEcru.toString());
			testintent.putExtra(GetDataIntentService.TARGET_HANDLE, tempChar._value.handle);
			context.startService(testintent);
		}

		int gui = 0;
		if ( tempChar._gUIFormat.Value != null)
			gui = tempChar._gUIFormat.Value[0];

		int type = 19;
		if ( tempChar._format.Value != null)
			type = tempChar._format.Value[0];

		switch(type){
		case 0x1: // Bool
			if (gui == 5)
				convertView = inflater.inflate(R.layout.characteristic_checkbox, null);
			else{
				convertView = inflater.inflate(R.layout.characteristic_textbox, null);
				EditText edittext = (EditText) convertView.findViewById(R.id.EditText);
				edittext.setText(tempChar._value.Value.toString());
			}
			break;
		case 0x4: // uint8
			switch (gui){
			case 3:
				convertView = inflater.inflate(R.layout.characteristic_seekbar, null);
				SeekBar seeker = (SeekBar) convertView.findViewById(R.id.seekbar);
				if ( tempChar._range.handle != 0){
					seeker.setMax(tempChar._range.Value[1]);
				}
				seeker.setProgress(2);
				break;
			default:
				convertView = inflater.inflate(R.layout.characteristic_textbox, null);
				EditText edittext = (EditText) convertView.findViewById(R.id.EditText);
				
				// edittext.clearFocus();
				break;
			}
			break;
		case 0x19: // utf-8 string
			convertView = inflater.inflate(R.layout.characteristic_textbox, null);
			EditText edittext = (EditText) convertView.findViewById(R.id.EditText);
			// edittext.clearFocus();
			break;
		default:
			break;
		}


//		switch (gui){   // FIX ME Dør fordi den er null
//		case 2:
//			convertView = inflater.inflate(R.layout.characteristic_textbox, null);
//			break;
//		case 3:
//			convertView = inflater.inflate(R.layout.characteristic_seekbar, null);
//			SeekBar seeker = (SeekBar) convertView.findViewById(R.id.seekbar);
//			if ( tempChar._range.handle != 0){
//				seeker.setMax(tempChar._range.Value[1]);
//			}
//			break;
//		case 5:
//			convertView = inflater.inflate(R.layout.characteristic_checkbox, null);
//			break;
//		default:
//			convertView = inflater.inflate(R.layout.characteristic_textbox, null);
//			EditText edittext = (EditText) convertView.findViewById(R.id.EditText);
//			// edittext.clearFocus();
//			break;
//		}

		TextView item = (TextView) convertView.findViewById(R.id.UserDescription);
		try {
			item.setText(new String(tempChar._description.Value,"UTF-8"));
		} catch (UnsupportedEncodingException e) {
			item.setText("Invalid encoded value");
			e.printStackTrace();
		}
		return convertView;
	}

	public class GetDataReceiver extends BroadcastReceiver{
		private static final String LOG_TAG = "GetDataReceiver";
		public static final String RESPONSE = "iha.bachelor.smo.aba.rah.easyconnect_v3.intent.action.PROCESS_RESPONSE";

		@Override
		public void onReceive(Context context, Intent intent) {
			boolean responseData = intent.hasExtra(RESPONSE);
			byte[] reponseMessage = intent.getByteArrayExtra(GetDataIntentService.RESPONSE_DATA);
			
			
			Log.d(LOG_TAG, "BroadcastReceived: " + ModuleInfoParser.bytesToHex(reponseMessage));

		}
	}
}
