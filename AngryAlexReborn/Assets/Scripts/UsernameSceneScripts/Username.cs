using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Username : MonoBehaviour
{

	public InputField userNameField;

    public void RandomizeUsername()
    {
    	System.Random r = new System.Random();
    	int nameLength = r.Next(0, 10);

    	const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		StringBuilder result = new StringBuilder(nameLength);
		  for (int i = 0; i < nameLength; i++) {
		    result.Append(characters[r.Next(characters.Length)]);
		  }

    	userNameField.text = result.ToString();
    }
}
