using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    [Header("Board")]
    public Sprite boardSprite;
    public float  squareSize = 1f;

    [Header("Square Colors")]
    public Color lightColor     = new Color(0.941f, 0.851f, 0.710f);
    public Color darkColor      = new Color(0.710f, 0.533f, 0.388f);
    public Color selectedColor  = new Color(0.13f, 0.70f, 0.20f, 0.55f);
    public Color moveColor      = new Color(0.13f, 0.70f, 0.20f, 0.40f);
    public Color captureColor   = new Color(0.90f, 0.20f, 0.20f, 0.5f);
    public Color lastMoveColor  = new Color(0.13f, 0.70f, 0.20f, 0.35f);

    [Header("Coordinates")]
    public bool showCoordinates = true;
    public float coordinateFontSize = 2.8f;
    public Color coordinateColor = new Color(0.90f, 0.75f, 0.55f);

    [Header("White Pieces")]
    public Sprite whitePawn;
    public Sprite whiteKnight;
    public Sprite whiteBishop;
    public Sprite whiteRook;
    public Sprite whiteQueen;
    public Sprite whiteKing;

    [Header("Black Pieces")]
    public Sprite blackPawn;
    public Sprite blackKnight;
    public Sprite blackBishop;
    public Sprite blackRook;
    public Sprite blackQueen;
    public Sprite blackKing;

    [Header("Audio")]
    public AudioClip moveSound;
    public AudioClip captureSound;

    private AudioSource _audio;

    private GameObject[,]      _squareObjects = new GameObject[8, 8];
    private SpriteRenderer[,]  _squareHighlights = new SpriteRenderer[8, 8];
    private Dictionary<Square, GameObject> _pieceObjects = new Dictionary<Square, GameObject>();
    private Sprite _squareSprite;
    private GameObject _piecesParent;

    private Square _lastMoveFrom;
    private Square _lastMoveTo;
    private bool   _hasLastMove;

    public void Initialize(BoardModel model)
    {
        _audio = gameObject.GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 0f;

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        _pieceObjects.Clear();
        _hasLastMove = false;

        _squareSprite = CreateWhitePixelSprite();
        CreateBoardBackground();
        CreateSquares();
        if (showCoordinates) CreateCoordinateLabels();
        SpawnAllPieces(model);
    }

    public void SyncPieces(BoardModel model)
    {
        _pieceObjects.Clear();
        SpawnAllPieces(model);
    }

    public void ClearHighlights()
    {
        for (int f = 0; f < 8; f++)
            for (int r = 0; r < 8; r++)
                _squareHighlights[f, r].color = Color.clear;
    }

    public void HighlightSquare(Square sq, Color color)
    {
        if (!sq.IsValid) return;
        _squareHighlights[sq.File, sq.Rank].color = color;
    }

    public void HighlightSelected(Square sq)  => HighlightSquare(sq, selectedColor);
    public void HighlightMove(Square sq)       => HighlightSquare(sq, moveColor);
    public void HighlightCapture(Square sq)    => HighlightSquare(sq, captureColor);

    public void ShowLastMove(Square from, Square to)
    {
        if (_hasLastMove)
        {
            HighlightSquare(_lastMoveFrom, Color.clear);
            HighlightSquare(_lastMoveTo,   Color.clear);
        }
        _lastMoveFrom = from;
        _lastMoveTo   = to;
        _hasLastMove  = true;
        HighlightSquare(from, lastMoveColor);
        HighlightSquare(to,   lastMoveColor);
    }

    public void ClearLastMove()
    {
        if (_hasLastMove)
        {
            HighlightSquare(_lastMoveFrom, Color.clear);
            HighlightSquare(_lastMoveTo,   Color.clear);
        }
        _hasLastMove = false;
    }

    public Square WorldToSquare(Vector3 worldPos)
    {
        int file = Mathf.FloorToInt(worldPos.x / squareSize + 4f);
        int rank = Mathf.FloorToInt(worldPos.y / squareSize + 4f);
        return new Square(file, rank);
    }

    public Vector3 SquareToWorld(Square sq) =>
        new Vector3((sq.File - 3.5f) * squareSize, (sq.Rank - 3.5f) * squareSize, 0f);

    private void CreateCoordinateLabels()
    {
        var parent = new GameObject("CoordinateLabels");
        parent.transform.parent = transform;

        float half  = squareSize * 0.5f;
        float inset = squareSize * 0.06f;

        // File letters a–h: bottom
        for (int f = 0; f < 8; f++)
        {
            Vector3 pos = SquareToWorld(new Square(f, 0)) + new Vector3(0f, -half + inset, -0.1f);
            CreateLabel(parent.transform, $"FileBot_{(char)('a' + f)}", ((char)('a' + f)).ToString(), TextAlignmentOptions.Bottom, pos);
        }

        // File letters a–h: top
        for (int f = 0; f < 8; f++)
        {
            Vector3 pos = SquareToWorld(new Square(f, 7)) + new Vector3(0f, half - inset, -0.1f);
            CreateLabel(parent.transform, $"FileTop_{(char)('a' + f)}", ((char)('a' + f)).ToString(), TextAlignmentOptions.Top, pos);
        }

        // Rank numbers 1–8: left
        for (int r = 0; r < 8; r++)
        {
            Vector3 pos = SquareToWorld(new Square(0, r)) + new Vector3(-half + inset, 0f, -0.1f);
            CreateLabel(parent.transform, $"RankLeft_{r + 1}", (r + 1).ToString(), TextAlignmentOptions.MidlineLeft, pos);
        }

        // Rank numbers 1–8: right
        for (int r = 0; r < 8; r++)
        {
            Vector3 pos = SquareToWorld(new Square(7, r)) + new Vector3(half - inset, 0f, -0.1f);
            CreateLabel(parent.transform, $"RankRight_{r + 1}", (r + 1).ToString(), TextAlignmentOptions.MidlineRight, pos);
        }
    }

    private void CreateLabel(Transform parent, string name, string text, TextAlignmentOptions alignment, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.parent = parent;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text      = text;
        tmp.fontSize  = coordinateFontSize;
        tmp.color     = coordinateColor;
        tmp.alignment = alignment;
        tmp.GetComponent<MeshRenderer>().sortingOrder = 3;
        go.transform.position = position;
        var rt = go.GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = Vector2.one * squareSize;
    }

    private void CreateBoardBackground()
    {
        if (boardSprite == null) return;

        var bg = new GameObject("BoardBackground");
        bg.transform.parent = transform;
        bg.transform.position = Vector3.zero;
        bg.transform.localPosition = Vector3.zero;

        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite       = boardSprite;
        sr.sortingOrder = 0;

        float targetSize  = squareSize * 8f;
        float spriteSize  = boardSprite.bounds.size.x;
        bg.transform.localScale = Vector3.one * (targetSize / spriteSize);
    }

    private void CreateSquares()
    {
        var squaresParent = new GameObject("Squares");
        squaresParent.transform.parent = transform;

        for (int f = 0; f < 8; f++)
        {
            for (int r = 0; r < 8; r++)
            {
                var sq = new GameObject($"Sq_{f}_{r}");
                sq.transform.parent   = squaresParent.transform;
                sq.transform.position = SquareToWorld(new Square(f, r));

                var sr = sq.AddComponent<SpriteRenderer>();
                sr.sprite       = _squareSprite;
                sr.color        = boardSprite == null
                    ? ((f + r) % 2 == 0 ? darkColor : lightColor)
                    : Color.clear;
                sr.sortingOrder = 1;
                sr.transform.localScale = Vector3.one * squareSize;

                _squareObjects[f, r]    = sq;
                _squareHighlights[f, r] = sr;
            }
        }
    }

    private void SpawnAllPieces(BoardModel model)
    {
        if (_piecesParent != null)
        {
            _piecesParent.SetActive(false); 
            UnityEngine.Object.Destroy(_piecesParent);
        }
        _piecesParent = new GameObject("Pieces");
        _piecesParent.transform.parent = transform;

        for (int f = 0; f < 8; f++)
        {
            for (int r = 0; r < 8; r++)
            {
                var sq    = new Square(f, r);
                var piece = model.GetPiece(sq);
                if (!piece.IsEmpty)
                    SpawnPiece(sq, piece, _piecesParent.transform);
            }
        }
    }

    private struct TightBounds
    {
        public float height;
        public float bottom;

        public TightBounds(float height, float bottom)
        {
            this.height = height;
            this.bottom = bottom;
        }
    }

    private TightBounds GetTightBounds(PieceType type, PieceColor color)
    {
        if (color == PieceColor.White)
        {
            return type switch
            {
                PieceType.Pawn   => new TightBounds(287f, 107f),
                PieceType.Knight => new TightBounds(294f, 103f),
                PieceType.Bishop => new TightBounds(355f, 73f),
                PieceType.Rook   => new TightBounds(320f, 90f),
                PieceType.Queen  => new TightBounds(339f, 90f),
                PieceType.King   => new TightBounds(439f, 50f),
                _                => new TightBounds(500f, 0f)
            };
        }
        else
        {
            return type switch
            {
                PieceType.Pawn   => new TightBounds(287f, 107f),
                PieceType.Knight => new TightBounds(294f, 103f),
                PieceType.Bishop => new TightBounds(355f, 73f),
                PieceType.Rook   => new TightBounds(320f, 90f),
                PieceType.Queen  => new TightBounds(339f, 81f),
                PieceType.King   => new TightBounds(439f, 50f),
                _                => new TightBounds(500f, 0f)
            };
        }
    }

    private float GetTargetFigureHeight(PieceType type) => type switch
    {
        PieceType.Pawn   => 0.82f,
        PieceType.Rook   => 0.98f,
        PieceType.Knight => 1.08f,
        PieceType.Bishop => 1.15f,
        PieceType.Queen  => 1.32f,
        PieceType.King   => 1.50f,
        _                => 1.00f
    };

    private void SetupPieceTransform(GameObject go, Square sq, PieceType type, PieceColor color)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        Sprite sprite = sr.sprite;
        float targetHeightFactor = GetTargetFigureHeight(type);

        float H_world = targetHeightFactor * squareSize;

        float localHeight = sprite.rect.height / sprite.pixelsPerUnit;
        if (localHeight <= 0f) localHeight = 1f;

        float scale = H_world / localHeight;
        go.transform.localScale = Vector3.one * scale;

        float targetBottomY = -0.42f * squareSize;
        float yOffset = targetBottomY + (H_world / 2f);

        Vector3 squarePos = SquareToWorld(sq);
        go.transform.position = new Vector3(squarePos.x, squarePos.y + yOffset, squarePos.z);

        sr.sortingOrder = 10 - sq.Rank;
    }

    private void SpawnPiece(Square sq, PieceData piece, Transform parent)
    {
        Sprite sprite = GetSprite(piece.Type, piece.Color);
        if (sprite == null) return;

        var go = new GameObject($"{piece.Color}_{piece.Type}_{sq}");
        go.transform.parent   = parent;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = 2;

        SetupPieceTransform(go, sq, piece.Type, piece.Color);

        _pieceObjects[sq] = go;
    }

    public void MovePieceVisual(Square from, Square to)
    {
        if (!_pieceObjects.TryGetValue(from, out var go)) return;

        bool isCapture = _pieceObjects.TryGetValue(to, out var captured);
        if (isCapture)
        {
            Destroy(captured);
            _pieceObjects.Remove(to);
        }

        AudioClip clip = (isCapture && captureSound != null) ? captureSound : moveSound;
        if (_audio != null && clip != null)
            _audio.PlayOneShot(clip);

        // Preserve current Y offset of piece
        float currentYOffset = go.transform.position.y - SquareToWorld(from).y;
        Vector3 targetPos = SquareToWorld(to);
        targetPos.y += currentYOffset;
        go.transform.position = targetPos;

        // Dynamically update sorting order based on new rank
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 10 - to.Rank;
        }

        _pieceObjects.Remove(from);
        _pieceObjects[to] = go;
    }

    public void RefreshPieceSprite(Square sq, PieceData piece)
    {
        if (!_pieceObjects.TryGetValue(sq, out var go)) return;
        go.GetComponent<SpriteRenderer>().sprite = GetSprite(piece.Type, piece.Color);
        SetupPieceTransform(go, sq, piece.Type, piece.Color);
    }

    private Sprite GetSprite(PieceType type, PieceColor color) => (type, color) switch
    {
        (PieceType.Pawn,   PieceColor.White) => whitePawn,
        (PieceType.Knight, PieceColor.White) => whiteKnight,
        (PieceType.Bishop, PieceColor.White) => whiteBishop,
        (PieceType.Rook,   PieceColor.White) => whiteRook,
        (PieceType.Queen,  PieceColor.White) => whiteQueen,
        (PieceType.King,   PieceColor.White) => whiteKing,
        (PieceType.Pawn,   PieceColor.Black) => blackPawn,
        (PieceType.Knight, PieceColor.Black) => blackKnight,
        (PieceType.Bishop, PieceColor.Black) => blackBishop,
        (PieceType.Rook,   PieceColor.Black) => blackRook,
        (PieceType.Queen,  PieceColor.Black) => blackQueen,
        (PieceType.King,   PieceColor.Black) => blackKing,
        _                                    => null
    };

    private Sprite CreateWhitePixelSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
    }
}
