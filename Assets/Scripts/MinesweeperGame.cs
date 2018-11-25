using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinesweeperGame : MonoBehaviour {

    [SerializeField] Material normalBlock;
    [SerializeField] Material highlightBlock;

    [SerializeField] Material one;
    [SerializeField] Material two;
    [SerializeField] Material three;
    [SerializeField] Material four;
    [SerializeField] Material five;
    [SerializeField] Material six;
    [SerializeField] Material seven;
    [SerializeField] Material eight;

    [SerializeField] Material empty;
    [SerializeField] Material border;
    [SerializeField] Material smiley;
    [SerializeField] Material smileyWin;
    [SerializeField] Material smileyLose;
    [SerializeField] Material mine;
    [SerializeField] Material mineHit;
    [SerializeField] Material flag;

    [SerializeField] GameObject blockArrayObj;
    [SerializeField] GameObject resetBlock;

    [SerializeField] GameObject minesText;
    [SerializeField] GameObject timerText;
    [SerializeField] GameObject highScoreText;

    [SerializeField] GameObject player;

    private const float FLAG_THRESHOLD = 0.2f;

    public int rows;
    public int cols;
    public int maxMines;

    private bool first;
    private bool playing;
    private int[] blocks;
    private bool[] blocksRemoved;
    private int blocksRemaining;
    private int clickIndex;
    private int mines;
    private int[] mineIndices;
    private float timer;
    private float flagTimer;
    private bool[] blocksFlagged;
    private int highScore;
    

    // Use this for initialization
    void Start () {
        first = true;
        playing = false;

        resetTimer();
        resetMineInfo();

        blocks = new int[rows * cols];
        blocksRemoved = new bool[rows * cols];
        blocksRemaining = rows * cols - maxMines;
        blocksFlagged = new bool[rows * cols];
        mineIndices = new int[maxMines];

        for (int i = 0; i < blocksFlagged.Length; i++)
        {
            blocksFlagged[i] = false;
        }

        try
        {
            highScore = int.Parse(highScoreText.GetComponent<TextMesh>().text);
        }
        catch
        {
            highScore = -1;
        }
    }
	
	// Update is called once per frame
	void Update () {
        float deltaTime = Time.deltaTime;

        if (playing)
        {
            timer += deltaTime;
            if (timer > 999)
            {
                timerText.GetComponent<TextMesh>().text = "999";
            }
            else
            {
                timerText.GetComponent<TextMesh>().text = ((int)timer).ToString("000");
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            flagTimer = 0.0f;
        }

        if (Input.GetMouseButton(0))
        {
            flagTimer += deltaTime;
        }
	}

    public void blockOnHover(GameObject block)
    {
        int blockIndex = int.Parse(block.transform.parent.name);
        if (blocksFlagged[blockIndex])
        {
            return;
        }

        Material[] mats = new Material[block.GetComponent<MeshRenderer>().materials.Length];

        mats[0] = highlightBlock;
        mats[1] = highlightBlock;
        mats[2] = highlightBlock;

        block.GetComponent<MeshRenderer>().materials = mats;
    }

    public void blockExitHover(GameObject block)
    {
        int blockIndex = int.Parse(block.transform.parent.name);

        if (block.GetComponent<BoxCollider>().enabled && !blocksFlagged[blockIndex])
        {
            resetBlockMat(block);
        }
    }

    private void resetBlockMat(GameObject block)
    {
        Material[] mats = new Material[block.GetComponent<MeshRenderer>().materials.Length];

        mats[0] = normalBlock;
        mats[1] = normalBlock;
        mats[2] = normalBlock;

        block.GetComponent<MeshRenderer>().materials = mats;
    }

    public void ResetGame()
    {
        if (first) return;
        
        resetBlock.GetComponent<Animator>().SetTrigger("Press");

        first = true;
        playing = false;

        resetTimer();
        resetMineInfo();

        blocksRemaining = rows * cols - maxMines;
        blocksFlagged = new bool[rows * cols];

        for (int i = 0; i < blockArrayObj.transform.childCount; i++)
        {
            GameObject block = blockArrayObj.transform.GetChild(i).GetChild(0).gameObject;

            Animator anim = block.GetComponent<Animator>();
            anim.ResetTrigger("Remove");
            anim.SetTrigger("Reset");
            resetBlockMat(block);

            block.GetComponent<BoxCollider>().enabled = true;

            blocksFlagged[i] = false;
        }

        resetBlockFaceChange(smiley);
    }

    public void clickBlock(GameObject block)
    {
        clickIndex = int.Parse(block.transform.parent.name);

        if (flagTimer > FLAG_THRESHOLD)
        {
            if (blocksFlagged[clickIndex])
            {
                faceChange(block, normalBlock);
                blocksFlagged[clickIndex] = false;
                mines++;
            }
            else
            {
                faceChange(block, flag);
                blocksFlagged[clickIndex] = true;
                mines--;
            }

            minesText.GetComponent<TextMesh>().text = mines.ToString("000");

            return;
        }

        if (blocksFlagged[clickIndex])
        {
            return;
        }

        if (first)
        {
            RandomiseBoard(clickIndex);
            first = false;
            playing = true;
        }

        removeBlock(clickIndex);
    }

    private void RandomiseBoard(int firstIndex)
    {
        // Set array to 0 (no mines)
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i] = 0;
            blocksRemoved[i] = false;
        }

        // Randomise mine locations
        var rand = new System.Random();
        for (int i = 0; i < maxMines; i++)
        {
            // Mine index
            int mineIndex;
            do
            {
                mineIndex = rand.Next(0, rows * cols);

            } while (mineIndex == firstIndex || blocks[mineIndex] < 0);


            blocks[mineIndex] = -1;
            mineIndices[i] = mineIndex;


            // Add 1 to adjacent blocks if no mines there and within bounds of the board
            bool top = mineIndex < cols;
            bool right = mineIndex % cols == cols - 1;
            bool bottom = mineIndex >= (rows - 1) * cols;
            bool left = mineIndex % cols == 0;

            // Top block
            if (!top && blocks[mineIndex - cols] >= 0)
                blocks[mineIndex - cols]++;
            
            // Top-Right block
            if (!top && !right && blocks[mineIndex - cols + 1] >= 0)
                blocks[mineIndex - cols + 1]++;

            // Right block
            if (!right && blocks[mineIndex + 1] >= 0)
                blocks[mineIndex + 1]++;

            // Bottom-Right block
            if (!bottom && !right && blocks[mineIndex + cols + 1] >= 0)
                blocks[mineIndex + cols + 1]++;

            // Bottom block
            if (!bottom && blocks[mineIndex + cols] >= 0)
                blocks[mineIndex + cols]++;

            // Bottom-Left block
            if (!bottom && !left && blocks[mineIndex + cols - 1] >= 0)
                blocks[mineIndex + cols - 1]++;

            // Left block
            if (!left && blocks[mineIndex - 1] >= 0)
                blocks[mineIndex - 1]++;

            // Top-Left block
            if (!top && !left && blocks[mineIndex - cols - 1] >= 0)
                blocks[mineIndex - cols - 1]++;
        }
    }

    private void removeBlock(int blockIndex)
    {
        if (blocksFlagged[blockIndex])
        {
            blocksFlagged[blockIndex] = false;
            mines++;
            minesText.GetComponent<TextMesh>().text = mines.ToString("000");
        }

        blocksRemoved[blockIndex] = true;
        blocksRemaining--;

        GameObject block = blockArrayObj.transform.GetChild(blockIndex).GetChild(0).gameObject;

        Animator anim = block.GetComponent<Animator>();
        anim.ResetTrigger("Reset");
        anim.SetTrigger("Remove");

        block.GetComponent<BoxCollider>().enabled = false;

        Material[] mats = new Material[block.GetComponent<MeshRenderer>().materials.Length];

        mats[0] = normalBlock;
        mats[2] = border;

        if (blocks[blockIndex] < 0) // hit mine
        {
            for (int i = 0; i < maxMines; i++)
            {
                if (mineIndices[i] != blockIndex)
                {
                    GameObject mineBlock = blockArrayObj.transform.GetChild(mineIndices[i]).GetChild(0).gameObject;

                    Animator animMine = mineBlock.GetComponent<Animator>();
                    animMine.ResetTrigger("Reset");
                    animMine.SetTrigger("Remove");

                    mats[1] = mine;
                    mineBlock.GetComponent<MeshRenderer>().materials = mats;
                }
            }

            mats[1] = mineHit;
            resetBlockFaceChange(smileyLose);
            finishGame();
        }
        else if (blocks[blockIndex] > 0) // adjacent to mine
        {
            switch (blocks[blockIndex])
            {
                case 1:
                    mats[1] = one;
                    break;
                case 2:
                    mats[1] = two;
                    break;
                case 3:
                    mats[1] = three;
                    break;
                case 4:
                    mats[1] = four;
                    break;
                case 5:
                    mats[1] = five;
                    break;
                case 6:
                    mats[1] = six;
                    break;
                case 7:
                    mats[1] = seven;
                    break;
                case 8:
                    mats[1] = eight;
                    break;
            }
        }
        else if (blocks[blockIndex] == 0) // Recursively remove blocks if empty space
        {
            // Check if within bounds of the board
            bool top = blockIndex < cols;
            bool right = blockIndex % cols == cols - 1;
            bool bottom = blockIndex >= (rows - 1) * cols;
            bool left = blockIndex % cols == 0;

            mats[1] = empty;

            // Top block
            if (!top && !blocksRemoved[blockIndex - cols])
            {
                removeBlock(blockIndex - cols);
            }

            // Top-Right block
            if (!top && !right && !blocksRemoved[blockIndex - cols + 1])
            {
                removeBlock(blockIndex - cols + 1);
            }

            // Right block
            if (!right && !blocksRemoved[blockIndex + 1])
            {
                removeBlock(blockIndex + 1);
            }

            // Bottom-Right block
            if (!bottom && !right && !blocksRemoved[blockIndex + cols + 1])
            {
                removeBlock(blockIndex + cols + 1);
            }

            // Bottom block
            if (!bottom && !blocksRemoved[blockIndex + cols])
            {
                removeBlock(blockIndex + cols);
            }

            // Bottom-Left block
            if (!bottom && !left && !blocksRemoved[blockIndex + cols - 1])
            {
                removeBlock(blockIndex + cols - 1);
            }

            // Left block
            if (!left && !blocksRemoved[blockIndex - 1])
            {
                removeBlock(blockIndex - 1);
            }

            // Top-Left block
            if (!top && !left && !blocksRemoved[blockIndex - cols - 1])
            {
                removeBlock(blockIndex - cols - 1);
            }
        }

        block.GetComponent<MeshRenderer>().materials = mats;

        if (blocksRemaining <= 0)
        {
            winGame();
        }
    }

    private void resetMineInfo()
    {
        mines = maxMines;
        minesText.GetComponent<TextMesh>().text = maxMines.ToString("000");
    }

    private void resetTimer()
    {
        timer = 0;
        timerText.GetComponent<TextMesh>().text = "000";
    }

    private void resetBlockFaceChange(Material faceMat)
    {
        Material[] mats = new Material[resetBlock.GetComponent<MeshRenderer>().materials.Length];
        Debug.Log(faceMat.name);
        mats[0] = normalBlock;
        mats[1] = faceMat;
        mats[2] = normalBlock;

        resetBlock.GetComponent<MeshRenderer>().materials = mats;
    }

    private void finishGame()
    {
        BoxCollider[] boxColArray = blockArrayObj.GetComponentsInChildren<BoxCollider>();
        for (int i = 0; i < boxColArray.Length; i++)
        {
            boxColArray[i].enabled = false;
        }

        playing = false;
    }

    private void winGame()
    {
        resetBlockFaceChange(smileyWin);
        finishGame();

        for (int i = 0; i < mineIndices.Length; i++)
        {
            GameObject mineBlock = blockArrayObj.transform.GetChild(mineIndices[i]).GetChild(0).gameObject;
            faceChange(mineBlock, flag);
        }

        if (highScore < 0 || timer < highScore)
        {
            highScore = (int)timer;
            highScoreText.GetComponent<TextMesh>().text = highScore.ToString("000");

            player.GetComponent<MinesweeperController>().saveHighScores();
        }
    }

    private void faceChange(GameObject block, Material faceMat)
    {
        Material[] mats = new Material[block.GetComponent<MeshRenderer>().materials.Length];

        mats[0] = normalBlock;
        mats[1] = faceMat;
        mats[2] = border;

        block.GetComponent<MeshRenderer>().materials = mats;
    }
}
