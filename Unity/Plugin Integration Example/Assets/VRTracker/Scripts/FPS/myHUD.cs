using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class myHUD : MonoBehaviour {

    //private vp_FPPlayerEventHandler m_Player = null;
    private float m_FormattedHealth = 0.0f;
    private ScoreScript scoreHandler;
	//private ZombieManager zombieManager = null;
    private TextMesh HP;
    private TextMesh Ammo;
    private TextMesh score;
	private TextMesh zombieNumber;


    public float HealthMultiplier = 10.0f;

    /*string FormattedHealth
    {

        get
        {
            //m_FormattedHealth = (m_Player.Health.Get() * HealthMultiplier);
            if (m_FormattedHealth < 1.0f)
                //m_FormattedHealth = (m_Player.Dead.Active ? Mathf.Min(m_FormattedHealth, 0.0f) : 1.0f);
            if (m_Player.Dead.Active && m_FormattedHealth > 0.0f)
                m_FormattedHealth = 0.0f;
            return ((int)m_FormattedHealth).ToString();
        }

    }*/

    // Use this for initialization
    void Start () {
        //m_Player = this.GetComponentInParent<vp_FPPlayerEventHandler>();
        scoreHandler = this.GetComponentInParent<ScoreScript>();
		HP = transform.Find("HP").GetComponent<TextMesh>();
        Ammo = transform.Find("Ammo").GetComponent<TextMesh>();
        score = transform.Find("Score").GetComponent<TextMesh>();
		zombieNumber = transform.Find ("Zombie").GetComponent<TextMesh> ();
	}
	
	// Update is called once per frame
	void Update () {
        updateAmmo();
        updateHealth();
        UpdateScore();
		//UpdateZombie ();
	}

    private void updateAmmo()
    {
        //Ammo.text = m_Player.CurrentWeaponAmmoCount.Get().ToString();
    }

    private void updateHealth()
    {
        //HP.text = FormattedHealth;
    }

    private void UpdateScore()
    {
        score.text = scoreHandler.getScore().ToString();
    }

	public void UpdateZombie(int number){
			//Debug.Log ("Nombre de zombie " + zombieManager.zombies.Count.ToString ());
			zombieNumber.text = number.ToString ();

	}
}
