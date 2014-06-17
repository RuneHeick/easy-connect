package iha.bachelor.smo.aba.rah.easyconnect_v3.adapter;

import iha.bachelor.smo.aba.rah.easyconnect_v3.R;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.Characteristic;
import iha.bachelor.smo.aba.rah.easyconnect_v3.model.Service;

import java.io.UnsupportedEncodingException;
import java.util.List;
import java.util.Map;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.graphics.Typeface;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.View.OnClickListener;
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
 
    public ExpandableListModuleAdapter(Activity context, List<Service> services, Map<Service, List<Characteristic>> laptopCollections) {
        this.context = context;
        this.serviceCollections = laptopCollections;
        this.services = services;
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
		final Characteristic tempChar = (Characteristic) getChild(groupPosition, childPosition);
		LayoutInflater inflater = context.getLayoutInflater();
        
		
		
		
		
		//switch (5){
		switch (tempChar._gUIFormat.Value[0]){   // FIX ME Dør fordi den er null
			case 2:
				convertView = inflater.inflate(R.layout.characteristic_textbox, null);
				break;
			case 3:
				convertView = inflater.inflate(R.layout.characteristic_seekbar, null);
				SeekBar seeker = (SeekBar) convertView.findViewById(R.id.seekbar);
				if ( tempChar._range.handle != 0){
					seeker.setMax(tempChar._range.Value[1]);
				}
				break;
			case 5:
				convertView = inflater.inflate(R.layout.characteristic_checkbox, null);
				break;
			default:
				convertView = inflater.inflate(R.layout.characteristic_textbox, null);
				EditText edittext = (EditText) convertView.findViewById(R.id.EditText);
				edittext.clearFocus();
				break;
//			case 1:
//				//lable needs to be implemented
//				break;
//			case 4:
//				//List needs to be implemented
//				break;
//			
//			case 6:
//				//Time needs to be implemented
//				break;
//			case 7:
//				//Date needs to be implemented
//				break;
//			case 8:
//				// time/Date needs to be implemented
//				break;
			
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
}
