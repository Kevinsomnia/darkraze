using UnityEngine;
using System.Collections;

public class UIController : MonoBehaviour {
	public Camera guiCamera;
    public Transform guiRoot;

	public UILabel weaponName;
	public UILabel curAmmoDisplay;
	public UILabel ammoLeftDisplay;
	public UISlider ammoBar;
	public UITexture weaponIcon;
	public UILabel weaponSlot;
	
	public UISlider healthBar;
	public UILabel healthText;
	public UISlider shieldBar;
	public UILabel shieldText;
	public UISlider staminaBar;
	public MeshRenderer bloodyScreen;
    public UITexture shieldTexture;
	public Renderer muzzleHelmetGlow;
	
	public UILabel useGUI;
    public UILabel empRecalibrate;
		
	public GameObject pauseMenu;
	public BlurEffect pauseBlur;
	public BlurEffect pauseBlur2;
	
	public CrosshairGUI crosshairs;
	
	public UIPanel settingsPanel;
	
	public Transform parentOfObjectives;

    public TeamMarkingSystem teamMarkerSystem;
	
	public UILabel waveCounter;
	public UILabel enemiesLeftCounter;
	
	public UILabel grabText;
	
	public UILabel fireModeLabel;
    public UILabel reloadIndicatorLabel;
	public UISprite grenadeSelectionSprite;
    public UILabel grenadeSelectionLabel;
	public UILabel grenadeOneLabel;
	public UILabel grenadeTwoLabel;
    public UITexture grenadeOneIcon;
    public UITexture grenadeTwoIcon;
	
	public MultiplayerGUI mpGUI;
	public FlickeringGUI[] flickeringPanels;
    public ParticleSystem rainFX;
    public UISprite fadeFromBlack;
    public Transform hitIndicatorRoot;

    //Access from debug inspector
    public GameObject[] toggleHUD;
    public ScreenAdjustment screenAdjustment;
    
    [HideInInspector] public GameManager gManager;

	public void Awake() {
        gManager = GetComponent<GameManager>();
	}

	public void EnableSurvival() {
		waveCounter.gameObject.SetActive(true);
	}

    public void UpdateHUD(bool enable) {
        for(int i = 0; i < toggleHUD.Length; i++) {
            toggleHUD[i].SetActive(enable);
        }
    }
}