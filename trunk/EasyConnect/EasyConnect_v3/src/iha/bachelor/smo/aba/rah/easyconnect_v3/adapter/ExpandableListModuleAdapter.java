package iha.bachelor.smo.aba.rah.easyconnect_v3.adapter;

import iha.bachelor.smo.aba.rah.easyconnect_v3.R;

import java.util.List;
import java.util.Map;

import android.app.Activity;
import android.content.Context;
import android.graphics.Typeface;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseExpandableListAdapter;
import android.widget.TextView;
 
public class ExpandableListModuleAdapter extends BaseExpandableListAdapter {
	private Activity context;
    private Map<String, List<String>> serviceCollections;
    private List<String> serviceNames;
 
    public ExpandableListModuleAdapter(Activity context, List<String> services, Map<String, List<String>> serviceCollections) {
        this.context = context;
        this.serviceCollections = serviceCollections;
        this.serviceNames = services;
    }

	public Object getChild(int groupPosition, int childPosition) {
        return serviceCollections.get(serviceNames.get(groupPosition)).get(childPosition);
    }
 
    public long getChildId(int groupPosition, int childPosition) {
        return childPosition;
    }
     
     
    public View getChildView(final int groupPosition, final int childPosition, boolean isLastChild, View convertView, ViewGroup parent) {
        final String characteristic = (String) getChild(groupPosition, childPosition);
        LayoutInflater inflater = context.getLayoutInflater();
         
        if (convertView == null) {
            convertView = inflater.inflate(R.layout.characteristic_item, null);
        }
         
        TextView item = (TextView) convertView.findViewById(R.id.characteristic_id);
        
        item.setText(characteristic);
        return convertView;
    }
 
    public int getChildrenCount(int groupPosition) {
        return serviceCollections.get(serviceNames.get(groupPosition)).size();
    }
 
    public Object getGroup(int groupPosition) {
        return serviceNames.get(groupPosition);
    }
 
    public int getGroupCount() {
        return serviceNames.size();
    }
 
    public long getGroupId(int groupPosition) {
        return groupPosition;
    }
 
    public View getGroupView(int groupPosition, boolean isExpanded, View convertView, ViewGroup parent) {
        String serviceName = (String) getGroup(groupPosition);
        if (convertView == null) {
            LayoutInflater infalInflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            convertView = infalInflater.inflate(R.layout.service_item, null);
        }
        TextView item = (TextView) convertView.findViewById(R.id.service_id);
        item.setTypeface(null, Typeface.BOLD);
        item.setText(serviceName);
        return convertView;
    }
 
    public boolean hasStableIds() {
        return true;
    }
 
    public boolean isChildSelectable(int groupPosition, int childPosition) {
        return true;
    }
}