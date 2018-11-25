using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BlockemGame : MonoBehaviour {

    [SerializeField] Camera mainCam;
    [SerializeField] GameObject shield;

    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject collectiblePrefab;

    [SerializeField] GameObject TurretN;
    [SerializeField] GameObject TurretNE;
    [SerializeField] GameObject TurretE;
    [SerializeField] GameObject TurretSE;
    [SerializeField] GameObject TurretS;
    [SerializeField] GameObject TurretSW;
    [SerializeField] GameObject TurretW;
    [SerializeField] GameObject TurretNW;

    [SerializeField] GameObject startLevelN;
    [SerializeField] GameObject startLevelE;
    [SerializeField] GameObject startLevelS;
    [SerializeField] GameObject startLevelW;

    [SerializeField] GameObject highScoreLevelText;
    [SerializeField] GameObject highScorePointsText;

    [SerializeField] Text currentLevelText;
    [SerializeField] Text currentPointsText;
    [SerializeField] Image indicator;

    private Color textButtonNormal;
    private Color textButtonDisable;

    private int highScoreLevel;
    private int highScorePoints;
    private string filepath;

    private GameObject[] turrets;
    private GameObject projectile;

    private int level;
    private int points;
    private bool playing;
    private bool lost;
    private float levelTimer;
    private float levelTimerThreshold;

    private float timerShoot;
    private float timerShootThreshold;
    private float shootSpeed;
    private int turretIndex;
    private int selection;

    private bool shieldUp;
    private float shieldRaiseDist;


    // For raining in between levels
    [SerializeField] GameObject rainDropPrefab;
    [SerializeField] GameObject parent;

    private const float LIFETIME = 0.5f;
    private const float SPAWN_TIME_THRESHOLD = 0.03f;

    private List<GameObject> rainDrops;
    private List<float> rainDropTimer;

    private float spawnTimer;

    // Use this for initialization
    void Start () {
        textButtonNormal = new Color(0.8f, 0.0f, 0.0f);
        textButtonDisable = new Color(0.5f, 0.5f, 0.5f);

        levelTimerThreshold = 30.0f;
        timerShootThreshold = 2.0f;

        turrets = new GameObject[8];
        turrets[0] = TurretN;
        turrets[1] = TurretNE;
        turrets[2] = TurretE;
        turrets[3] = TurretSE;
        turrets[4] = TurretS;
        turrets[5] = TurretSW;
        turrets[6] = TurretW;
        turrets[7] = TurretNW;

        shieldUp = false;
        shieldRaiseDist = 0.35f;

        filepath = Application.persistentDataPath + "/highScoresBlockem.txt";
        loadHighScores();

        try
        {
            highScoreLevel = int.Parse(highScoreLevelText.GetComponent<TextMesh>().text);
        }
        catch
        {
            highScoreLevel = -1;
        }

        try
        {
            highScorePoints = int.Parse(highScorePointsText.GetComponent<TextMesh>().text);
        }
        catch
        {
            highScorePoints = -1;
        }

        indicator.gameObject.SetActive(false);
        resetGame();

        rainDrops = new List<GameObject>();
        rainDropTimer = new List<float>();

        spawnTimer = 0.0f;
    }
	
	// Update is called once per frame
	void Update () {
        float deltaTime = Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && playing)
        {
            shield.transform.Translate(Vector3.up * shieldRaiseDist);
            shieldUp = true;
        }

        if (Input.GetMouseButtonUp(0) && shieldUp)
        {
            shield.transform.Translate(Vector3.down * shieldRaiseDist);
            shieldUp = false;
        }

        if (playing)
        {
            levelTimer += deltaTime;

            timerShoot += deltaTime;
            if (timerShoot >= timerShootThreshold)
            {
                turretShoot();
            }

            if (projectile != null)
            {
                Vector3 moveDir = (transform.position - projectile.transform.position).normalized;

                projectile.transform.Translate(moveDir * shootSpeed);

                indicator.gameObject.SetActive(true);

                setIndicatorArrow();
            }
            else
            {
                indicator.gameObject.SetActive(false);
            }

            if (levelTimer >= levelTimerThreshold)
            {
                playing = false;
                levelTimer = 0.0f;
                winLevel();
            }
        }

        if (Input.GetMouseButton(0) && !playing)
        {
            Debug.Log("spawning area");
            
            // Spawning RainDrops
            spawnTimer += deltaTime;
            if (spawnTimer >= SPAWN_TIME_THRESHOLD)
            {
                var rand = new System.Random();

                GameObject rainDrop = Instantiate(rainDropPrefab, parent.transform);
                rainDrop.transform.Translate(new Vector3(rand.Next(-15, 15), rand.Next(-15, 15), 0.0f));

                rainDrops.Add(rainDrop);
                rainDropTimer.Add(0.0f);

                spawnTimer = 0.0f;
            }
        }

        Debug.Log("RDC: " + rainDrops.Count);
        Debug.Log("RDT: " + rainDropTimer.Count);

        // Raindrop Lifecycle
        for (int i = 0; i < rainDrops.Count; i++)
        {
            rainDropTimer[i] += deltaTime;

            Debug.Log(i + ": " + rainDropTimer[i]);

            if (rainDropTimer[i] >= LIFETIME)
            {
                rainDropTimer.RemoveAt(i);
                Destroy(rainDrops[i]);
                rainDrops.RemoveAt(i);
                i--;
            }
        }
    }

    private void resetGame()
    {
        level = 1;
        levelTimer = 0.0f;

        timerShoot = 0.0f;
        shootSpeed = 0.3f;

        lost = false;
        
        currentLevelText.text = "1";
        currentPointsText.text = "0";

        nextProjectile();
    }

    public void startLevel()
    {
        if (level == 1 || lost)
        {
            resetGame();
        }

        playing = true;

        startLevelN.GetComponent<BoxCollider>().enabled = false;
        startLevelE.GetComponent<BoxCollider>().enabled = false;
        startLevelS.GetComponent<BoxCollider>().enabled = false;
        startLevelW.GetComponent<BoxCollider>().enabled = false;

        startLevelN.GetComponent<TextMesh>().color = textButtonDisable;
        startLevelE.GetComponent<TextMesh>().color = textButtonDisable;
        startLevelS.GetComponent<TextMesh>().color = textButtonDisable;
        startLevelW.GetComponent<TextMesh>().color = textButtonDisable;
    }

    private void winLevel()
    {
        bool change = false;

        if (level > highScoreLevel)
        {
            highScoreLevel = level;
            change = true;
        }
        
        if (points > highScorePoints)
        {
            highScorePoints = points;
            change = true;
        }

        if (change)
        {
            highScoreLevelText.GetComponent<TextMesh>().text = highScoreLevel.ToString();
            highScorePointsText.GetComponent<TextMesh>().text = highScorePoints.ToString();

            saveHighScores();
        }

        enableStartButtons();
        timerShoot = 0.0f;
        levelTimer = 0.0f;
        level++;
        shootSpeed += 0.05f;
        timerShootThreshold -= 0.1f;
    }

    private void loseLevel()
    {
        lost = true;
        playing = false;
        indicator.gameObject.SetActive(false);

        enableStartButtons();
    }

    private void turretShoot()
    {
        if (selection <= 0)
        {
            projectile = Instantiate(collectiblePrefab, turrets[turretIndex].transform);
        }
        else
        {
            projectile = Instantiate(projectilePrefab, turrets[turretIndex].transform);
        }
        projectile.transform.position = turrets[turretIndex].transform.GetChild(0).position + Vector3.up * 3.2f;

        timerShoot = 0.0f;

        nextProjectile();
    }

    private void enableStartButtons()
    {
        startLevelN.GetComponent<TextMesh>().color = textButtonNormal;
        startLevelE.GetComponent<TextMesh>().color = textButtonNormal;
        startLevelS.GetComponent<TextMesh>().color = textButtonNormal;
        startLevelW.GetComponent<TextMesh>().color = textButtonNormal;

        startLevelN.GetComponent<BoxCollider>().enabled = true;
        startLevelE.GetComponent<BoxCollider>().enabled = true;
        startLevelS.GetComponent<BoxCollider>().enabled = true;
        startLevelW.GetComponent<BoxCollider>().enabled = true;
    }

    private void nextProjectile()
    {
        var rand = new System.Random();
        turretIndex = rand.Next(0, turrets.Length);
        selection = rand.Next(0, 10); // 10% for collectible
    }

    private void setIndicatorArrow()
    {
        float angle = Vector3.Angle(mainCam.transform.forward, projectile.transform.position - transform.position);
        indicator.transform.localEulerAngles = new Vector3(0.0f, 0.0f, angle + 90.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        float angle = Vector3.Angle(mainCam.transform.forward, projectile.transform.position - transform.position);

        if (other.gameObject.CompareTag("Collectible"))
        {
            if (shieldUp && angle < 20.0f)
            {
                // block sound (no bonus points)
            }
            else
            {
                points += level * 5;
            }
        }
        else // Projectile
        {
            if (shieldUp && angle < 20.0f)
            {
                points += level;
            }
            else
            {
                loseLevel();
            }
        }

        currentPointsText.text = points.ToString();

        Destroy(other.gameObject);
        projectile = null;
    }

    public void saveHighScores()
    {
        StreamWriter writer = new StreamWriter(filepath, false);
        writer.WriteLine(highScoreLevelText.GetComponent<TextMesh>().text);
        writer.WriteLine(highScorePointsText.GetComponent<TextMesh>().text);
        writer.Close();
    }

    public void loadHighScores()
    {
        if (File.Exists(filepath))
        {
            StreamReader reader = new StreamReader(filepath, false);
            string level = reader.ReadLine();
            string points = reader.ReadLine();
            


            try
            {
                highScoreLevel = int.Parse(level);
                highScoreLevelText.GetComponent<TextMesh>().text = level;
            }
            catch
            {
                highScoreLevel = -1;
                highScoreLevelText.GetComponent<TextMesh>().text = "---";
            }

            try
            {
                highScoreLevel = int.Parse(level);
                highScorePointsText.GetComponent<TextMesh>().text = points;
            }
            catch
            {
                highScoreLevel = -1;
                highScorePointsText.GetComponent<TextMesh>().text = "---";
            }
        }
        else
        {
            highScoreLevelText.GetComponent<TextMesh>().text = "---";
            highScorePointsText.GetComponent<TextMesh>().text = "---";
        }
    }
}
