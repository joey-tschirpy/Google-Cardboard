using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class MinesweeperController : MonoBehaviour {

    [SerializeField] GameObject resetButton9x9;
    [SerializeField] GameObject blockArray9x9;
    [SerializeField] GameObject board9x9;

    [SerializeField] GameObject resetButton16x16;
    [SerializeField] GameObject blockArray16x16;
    [SerializeField] GameObject board16x16;

    [SerializeField] GameObject resetButton30x16;
    [SerializeField] GameObject blockArray30x16;
    [SerializeField] GameObject board30x16;

    [SerializeField] Camera mainCam;
    [SerializeField] GameObject title;
    [SerializeField] GameObject gameSelectionOrbs;

    [SerializeField] GameObject highScoreEasy;
    [SerializeField] GameObject highScoreIntermediate;
    [SerializeField] GameObject highScoreExpert;

    private const float MOVE_SPEED = 0.1f;
    private const float X_POS_BOUND = 3.0f;
    private const float Z_ROT_BOUND = 0.2f;

    private Vector3 pos9Rows;
    private Vector3 pos16Rows;

    private string filepath;

    // Use this for initialization
    void Start () {
        pos9Rows = new Vector3(0.0f, 3.0f, -3.75f);
        pos16Rows = new Vector3(0.0f, 5.0f, -3.75f);

        filepath = Application.persistentDataPath + "/highScoresMinesweeper.txt";

        loadHighScores();
    }
	
	// Update is called once per frame
	void Update () {
        if (mainCam.transform.rotation.z < -Z_ROT_BOUND && mainCam.transform.position.x < X_POS_BOUND)
        {
            transform.Translate(Vector3.right * MOVE_SPEED);
        }
        else if (mainCam.transform.rotation.z > Z_ROT_BOUND && mainCam.transform.position.x > -X_POS_BOUND)
        {
            transform.Translate(Vector3.left * MOVE_SPEED);
        }
    }

    public void loadEasy(GameObject menuItem)
    {
        title.transform.position = resetButton9x9.transform.position + (Vector3.up * 2.0f);
        gameSelectionOrbs.transform.localPosition = menuItem.transform.localPosition;

        transform.position = pos9Rows;
        board9x9.GetComponent<MinesweeperGame>().ResetGame();

        set9x9(true);
        set16x16(false);
        set30x16(false);
    }

    public void loadIntermediate(GameObject menuItem)
    {
        title.transform.position = resetButton16x16.transform.position + (Vector3.up * 2.0f);
        gameSelectionOrbs.transform.localPosition = menuItem.transform.localPosition;

        transform.position = pos16Rows;
        board16x16.GetComponent<MinesweeperGame>().ResetGame();

        set9x9(false);
        set16x16(true);
        set30x16(false);


    }

    public void loadExpert(GameObject menuItem)
    {
        title.transform.position = resetButton30x16.transform.position + (Vector3.up * 2.0f);
        gameSelectionOrbs.transform.localPosition = menuItem.transform.localPosition;

        transform.position = pos16Rows;
        board30x16.GetComponent<MinesweeperGame>().ResetGame();

        set9x9(false);
        set16x16(false);
        set30x16(true);
    }

    private void set9x9(bool active)
    {
        resetButton9x9.SetActive(active);
        blockArray9x9.SetActive(active);
        board9x9.SetActive(active);
    }

    private void set16x16(bool active)
    {
        resetButton16x16.SetActive(active);
        blockArray16x16.SetActive(active);
        board16x16.SetActive(active);
    }

    private void set30x16(bool active)
    {
        resetButton30x16.SetActive(active);
        blockArray30x16.SetActive(active);
        board30x16.SetActive(active);
    }

    public void onHoverTextButton(GameObject textObj)
    {
        textObj.transform.localScale = textObj.transform.localScale * 1.2f;
        textObj.GetComponent<TextMesh>().color = new Color(0.9f, 0.9f, 0.9f);
    }

    public void onExitTextButton(GameObject textObj)
    {
        textObj.transform.localScale = textObj.transform.localScale / 1.2f;
        textObj.GetComponent<TextMesh>().color = new Color(0.8f, 0.0f, 0.0f);
    }

    public void saveHighScores()
    {
        StreamWriter writer = new StreamWriter(filepath, false);
        writer.WriteLine(highScoreEasy.GetComponent<TextMesh>().text);
        writer.WriteLine(highScoreIntermediate.GetComponent<TextMesh>().text);
        writer.WriteLine(highScoreExpert.GetComponent<TextMesh>().text);
        writer.Close();
    }

    public void loadHighScores()
    {
        if (File.Exists(filepath))
        {
            StreamReader reader = new StreamReader(filepath, false);
            highScoreEasy.GetComponent<TextMesh>().text = reader.ReadLine();
            highScoreIntermediate.GetComponent<TextMesh>().text = reader.ReadLine();
            highScoreExpert.GetComponent<TextMesh>().text = reader.ReadLine();
        }
        else
        {
            highScoreEasy.GetComponent<TextMesh>().text = "---";
            highScoreIntermediate.GetComponent<TextMesh>().text = "---";
            highScoreExpert.GetComponent<TextMesh>().text = "---";
        }
    }
}
