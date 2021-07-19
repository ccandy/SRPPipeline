using UnityEngine;
using System.Collections;

using Frameworks;

using UnityEngine.UI;

public class FileIOSample : MonoBehaviour 
{
	public Text StateText = null;

	public InputField FileText = null;
	// Use this for initialization
	void Start () 
	{
		if (!FileIO.InitDataPath())
		{
			string Info = string.Format("InitDataPath Failed!\nDataPath:\n\t{0}\npersistentDataPath:\n\t{1}", FileIO.DataPath, Application.persistentDataPath);
			if (StateText != null)
			{
				StateText.text = Info;
				Debug.Log(Info);
			}
				
		}
		else
		{
			string Info = string.Format("InitDataPath succeed!\nDataPath:\n\t{0}\npersistentDataPath:\n\t{1}", FileIO.DataPath, Application.persistentDataPath);
			if (StateText != null)
			{
				StateText.text = Info;
				Debug.Log(Info);
			}
				
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	public void LoadSampleFile()
	{
		if (FileText == null)
			return;

		string fileText = null;
		if (FileIO.LoadToDataPath("test.txt", ref fileText))
		{
			FileText.text = fileText;
			if (StateText != null)
				StateText.text = "Load succeed!";
		}
		else
		{
			if (StateText != null)
				StateText.text = "Load failed!";
		}
	}

	public void SaveSampleFile()
	{
		if (FileText == null)
			return;

		if (FileIO.SaveToDataPath("test.txt", FileText.text))
		{
			if (StateText != null)
				StateText.text = "Save succeed!";
		}
		else
		{
			if (StateText != null)
				StateText.text = "Save failed!";
		}
	}
}
