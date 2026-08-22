using UnityEngine;
using UnityEngine.EventSystems; // Required for standard hover detection

// Implementing the interfaces allows Unity to call the hover events automatically
public class HoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Type the text you want to show on hover here.")]
    public string tooltipMessage = "Your text here"; 
    
    // Adjust this to change the text color in the Inspector
    public Color textColor = Color.white;

    private bool isHovering = false;

    // Called automatically when the mouse enters the UI element
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    // Called automatically when the mouse leaves the UI element
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    // OnGUI is called every frame to draw legacy GUI elements
    void OnGUI()
    {
        if (isHovering)
        {
            // Get the current mouse position in screen space
            Vector2 mousePos = Event.current.mousePosition;

            // Define where the text should be drawn (offset slightly from the cursor)
            // Rect(x, y, width, height)
            Rect textRect = new Rect(mousePos.x - 50, mousePos.y + 30, 30, 70);

            // Create a custom style to show ONLY text (no background box)
            GUIStyle style = new GUIStyle();
            style.normal.textColor = textColor;
            style.fontSize = 24; 
            style.fontStyle = FontStyle.Bold;

            // Draw the text directly to the screen
            GUI.Label(textRect, tooltipMessage, style);
        }
    }
}