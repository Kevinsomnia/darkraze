using UnityEngine;
using System.Collections;

[System.Serializable]
public class Tougheners
{
    //*** MODIFIERS ***
    public float healthModifier = 1.06f;
    public float damageModifer = 1.05f;
    public float valueModifier = 1.1f;
}

[System.Serializable]
public class WaveManager : MonoBehaviour
{
    public GameObject[] spawnableEnemies; //A list of enemies to spawn from
    public Transform[] spawnPoints;
    public float spawnOffsetY = 1.5f;
    public int currentWave = 1;
    public float restTime = 30f;
    public int baseAmountOfEnemies = 6;
    public int amountOfEnemiesLow = 2;
    public int amountOfEnemiesHigh = 4;
    public float spawnRadius = 3;
    public float spawnRate = 4;
    public float spawnTimeModifier = 0.95f;
    public AudioClip newWaveSound;

    public Tougheners modifiers = new Tougheners();

    private float timer;
    private float waveTimer;
    private int enemiesLeft;
    private int enemiesToSpawn;
    private int spawnedEnemies;
    private int nextWaveTimer;
    private bool hasCompletedWave;
    private bool startWaveTimer;
    private bool enableSpawning;

    private TypingEffect typingEffect;

    private UILabel waveCounter;
    private UILabel enemiesLeftCounter;

    private Tougheners currentModifier;

    [HideInInspector] public int enemiesKilled;

    void Awake()
    {
        GeneralVariables.waveManager = this;
    }

    void Start()
    {
        UIController controller = GeneralVariables.uiController;
        controller.EnableSurvival();
        waveCounter = controller.waveCounter;
        enemiesLeftCounter = controller.enemiesLeftCounter;

        currentModifier = new Tougheners();
        currentModifier.damageModifer = 1f;
        currentModifier.healthModifier = 1f;
        currentModifier.valueModifier = 1f;

        typingEffect = waveCounter.GetComponent<TypingEffect>();

        enemiesToSpawn = baseAmountOfEnemies;
        enemiesLeft = enemiesToSpawn;
        typingEffect.UpdateText("WAVE " + currentWave.ToString());
        enableSpawning = true;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (startWaveTimer)
        {
            waveTimer += Time.deltaTime;
        }

        if (timer >= spawnRate && spawnedEnemies < enemiesToSpawn && enableSpawning)
        {
            SpawnEnemy();
        }

        if (enemiesLeft > 0)
        {
            enemiesLeftCounter.text = "Enemies Left: " + enemiesLeft.ToString();
        }
        else
        {
            if (!hasCompletedWave)
            {
                hasCompletedWave = true;
                startWaveTimer = true;
            }

            if (waveTimer >= restTime)
            {
                StartNewWave();
            }

            nextWaveTimer = Mathf.RoundToInt(restTime - waveTimer);
            enemiesLeftCounter.text = "Next Wave Starts In: " + DarkRef.GetTimerFormat(nextWaveTimer);
        }
    }

    public void SpawnEnemy()
    {
        Vector3 randomSpawn = new Vector3(Random.Range(-spawnRadius, spawnRadius), 1000f, Random.Range(-spawnRadius, spawnRadius));
        RaycastHit hit;
        if (Physics.Raycast(spawnPoints[Random.Range(0, spawnPoints.Length)].position + randomSpawn, Vector3.down, out hit, 5000f))
        {
            GameObject spawnedEnemy = Instantiate(spawnableEnemies[Random.Range(0, spawnableEnemies.Length)].gameObject, hit.point + new Vector3(0f, spawnOffsetY, 0f), Quaternion.identity) as GameObject;
            spawnedEnemy.SendMessage("Toughen", currentModifier, SendMessageOptions.RequireReceiver);
        }
        spawnedEnemies++;
        timer = 0f;
    }

    private void StartNewWave()
    {
        NGUITools.PlaySound(newWaveSound);
        currentWave++;
        hasCompletedWave = false;

        if (currentWave > 1)
        {
            currentModifier.damageModifer = Mathf.Pow(modifiers.damageModifer, currentWave - 1);
            currentModifier.healthModifier = Mathf.Pow(modifiers.healthModifier, currentWave - 1);
            currentModifier.valueModifier = Mathf.Pow(modifiers.valueModifier, currentWave - 1);
        }

        spawnedEnemies = 0;
        startWaveTimer = false;
        timer = 0f;
        waveTimer = 0;
        enemiesToSpawn += Random.Range(amountOfEnemiesLow, amountOfEnemiesHigh);
        enemiesLeft += enemiesToSpawn;
        spawnRate *= spawnTimeModifier;
        typingEffect.UpdateText("WAVE " + currentWave.ToString());
    }

    public void EnemyKilled()
    {
        enemiesLeft--;
        enemiesKilled++;
    }

    public void ToggleSpawn(bool e)
    {
        enableSpawning = e;
    }
}