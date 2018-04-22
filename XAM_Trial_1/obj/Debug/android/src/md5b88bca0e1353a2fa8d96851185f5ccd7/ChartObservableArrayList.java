package md5b88bca0e1353a2fa8d96851185f5ccd7;


public class ChartObservableArrayList
	extends md5b88bca0e1353a2fa8d96851185f5ccd7.ObservableArrayList
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("Com.Syncfusion.Charts.ChartObservableArrayList, Syncfusion.SfChart.Android, Version=16.1451.0.32, Culture=neutral, PublicKeyToken=null", ChartObservableArrayList.class, __md_methods);
	}


	public ChartObservableArrayList ()
	{
		super ();
		if (getClass () == ChartObservableArrayList.class)
			mono.android.TypeManager.Activate ("Com.Syncfusion.Charts.ChartObservableArrayList, Syncfusion.SfChart.Android, Version=16.1451.0.32, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public ChartObservableArrayList (int p0)
	{
		super (p0);
		if (getClass () == ChartObservableArrayList.class)
			mono.android.TypeManager.Activate ("Com.Syncfusion.Charts.ChartObservableArrayList, Syncfusion.SfChart.Android, Version=16.1451.0.32, Culture=neutral, PublicKeyToken=null", "System.Int32, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0 });
	}


	public ChartObservableArrayList (java.util.Collection p0)
	{
		super (p0);
		if (getClass () == ChartObservableArrayList.class)
			mono.android.TypeManager.Activate ("Com.Syncfusion.Charts.ChartObservableArrayList, Syncfusion.SfChart.Android, Version=16.1451.0.32, Culture=neutral, PublicKeyToken=null", "System.Collections.ICollection, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0 });
	}

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
