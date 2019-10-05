using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{
	public InputField userNameField;
		
    public void LoadByIndex(int sceneIndex) {
    	SceneManager.LoadScene(sceneIndex);
		Player.UserName = userNameField.text; 
		
		Debug.Log(Player.UserName);
    }
}