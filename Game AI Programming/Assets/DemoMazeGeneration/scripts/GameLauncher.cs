using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadNewGameScene()
    {
        SceneManager.LoadScene(1);//game scene

    }

}
