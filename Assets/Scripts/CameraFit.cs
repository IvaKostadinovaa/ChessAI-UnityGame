using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    private float _lastAspect = 0f;
    private Vector3 _lastBoardBoundsCenter;
    private Vector3 _lastBoardBoundsSize;

    void LateUpdate()
    {
        SpriteRenderer board = FindBoard();
        if (board == null) return;

        Camera cam = GetComponent<Camera>();
        if (cam.aspect != _lastAspect || 
            board.bounds.center != _lastBoardBoundsCenter || 
            board.bounds.size != _lastBoardBoundsSize)
        {
            FitCameraToBoard(cam, board);
        }
    }

    private void FitCameraToBoard(Camera cam, SpriteRenderer board)
    {
        float boardHeight = board.bounds.size.y;
        float boardWidth  = board.bounds.size.x;

        // 1. Locate HUD Panels and compute their actual screen width in pixels
        float leftWidth = 0f;
        float rightWidth = 0f;
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            Transform leftPanel = canvas.transform.Find("GameplayUI/LeftSidePanel") ?? canvas.transform.Find("LeftSidePanel");
            Transform rightPanel = canvas.transform.Find("GameplayUI/RightSidePanel") ?? canvas.transform.Find("RightSidePanel");
            
            if (leftPanel != null && leftPanel.gameObject.activeInHierarchy)
            {
                var rt = leftPanel.GetComponent<RectTransform>();
                if (rt != null) leftWidth = rt.rect.width * canvas.scaleFactor;
            }
            if (rightPanel != null && rightPanel.gameObject.activeInHierarchy)
            {
                var rt = rightPanel.GetComponent<RectTransform>();
                if (rt != null) rightWidth = rt.rect.width * canvas.scaleFactor;
            }
        }

        // 2. Calculate available fraction of the screen width
        float totalPanelWidth = leftWidth + rightWidth;
        float availableWidthFraction = 1f;
        if (Screen.width > totalPanelWidth + 50f)
        {
            availableWidthFraction = (Screen.width - totalPanelWidth) / Screen.width;
        }
        else
        {
            availableWidthFraction = 0.2f; // Fallback safety minimum
        }

        // 3. Compute orthographic size
        float heightSize = boardHeight / 2f;
        float widthSize  = (boardWidth / 2f) / (cam.aspect * availableWidthFraction);

        float topPadding    = 0.8f;
        float bottomPadding = 0.8f;

        // Ensure both board height and width fit properly in the un-covered camera view
        float targetSize = Mathf.Max(heightSize, widthSize) + (topPadding + bottomPadding) / 2f;
        cam.orthographicSize = targetSize;

        // 4. Compute horizontal offset to center the board in the gap
        float pixelOffset = (leftWidth - rightWidth) / 2f;
        float pixelOffsetFraction = pixelOffset / Screen.width;
        float worldOffsetX = pixelOffsetFraction * (2f * cam.orthographicSize * cam.aspect);

        float yOffset = (topPadding - bottomPadding) / 2f;
        transform.position = new Vector3(board.bounds.center.x + worldOffsetX, board.bounds.center.y + yOffset, -10f);

        _lastAspect = cam.aspect;
        _lastBoardBoundsCenter = board.bounds.center;
        _lastBoardBoundsSize = board.bounds.size;
    }

    SpriteRenderer FindBoard()
    {
        foreach (SpriteRenderer sr in FindObjectsByType<SpriteRenderer>())
            if (sr.sprite != null && sr.sprite.name.ToLower().Contains("board"))
                return sr;
        return null;
    }
}
