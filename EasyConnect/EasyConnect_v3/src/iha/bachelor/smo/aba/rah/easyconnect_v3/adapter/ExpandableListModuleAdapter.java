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
import android.content.IntentFilter;
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
	private static final String LOG_TAG = "ExpandableListAdapter";
	private Activity context;
	private Map<Service, List<Characteristic>> serviceCollections;
	private List<Service> services;
	private final Byte ReadMask = 0x02;
	private final Byte WriteMask = 0x04;
	private String ModuleMacAdress;
	private ECRU ParentEcru;
	private static GetDataReceiver receiver;
	private boolean reloadNeeded;

	public static GetDataReceiver getReceiver(){
		return receiver;
	}
	public static void setReceiver(GetDataReceiver input){
		receiver = input;
	}

	public ExpandableListModuleAdapter(Activity context, List<Service> services, Map<Service, List<Characteristic>> laptopCollections, String ModuleMac, ECRU parentEcru) {
		this.context = context;
		this.serviceCollections = laptopCollections;
		this.services = services;
		this.ModuleMacAdress = ModuleMac;
		this.ParentEcru = parentEcru;

		IntentFilter filter = new IntentFilter(GetDataReceiver.RESPONSE);
		filter.addCategory(Intent.CATEGORY_DEFAULT);
		receiver = new GetDataReceiver();
		context.registerReceiver(receiver, filter);
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
		
		boolean Read = (tempChar._value.ReadWriteProps & ReadMask) > 0;
		reloadNeeded = true;

		if (Read && reloadNeeded){
			Intent testintent = new Intent(context, GetDataIntentService.class);
			testintent.putExtra(GetDataIntentService.TARGET_MODULE, ModuleMacAdress);
			testintent.putExtra(GetDataIntentService.TARGET_ECRU, ParentEcru.toString());
			testintent.putExtra(GetDataIntentService.TARGET_HANDLE, tempChar._value.handle);
			testintent.putExtra(GetDataIntentService.PARENTPOSITION, groupPosition);
			testintent.putExtra(GetDataIntentService.CHILDPOSITION, childPosition);
			context.startService(testintent);
		}

		int gui = 0;
		if ( tempChar._gUIFormat.Value != null)
			gui = tempChar._gUIFormat.Value[0];

		int type = 19;
		if ( tempChar._format.Value != null)
			type = tempChar._format.Value[0];

		switch(type){
		case TypeEnum.Bool:
			switch (gui){
			case GuiEnum.slider:
				convertView = createSeekbarView(tempChar, inflater, convertView);
				break;
			case GuiEnum.checkbox:
				convertView = createCheckboxView(tempChar, inflater, convertView);
				break;
			default:
				convertView = createLabelView(tempChar, inflater, convertView);
				break;
			}
			break;
		case TypeEnum.UINT8:
			switch (gui){
			case GuiEnum.slider:
				convertView = createSeekbarView(tempChar, inflater, convertView);
				break;
			default:
				convertView = createLabelView(tempChar, inflater, convertView);
				break;
			}
			break;
		case TypeEnum.UTFString:
			switch (gui){
			case GuiEnum.label:
				convertView = createLabelView(tempChar, inflater, convertView);
				break;
			default:
				convertView = createLabelView(tempChar, inflater, convertView);
				break;
			}
			break;
		default:
			break;
		}

		TextView item = (TextView) convertView.findViewById(R.id.UserDescription);
		try {
			item.setText(new String(tempChar._description.Value,"UTF-8"));
		} catch (UnsupportedEncodingException e) {
			item.setText("Invalid encoded value");
			e.printStackTrace();
		}
		return convertView;
	}
	
	private View createCheckboxView(Characteristic tempChar, LayoutInflater inflater, View convertView) {
		convertView = inflater.inflate(R.layout.characteristic_checkbox, null);
		return convertView;
	}
	
	private View createSeekbarView(Characteristic tempChar,	LayoutInflater inflater, View convertView) {
		convertView = inflater.inflate(R.layout.characteristic_seekbar, null);
		SeekBar seeker = (SeekBar) convertView.findViewById(R.id.seekbar);
		if ( tempChar._range.handle != 0){
			seeker.setMax(tempChar._range.Value[1]);
		}
		seeker.setProgress(tempChar._value.Value[0]); // TODO: test
		return convertView;
	}
	
	private View createLabelView(Characteristic tempChar,	LayoutInflater inflater, View convertView) {
		boolean Write = (tempChar._value.ReadWriteProps & WriteMask) > 0;
		if (Write){
			convertView = inflater.inflate(R.layout.characteristic_textbox, null);
			EditText edittext = (EditText) convertView.findViewById(R.id.EditText);
			try {
				String test = new String(tempChar._value.Value, "UTF-8");
				edittext.setText(test);
				Log.d(LOG_TAG, "SetTextDone");
			} catch (UnsupportedEncodingException e1) {
				edittext.setText("Invalid encoded value");
				Log.d(LOG_TAG, "Error: " + e1);
			}
			// TODO: Set on changed listener
		}
		else {
			convertView = inflater.inflate(R.layout.characteristic_textview, null);
			TextView labelView = (TextView) convertView.findViewById(R.id.LabelView);
			try {
				labelView.setText(new String(tempChar._value.Value,"UTF-8"));
			} catch (UnsupportedEncodingException e) {
				labelView.setText("Invalid encoded value");
				Log.i(LOG_TAG, "Error:_" + e);
			} catch (Exception e){
				Log.i(LOG_TAG, "Error:_" + e);
			}
		}
		return convertView;
	}

	public void SetNewData(int GroupLocation, int ChildLocation, byte[] data){
		Log.d(LOG_TAG, "data= " + ModuleInfoParser.bytesToHex(data));
		Service tempService = (Service) getGroup(GroupLocation);
		Characteristic tempCharacteristic = (Characteristic) getChild(GroupLocation, ChildLocation);
		if (tempCharacteristic._value.Value != data){
			tempCharacteristic._value.Value = data;
			tempService._characteristicList.set(ChildLocation, tempCharacteristic);
			serviceCollections.put(tempService, tempService._characteristicList);
			reloadNeeded = true;
		}
		else
			reloadNeeded = false;
		Log.d(LOG_TAG, "crasy things completed");
	}

	public class GetDataReceiver extends BroadcastReceiver{
		private static final String LOG_TAG = "GetDataReceiver";
		public static final String RESPONSE = "iha.bachelor.smo.aba.rah.easyconnect_v3.intent.action.PROCESS_RESPONSE";

		@Override
		public void onReceive(Context context, Intent intent) {
			boolean responseData = intent.hasExtra(RESPONSE);
			byte[] reponseMessage = intent.getByteArrayExtra(GetDataIntentService.RESPONSE_DATA);

			int childNr = intent.getIntExtra(GetDataIntentService.CHILDPOSITION, -1);
			int parentNr = intent.getIntExtra(GetDataIntentService.PARENTPOSITION, -1);

			if (childNr >= 0 && parentNr >= 0)
				SetNewData(parentNr, childNr, reponseMessage);
			Log.d(LOG_TAG, "BroadcastReceived: " + ModuleInfoParser.bytesToHex(reponseMessage));

		}
	}

	public class GuiEnum{
		public static final int label = 1;
		public static final int slider = 3;
		public static final int checkbox = 5;
	}
	
	public class TypeEnum{
		public static final int UINT8 = 0x4;
		public static final int Bool = 0x1;
		public static final int UTFString = 0x19;
	}
}

