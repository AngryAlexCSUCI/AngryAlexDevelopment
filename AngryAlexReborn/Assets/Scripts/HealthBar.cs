using System;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : Player
{
    public float m_StartingHealth;     // The amount of health each tank starts with.

    public Slider m_Slider_self;     // The slider to represent how much health the tank currently has.
    public Image m_Fill_self;                           // The image component of the slider.

    public Color m_FullHealthColor = Color.green;       // The color the health bar will be when on full health.
    public Color m_ZeroHealthColor = Color.red;         // The color the health bar will be when on no health.
   
    [HideInInspector]
    public bool isLocalPlayer = false;

    [HideInInspector]
    public float m_CurrentHealth;            // How much health the tank currently has.
    private bool m_Dead;                     // Has the tank been reduced beyond zero health yet?



    [HideInInspector]
    public string playerName { get; set; }


    void FixedUpdate()
    {
    }
    private void Awake()
    {
    }
    void Start()
    {
        if (isLocalPlayer)
        {
            GetComponentInChildren<Canvas>().enabled = false;
        }
    }
    private void OnEnable()
    {
        // When the tank is enabled, reset the tank's health and whether or not it's dead.
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;

        // Update the health slider's value and color.
        SetHealthUI();
    }


    public void TakeDamage(float amount)
    {
        Debug.Log("HealthBar TakeDamage: ENTER");
        Debug.Log("HealthBar TakeDamage: testing if local player");
        if (!isLocalPlayer)
        {
            Debug.Log("HealthBar TakeDamage: local player, END TakeDamage");
            return;
        }
        Debug.Log("HealthBar TakeDamage: remote player, continuing");


        // Reduce current health by the amount of damage done.
        m_CurrentHealth -= amount;
        //make sure health doesn't go negative
        if (m_CurrentHealth < 0)
        {
            m_CurrentHealth = 0;
        }
        Debug.Log("HealthBar TakeDamage: Dealt " + amount + " damage");
        // Change the UI elements appropriately.
        SetHealthUI();

        // If the current health is at or below zero and it has not yet been registered, call OnDeath.
        Debug.Log("HealthBar TakeDamage: Testing health and dead status");
        if (m_CurrentHealth <= 0f && !m_Dead)
        {
            Debug.Log("HealthBar TakeDamage: health below zero and player not dead, running OnDeath");
            OnDeath();
        }
        Debug.Log("HealthBar TakeDamage: END");
    }

    private void SetHealthUI()
    {
        Debug.Log("HealthBar SetHealthUI: ENTER");

        if (!isLocalPlayer)
        {
            return;
        } else {
            // Set the slider's value appropriately.
            // m_Slider.value = m_CurrentHealth;
            m_Slider_self.value = m_CurrentHealth;
            // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
            // m_Fill.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
            m_Fill_self.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
        }
        Debug.Log("HealthBar SetHealthUI: END");
    }

    private void OnDeath()
    {
        // Set the flag so that this function is only called once.
        m_Dead = true;

        // Turn the car off.
        gameObject.SetActive(false);
      
        Debug.Log("Player " + gameObject.name + " killed.");
        WebSocketManager.HealthChangeJson player = new WebSocketManager.HealthChangeJson(gameObject.name, UserName);
        string playerJson = JsonUtility.ToJson(player);
        WebSocketManager.instance.Dispatch("disconnect", playerJson, true);
    }

    private float spriteBlinkingTimer = 0.0f;
    private float spriteBlinkingMiniDuration = 0.1f;
    private float spriteBlinkingTotalTimer = 0.0f;
    private float spriteBlinkingTotalDuration = 1.0f;
    [HideInInspector]
    public bool startBlinking = false;
    [HideInInspector]
    public GameObject carObject;
    void Update()
    {
        if (startBlinking == true)
        {
            SpriteBlinkingEffect(carObject);
        }
    }

    public void SpriteBlinkingEffect(GameObject carObject)
    {
        spriteBlinkingTotalTimer += Time.deltaTime;
        if (spriteBlinkingTotalTimer >= spriteBlinkingTotalDuration)
        {
            startBlinking = false;
            spriteBlinkingTotalTimer = 0.0f;
            carObject.GetComponent<SpriteRenderer>().enabled = true;   // according to your sprite
            return;
        }

        spriteBlinkingTimer += Time.deltaTime;
        if (spriteBlinkingTimer >= spriteBlinkingMiniDuration)
        {
            spriteBlinkingTimer = 0.0f;
            if (carObject.GetComponent<SpriteRenderer>().enabled == true)
            {
                carObject.GetComponent<SpriteRenderer>().enabled = false;  //make changes
            }
            else
            {
                carObject.GetComponent<SpriteRenderer>().enabled = true;   //make changes
            }
        }
    }
}
