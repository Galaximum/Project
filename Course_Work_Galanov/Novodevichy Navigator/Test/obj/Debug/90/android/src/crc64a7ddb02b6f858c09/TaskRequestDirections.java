package crc64a7ddb02b6f858c09;


public class TaskRequestDirections
	extends android.os.AsyncTask
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onPostExecute:(Ljava/lang/Object;)V:GetOnPostExecute_Ljava_lang_Object_Handler\n" +
			"n_doInBackground:([Ljava/lang/Object;)Ljava/lang/Object;:GetDoInBackground_arrayLjava_lang_Object_Handler\n" +
			"";
		mono.android.Runtime.register ("Test.TaskRequestDirections, Test", TaskRequestDirections.class, __md_methods);
	}


	public TaskRequestDirections ()
	{
		super ();
		if (getClass () == TaskRequestDirections.class)
			mono.android.TypeManager.Activate ("Test.TaskRequestDirections, Test", "", this, new java.lang.Object[] {  });
	}

	public TaskRequestDirections (crc64a7ddb02b6f858c09.MainActivity p0)
	{
		super ();
		if (getClass () == TaskRequestDirections.class)
			mono.android.TypeManager.Activate ("Test.TaskRequestDirections, Test", "Test.MainActivity, Test", this, new java.lang.Object[] { p0 });
	}


	public void onPostExecute (java.lang.Object p0)
	{
		n_onPostExecute (p0);
	}

	private native void n_onPostExecute (java.lang.Object p0);


	public java.lang.Object doInBackground (java.lang.Object[] p0)
	{
		return n_doInBackground (p0);
	}

	private native java.lang.Object n_doInBackground (java.lang.Object[] p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
