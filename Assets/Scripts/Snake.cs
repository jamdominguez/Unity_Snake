using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Snake : MonoBehaviour {

    public GameObject block;
    public GameObject sides;
    public GameObject itemPrefab;
    public int width, heigh;
    public Text ScoreText;
    public int itemPoints;
    public float explosionForce;

    private Queue<GameObject> snakeBody = new Queue<GameObject>();
    private GameObject snakeHead;
    private Vector3 snakeDirection = Vector3.right;
    private enum BoxType {
        EMPTY, OBSTACLE,
        ITEM
    }
    private BoxType[,] map;
    private GameObject item;
    private int score = 0;

    private void Awake() {
        map = new BoxType[width, heigh];
        BuildMap();

        float initX = width / 2;
        float initY = heigh / 2;
        //Build Snake
        for (int c = 10; c > 0; c--) {
            NewBlock(initX - c, initY);
        }
        snakeHead = NewBlock(initX, initY);

        CreateItem();

        StartCoroutine(Movement());
    }

    private IEnumerator Movement() {
        WaitForSeconds wait = new WaitForSeconds(0.15f);
        while (true) {
            Vector3 newPos = snakeHead.transform.position + snakeDirection; // calc snake direction
            BoxType boxType = GetMapValue(newPos.x, newPos.y);

            if (boxType == BoxType.EMPTY) { // move
                GameObject snakeBodyPart = snakeBody.Dequeue(); // dequeue the last body block
                SetMapValue(snakeBodyPart.transform.position.x, snakeBodyPart.transform.position.y, BoxType.EMPTY); //restore map value
                snakeBodyPart.transform.position = newPos; // change block position to the next snake position (head)
                snakeBody.Enqueue(snakeBodyPart); // add to the body again
                SetMapValue(snakeBodyPart.transform.position.x, snakeBodyPart.transform.position.y, BoxType.OBSTACLE); //change map value
                snakeHead = snakeBodyPart;
                yield return wait;
            } else if (boxType == BoxType.ITEM) { //add body blcok
                GameObject snakeBodyPart = NewBlock(newPos.x, newPos.y);
                snakeHead = snakeBodyPart;
                IncreaseScore(itemPoints);
                MoveItem();
                yield return wait;
            } else {
                Debug.Log("OBSTACLE!!!");
                GameOver();
                yield return new WaitForSeconds(3);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                yield break; // coorutine ends
            }

        }
    }

    private void GameOver() {
        // Take all rigidbody children from Snake (the blocks)
        DestroyBlocks(GetComponentsInChildren<Rigidbody>());
        DestroyBlocks(sides.GetComponentsInChildren<Rigidbody>());
    }

    private void DestroyBlocks(Rigidbody[] rigidbody) {
        foreach (Rigidbody rigi in rigidbody) {
            rigi.useGravity = true;
            rigi.AddForce(Random.insideUnitCircle.normalized * explosionForce);
            rigi.AddTorque(0, 0, Random.Range(-explosionForce, explosionForce));
        }
    }

    private void MoveItem() {
        Vector2Int newItemPos = GetEmptyPosition();
        item.transform.position = new Vector3(newItemPos.x, newItemPos.y);
        SetMapValue(newItemPos.x, newItemPos.y, BoxType.ITEM);
    }

    private void IncreaseScore(int points) {
        score += points;
        ScoreText.text = score.ToString();
        Debug.Log("score: " + score);
    }

    private void CreateItem() {
        Vector2Int pos = GetEmptyPosition();
        item = Instantiate(itemPrefab, new Vector3(pos.x, pos.y), Quaternion.identity, sides.transform); // the item position will be modified always the snake get it
        SetMapValue(pos.x, pos.y, BoxType.ITEM);
    }

    private Vector2Int GetEmptyPosition() {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();

        // all possible empty boxes
        for (int x = 1; x < width - 1; x++) {
            for (int y = 1; y < heigh - 1; y++) {
                if (GetMapValue(x, y) == BoxType.EMPTY) emptyPositions.Add(new Vector2Int(x, y));
            }
        }

        int randomPos = Random.Range(0, emptyPositions.Count);
        return emptyPositions[randomPos];
    }

    private void SetMapValue(float x, float y, BoxType boxValue) {
        map[Mathf.RoundToInt(x), Mathf.RoundToInt(y)] = boxValue;
    }

    private BoxType GetMapValue(float x, float y) {
        return map[Mathf.RoundToInt(x), Mathf.RoundToInt(y)];
    }

    private GameObject NewBlock(float x, float y) {
        Vector3 pos = new Vector3(x, y);
        GameObject newBlock = Instantiate(block, pos, Quaternion.identity, this.transform); // child of sanke, this script is in snake
        snakeBody.Enqueue(newBlock);
        SetMapValue(x, y, BoxType.OBSTACLE); //change map value

        return newBlock;
    }

    private void BuildMap() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < heigh; y++) {
                if (x == 0 || x == width - 1 || y == 0 || y == heigh - 1) {
                    Vector3 pos = new Vector3(x, y);
                    Instantiate(block, pos, Quaternion.identity, sides.transform); // child of sides
                    SetMapValue(x, y, BoxType.OBSTACLE); //change map value
                } else {
                    SetMapValue(x, y, BoxType.EMPTY); //change map value
                }
            }
        }
    }

    private void Update() {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 directionSelected = new Vector3(horizontal, vertical);

        if (directionSelected != Vector3.zero) snakeDirection = directionSelected;
    }
}
