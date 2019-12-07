using UnityEngine;
using UnityEngine.UI;
public class HealthBar : Player
{
    public float m_StartingHealth;               // The amount of health each tank starts with.
    public Slider m_Slider;                             // The slider to represent how much health the tank currently has.
    public Image m_Fill;      // The image component of the slider.

    public Slider m_Slider_self;                             // The slider to represent how much health the tank currently has.

    public Image m_Fill_self;                           // The image component of the slider.
    public Color m_FullHealthColor = Color.green;       // The color the health bar will be when on full health.
    public Color m_ZeroHealthColor = Color.red;         // The color the health bar will be when on no health.
   
    [HideInInspector]
    public bool isLocalPlayer = false;

    [HideInInspector]
    public float m_CurrentHealth;                      // How much health the tank currently has.
    private bool m_Dead;                                // Has the tank been reduced beyond zero health yet?

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


    public void TakeDamage(float amount, string to, string from)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Reduce current health by the amount of damage done.
        m_CurrentHealth -= amount;
        Debug.Log("take damage 20)");

        HealthChangeObj healthChange = new HealthChangeObj(to, from, amount);
        WebSocketManager.instance.Dispatch("health_damage", JsonUtility.ToJson(healthChange), true);
        
        // Change the UI elements appropriately.
        SetHealthUI();

        // If the current health is at or below zero and it has not yet been registered, call OnDeath.
        if (m_CurrentHealth <= 0f && !m_Dead)
        {
            OnDeath();
        }
    }

    private void SetHealthUI()
    {

        if (!isLocalPlayer)
        {
            return;
        }

        // Set the slider's value appropriately.
        m_Slider.value = m_CurrentHealth;
        m_Slider_self.value = m_CurrentHealth;
        // Interpolate the color of the bar between the chosen colors based on the current percentage of the starting health.
        m_Fill.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
        m_Fill_self.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
    }

    private void OnDeath()
    {
        // Set the flag so that this function is only called once.
        m_Dead = true;

        // Turn the car off.
        gameObject.SetActive(false);
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


    public class HealthChangeObj
    {
        public string name { get; set; }
        public string from { get; set; }
        public float damage { get; set; }

        public HealthChangeObj(string _name, string _from, float _damage)
        {
            name = _name;
            from = _from;
            damage = _damage;
        }

    }

}
