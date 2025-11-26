using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class JigsawPuzzle : MonoBehaviour
{
    [Header("Game Elements")]
    [Range(2, 6)] // Range allowed for pieces (Gives slider in Inspector)
    [SerializeField] private int difficulty = 4; // Amount of pieces.
    [SerializeField] private Transform gameHolder;
    [SerializeField] private Transform piecePrefab;


    [Header("UI Elements")] // Groups them together.
    // SerializedField): Set in the inspecter.
    [SerializeField] private List<Texture2D> imageTexture; // Contains image textures
    [SerializeField] private Transform levelSelectPanal; // Instantiate things within panal
    [SerializeField] private Image levelSelectPrefab; // Help assign sprite
   // [SerializeField] private GameObject playAgainButton;

    private List<Transform> pieces;
    private Vector2Int dimensions;
    private float width;
    private float height;
    private Transform draggingPiece = null;
    private float mouseDownTime;
    private float heldTime;
    private int piecesCorrect;
    private bool completePuzzle;
    public const float HOLDTHRESHOLD = 0.15f;
    private float pieceRotation;

    int[] rotationPiece = {0, 90, 270};

    private Vector3 offset;
    void Start()
    {
        // Create the UI
        foreach (Texture2D texture in imageTexture)
        {
            Image image = Instantiate(levelSelectPrefab, levelSelectPanal);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            // Assign button action
            image.GetComponent<Button>().onClick.AddListener(delegate { StartGame(texture); });
        }
    }
    public void StartGame(Texture2D jigsawTexture)
    {
        // Hide UI
        levelSelectPanal.gameObject.SetActive(false);

        // Track puzzle pieces
        pieces = new List<Transform>();

        // Calculate piece size based on difficulty
        dimensions = GetDimensions(jigsawTexture, difficulty);

        CreateJigsawPieces(jigsawTexture);

        // Place pieces randomly across board
        Scatter();

        // Update the border to fit the puzzle
        UpdateBorder();

        piecesCorrect = 0;
    }


    private Vector2Int GetDimensions(Texture2D jigsawTexture, int difficulty)
    {
        Vector2Int dimensions = Vector2Int.zero;

        // Difficulty is the number of pieces on the smallest texture dimension
        if (jigsawTexture.width < jigsawTexture.height)
        {
            dimensions.x = difficulty;
            dimensions.y = (difficulty * jigsawTexture.height) / jigsawTexture.width;
        }
        else
        {
            dimensions.y = difficulty;
            dimensions.x = (difficulty * jigsawTexture.width) / jigsawTexture.height;
        }

        return dimensions;
    }

    // Create jigsaw pieces
    private void CreateJigsawPieces(Texture2D jigsawTexture)
    {
        // Calculate size of pieces based on dimensions
        height = 1f / dimensions.y;
        float aspect = (float)jigsawTexture.width / jigsawTexture.height;
        width = aspect / dimensions.x;

        for (int row = 0; row < dimensions.y; row++)
        {
            for (int col = 0; col < dimensions.x; col++)
            {
                // Create piece in the right location and of right size
                Transform piece = Instantiate(piecePrefab, gameHolder);
                piece.localPosition = new Vector3
                (
                    (-width * dimensions.x / 2) + (width * col) + (width / 2),
                    (-height * dimensions.y / 2) + (height * row) + (height / 2),
                    1f
                );
                piece.localScale = new Vector3(width, height, 1f);

                // name useful for debugging
                piece.name = $"Piece {(row * dimensions.x) + col}";
                pieces.Add(piece);

                // Assign the corredct part of the texture for this jigsaw
                // Need width and height to both be normalised between 0 and 1 for the uv
                float width1 = 1f / dimensions.x;
                float height1 = 1f / dimensions.y;

                // UV coord order is anti-clockwise: (0,0), (1,0), (0,1), (1, 1)
                Vector2[] uv = new Vector2[4];
                uv[0] = new Vector2(width1 * col, height1 * row);
                uv[1] = new Vector2(width1 * (col + 1), height * row);
                uv[2] = new Vector2(width1 * col, height1 * (row + 1));
                uv[3] = new Vector2(width1 * (col + 1), height1 * (row + 1));
                // Assign our new UVs to the mesh
                Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                mesh.uv = uv;
                // Update the texture on the piece
                piece.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", jigsawTexture);
            }
        }
    }

     private void Scatter()
    {
        // Calculate the visiable camera orthorgraphic size of the screen
        float orthoHeight = Camera.main.orthographicSize;
        float Screenaspect = (float)Screen.width / Screen.height;
        float orthoWidth = (Screenaspect * orthoHeight);

        // Pieces are away from the edges
        float pieceWidth = width * gameHolder.localScale.x;
        float pieceHeight = height * gameHolder.localScale.y;

        orthoHeight -= pieceHeight;
        orthoWidth -= pieceWidth;

        // Place each piece randomly in the visable screen
        foreach (Transform piece in pieces)
        {
            float x = UnityEngine.Random.Range(-orthoWidth, orthoWidth);
            float y = UnityEngine.Random.Range(-orthoHeight, orthoHeight);
            int indexRotate = UnityEngine.Random.Range(0, rotationPiece.Length);
            int newRotation = rotationPiece[indexRotate];
            quaternion rotation = Quaternion.Euler(0f, 0f, newRotation); // Random rotation

            piece.position = new Vector3(x, y, -1);
            piece.rotation = rotation;
        }
    }

    // Update the boarder to fit the puzzle
    private void UpdateBorder()
    {
        LineRenderer lineRenderer = gameHolder.GetComponent<LineRenderer>();

        // Get size of the puzzle (Get half the size to simplify)
        // width = width of individual puzzle piece
        // dimensions.x = how many pieces there are.
        float halfWidth = (width * dimensions.x) / 2f;
        float halfHeight = (height * dimensions.y) / 2f;

        // Put boarder behind the pieces
        float borderZ = 0f;

        // Set border verticies, starting top left, going clockwise
        lineRenderer.SetPosition(0, new Vector3(-halfWidth, halfHeight, borderZ));
        lineRenderer.SetPosition(1, new Vector3(halfWidth, halfHeight, borderZ));
        lineRenderer.SetPosition(2, new Vector3(halfWidth, -halfHeight, borderZ));
        lineRenderer.SetPosition(3, new Vector3(-halfWidth, -halfHeight, borderZ));

        // Set the thickness of the border line
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // Show the border line
        lineRenderer.enabled = true;

    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            mouseDownTime = Time.time;

            if(hit)
            {
                draggingPiece = hit.transform;
                offset = draggingPiece.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                offset += Vector3.back;
            }
        }
        // Stop dragging when mouse is released
        if(draggingPiece && Input.GetMouseButtonUp(0))
        {
            //TO BE CONTINUED
            heldTime = Time.time - mouseDownTime;

            if(heldTime < HOLDTHRESHOLD)
            {
                 draggingPiece.transform.Rotate(0f, 0f, 90f);
                 pieceRotation = draggingPiece.transform.rotation.z;
                 GetRotation(pieceRotation);
                 Debug.Log("Piece rotation: " + draggingPiece.transform.rotation.z);
            }

            SnapAndDisableIfCorrect();
            draggingPiece.position += Vector3.forward;
            draggingPiece = null;
        }

        // Set the dragging piece position to the mouse position
        if(draggingPiece)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            newPosition += offset;
            draggingPiece.position = newPosition;
        }

        if(piecesCorrect == pieces.Count)
        {
            completePuzzle = true;
            //Debug.Log("Puzzle Complete");
        }
    }

    private void SnapAndDisableIfCorrect()
    {
       // Know the index number of the piece to determine correct position
       int pieceIdex =  pieces.IndexOf(draggingPiece);

       // The coords of piece in the puzzle
       int col = pieceIdex % dimensions.x;
       int row = pieceIdex / dimensions.x;

       // target position in non-scaled coords
        Vector2 targetPosition = new((-width * dimensions.x / 2) + (width * col) + (width / 2),
                                     (-height * dimensions.y / 2) + (height * row) + (height / 2));
        
        // Check if correct location
        if(Vector2.Distance(draggingPiece.localPosition, targetPosition) < (width / 3) && draggingPiece.rotation.z == 0f)
        {
            // Snap to position
            draggingPiece.localPosition = targetPosition;

            draggingPiece.GetComponent<BoxCollider2D>().enabled = false;

            piecesCorrect++;
        }

    }

    private void GetRotation(float pieceRotation)
    {
        if(pieceRotation >= -10f && pieceRotation <= 10f)
        {
            draggingPiece.transform.Rotate(0f, 0f, pieceRotation * 0);
        }
  
    }

}
