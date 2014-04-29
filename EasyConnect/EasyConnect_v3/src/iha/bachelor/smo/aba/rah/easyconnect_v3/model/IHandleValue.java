package iha.bachelor.smo.aba.rah.easyconnect_v3.model;

public interface IHandleValue {
	public short getHandle();
	public void setHandle(short handle);
	public byte[] getValue();
	public void setValue(byte[] value);
	
	public class HandleValuePair implements IHandleValue{
	    public short handle;
	    public byte[] Value;
	    
		public byte[] getValue() {
			return Value;
		}
		
		public void setValue(byte[] value) {
			Value = value;
		}
		
		public short getHandle() {
			return handle;
		}
		
		public void setHandle(short handle) {
			this.handle = handle;
		}
	}
	
	public class CharacteristicValueHandle implements IHandleValue{
	    public short handle;
	    public byte[] Value;
	    public byte ReadWriteProps;

		public byte getReadWriteProps() {
			return ReadWriteProps;
		}

		public void setReadWriteProps(byte readWriteProps) {
			ReadWriteProps = readWriteProps;
		}

		public byte[] getValue() {
			return Value;
		}
		
		public void setValue(byte[] value) {
			Value = value;
		}
		
		public short getHandle() {
			return handle;
		}
		
		public void setHandle(short handle) {
			this.handle = handle;
		}
	}
}
